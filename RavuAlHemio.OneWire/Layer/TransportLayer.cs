using System;
using System.Collections.Generic;
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
                        return ReadEEPROM(bank, serialNumber, startAddress, continuedRead, readLength);
                    }
                    return ReadApplicationRegister(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x04:
                case 0x06:
                case 0x08:
                case 0x0A:
                case 0x0C:
                case 0x23:
                    // Non-Volatile Memory Bank, Scratchpad Memory Bank
                case 0x18:
                    // Non-Volatile Memory Bank, Scratchpad SHA Memory Bank
                case 0x1A:
                case 0x1D:
                    // Non-Volatile Cyclic Redundancy Check Memory Bank, Scratchpad Ex(???) Memory Bank
                case 0x21:
                    // Non-Volatile Cyclic Redundancy Check Memory Bank, Scratchpad Cyclic Redundancy Check Memory Bank

                    if (bank > 0)
                    {
                        return ReadNonVolatileMemory(bank, serialNumber, startAddress, continuedRead, readLength);
                    }
                    return ReadScratchpad(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x33:
                case 0xB3:
                    // SHA EEPROM Memory Bank
                    return ReadSHAEEPROM(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x2D:
                    // EEPROM with Write Protection Memory Bank
                    return ReadEEPROMWriteProtected(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x09:
                case 0x0B:
                case 0x0F:
                case 0x12:
                case 0x13:
                    // EPROM Memory Bank
                    return ReadEPROM(bank, serialNumber, startAddress, continuedRead, readLength);

                case 0x37:
                case 0x77:
                    // Password-Protected EEPROM Memory Bank, Scratchpad Cyclic Redundancy Check 0x77 Memory Bank

                    if (bank > 0)
                    {
                        return ReadEEPROMPasswordProtected(bank, serialNumber, startAddress, continuedRead, readLength);
                    }

                    byte[] extra;
                    return ReadScratchpadCRC77(serialNumber, readLength, out extra);
            }
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
