using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire.Layer
{
    public class TransportLayer : IDisposable
    {
        private bool _disposed;

        public OneWirePort Port { get; set; }
        protected IOneWireTransport Driver { get; set; }

        public TransportLayer(OneWirePort port, IOneWireTransport driver)
        {
            Port = port;
            Driver = driver;
        }

        /// <see cref="IOneWireTransport.TransferBlock"/>
        [CanBeNull]
        public virtual byte[] TransferBlock(bool resetFirst, [NotNull] IList<byte> bytes)
        {
            return Driver.TransferBlock(Port.PortNumber, resetFirst, bytes);
        }

        /// <summary>
        /// Reads memory of a given bank.
        /// </summary>
        /// <remarks>
        /// This method does not perform any error checking. Some data includes a CRC value, e.g. that read with
        /// <see cref="ReadPagePacket"/>. Some devices provide their own CRC, e.g. those read with
        /// <see cref="ReadPageCRC"/>, but this must be verified using <see cref="PageHasAutoCRC"/>. Otherwise, this
        /// method should be called multiple times to ensure that the data has been read correctly.
        /// </remarks>
        /// <param name="bank">The memory bank from which to read.</param>
        /// <param name="serialNumber">The serial number of the 1-Wire device from which to read.</param>
        /// <param name="startAddress">The address at which to start the read.</param>
        /// <param name="continuedRead">Whether the previous read should simply be continued.</param>
        /// <param name="readLength">The number of bytes to read.</param>
        public virtual byte[] Read(byte bank, ulong serialNumber, int startAddress, bool continuedRead, int readLength)
        {
            var deviceClass = (byte)(serialNumber & 0x7F);
            switch (deviceClass)
            {
                case 0x14:
                    // EEPROM Memory Bank, Application Register Memory Bank

                    if (bank > 0)
                    {
                        return GenericRead(
                            "EEPROM", (b, f) => 32, 0xF0, 1, false, false,
                            bank, serialNumber, startAddress, continuedRead, readLength
                        );
                    }
                    return GenericRead(
                        "application register", (b, f) => 8, 0xC3, 1, false, false,
                        bank, serialNumber, startAddress, continuedRead, readLength
                    );

                case 0x04:
                case 0x06:
                case 0x08:
                case 0x0A:
                case 0x0C:
                case 0x23:
                    // Non-Volatile Memory Bank, Scratchpad Memory Bank
                case 0x1A:
                case 0x1D:
                    // Non-Volatile Cyclic Redundancy Check Memory Bank, Scratchpad Ex(???) Memory Bank

                    if (bank > 0)
                    {
                        return GenericRead(
                            "non-volatile memory", GetNonVolatileMemorySize, 0xF0, 2, true, false,
                            bank, serialNumber, startAddress, continuedRead, readLength
                        );
                    }
                    byte[] fullScratchpad = GenericRead(
                        "scratchpad", GetScratchMemorySize, 0xAA, 3, false, false,
                        bank, serialNumber, 0xFFFFFF, false, startAddress + readLength
                    );
                    return fullScratchpad.Skip(startAddress).ToArray();

                case 0x18:
                    // Non-Volatile Memory Bank, Scratchpad SHA Memory Bank
                case 0x21:
                    // Non-Volatile Cyclic Redundancy Check Memory Bank, Scratchpad Cyclic Redundancy Check Memory Bank

                    if (bank > 0)
                    {
                        return GenericRead(
                            "non-volatile memory", GetNonVolatileMemorySize, 0xF0, 2, true, false,
                            bank, serialNumber, startAddress, continuedRead, readLength
                        );
                    }
                    byte[] fullScratchpadExtra = GenericRead(
                        "scratchpad with SHA or CRC", GetNonVolatileMemorySize, 0xAA, 3, false, true,
                        bank, serialNumber, 0xFFFFFF, false, readLength
                    );
                    // TODO: extract extra data
                    // TODO: trim and verify CRC
                    throw new NotImplementedException();

                case 0x33:
                case 0xB3:
                    // SHA EEPROM Memory Bank
                    return GenericRead(
                        "SHA EEPROM", GetSHAEEPROMSize, 0xF0, 2, true, false,
                        bank, serialNumber, startAddress + GetSHAEEStartingAddress(bank, serialNumber), continuedRead, readLength
                    );

                case 0x2D:
                    // EEPROM with Write Protection Memory Bank
                    return GenericRead(
                        "EEPROM with write protection", GetEEPROMWithWriteProtectionSize, 0xF0, 2, true, false,
                        bank, serialNumber, startAddress + GetEEPROMWithWriteProtectionStartingAddress(bank, serialNumber), continuedRead, readLength
                    );

                case 0x09:
                case 0x0B:
                case 0x0F:
                case 0x12:
                case 0x13:
                    // EPROM Memory Bank
                    throw new NotImplementedException();
                    //return ReadEPROM(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x37:
                case 0x77:
                    // Password-Protected EEPROM Memory Bank, Scratchpad Cyclic Redundancy Check 0x77 Memory Bank

                    if (bank > 0)
                    {
                        throw new NotImplementedException();
                        //return ReadEEPROMPasswordProtected(bank, serialNumber, startAddress, continuedRead, readLength);
                    }

                    byte[] extra;
                    throw new NotImplementedException();
                    //return ReadScratchpadCRC77(serialNumber, readLength, out extra);
            }

            // unknown value
            throw new NotImplementedException(string.Format("unknown device class 0x{0:X2}", deviceClass));
        }

        protected virtual byte[] GenericRead(
            string memoryTypeDescription,
            Func<byte, ulong, int> bankAndFamilyToMaxSize,
            byte readCommand,
            int addressByteCount,
            bool allowContinuedRead,
            bool returnAddressResponseToo,

            byte bank,
            ulong serialNumber,
            int startAddress,
            bool continuedRead,
            int readLength
        )
        {
            if (addressByteCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(addressByteCount));
            }

            var operation = $"reading {memoryTypeDescription}";

            if (startAddress + readLength > bankAndFamilyToMaxSize(bank, (byte)(serialNumber & 0xFF)))
            {
                throw OneWireOperationException.Create(
                    operation,
                    ErrorCode.ReadOutOfRange
                );
            }

            var ret = new List<byte>(readLength);

            if (!allowContinuedRead || !continuedRead || returnAddressResponseToo)
            {
                Port.Network.SerialNumber = serialNumber;

                if (!Port.Network.AccessDevice())
                {
                    throw OneWireOperationException.Create(
                        operation,
                        ErrorCode.DeviceSelectFail
                    );
                }

                var addrSendBuf = new byte[addressByteCount + 1];
                addrSendBuf[0] = readCommand;
                addrSendBuf[1] = (byte) (startAddress & 0xFF);
                if (addressByteCount > 1)
                {
                    addrSendBuf[2] = (byte) ((startAddress >> 8) & 0xFF);
                }
                if (addressByteCount > 2)
                {
                    addrSendBuf[3] = (byte) ((startAddress >> 16) & 0xFF);
                }
                if (addressByteCount > 3)
                {
                    addrSendBuf[4] = (byte) ((startAddress >> 24) & 0xFF);
                }

                byte[] addrRecvBuf = TransferBlock(false, addrSendBuf);
                if (addrRecvBuf == null)
                {
                    throw OneWireOperationException.Create(
                        operation,
                        ErrorCode.BlockFailed
                    );
                }

                ret.Capacity = readLength + addressByteCount;
                ret.AddRange(addrSendBuf.Skip(1));
            }

            byte[] readRequestBuf = Enumerable.Repeat((byte)0xFF, readLength).ToArray();
            byte[] readBuf = TransferBlock(false, readRequestBuf);
            if (readBuf == null)
            {
                throw OneWireOperationException.Create(
                    operation,
                    ErrorCode.BlockFailed
                );
            }

            ret.AddRange(readBuf);
            return ret.ToArray();
        }

        [Pure]
        protected static byte SerialToFamilyNumber(ulong serialNumber)
        {
            return (byte) (serialNumber & 0x7F);
        }

        protected virtual int GetNonVolatileMemorySize(byte bank, ulong serialNumber)
        {
            byte familyNumber = SerialToFamilyNumber(serialNumber);
            switch (familyNumber)
            {
                // TODO
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual int GetScratchMemorySize(byte bank, ulong serialNumber)
        {
            byte familyNumber = SerialToFamilyNumber(serialNumber);
            switch (familyNumber)
            {
                // TODO
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual int GetSHAEEPROMSize(byte bank, ulong serialNumber)
        {
            // pageLength * getNumeberPagesSHAEE(bank, SNum[0])
            const int pageLength = 32;

            byte familyNumber = SerialToFamilyNumber(serialNumber);
            switch (familyNumber)
            {
                // TODO
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual int GetSHAEEStartingAddress(byte bank, ulong serialNumber)
        {
            // TODO
            throw new NotImplementedException();
        }

        protected virtual int GetEEPROMWithWriteProtectionSize(byte bank, ulong serialNumber)
        {
            // pageLength * getNumeberPagesSHAEE(bank, SNum[0])
            const int pageLength = 32;

            byte familyNumber = SerialToFamilyNumber(serialNumber);
            switch (familyNumber)
            {
                // TODO
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual int GetEEPROMWithWriteProtectionStartingAddress(byte bank, ulong serialNumber)
        {
            // TODO
            throw new NotImplementedException();
        }

        #region disposal logic
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // release managed resources
            }

            // release unmanaged resources

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TransportLayer()
        {
            Dispose(false);
        }
        #endregion
    }
}
