using System;
using System.Collections.Generic;
using System.Linq;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire.Layer
{
    public class NetworkLayer : IDisposable
    {
        private bool _disposed;

        public OneWirePort Port { get; set; }
        protected IOneWireNetwork Driver { get; set; }

        public NetworkLayer(OneWirePort port, IOneWireNetwork driver)
        {
            Port = port;
            Driver = driver;
        }

        /// <see cref="IOneWireNetwork.FindFirstDevice"/>
        public virtual bool FindFirstDevice(bool resetBeforeSearch, bool alarmOnly)
        {
            return Driver.FindFirstDevice(Port.PortNumber, resetBeforeSearch, alarmOnly);
        }

        /// <see cref="IOneWireNetwork.FindNextDevice"/>
        public virtual bool FindNextDevice(bool resetBeforeSearch, bool alarmOnly)
        {
            return Driver.FindNextDevice(Port.PortNumber, resetBeforeSearch, alarmOnly);
        }

        /// <see cref="IOneWireNetwork.GetSerialNumber"/>
        /// <see cref="IOneWireNetwork.SetSerialNumber"/>
        public virtual ulong SerialNumber
        {
            get
            {
                return Driver.GetSerialNumber(Port.PortNumber);
            }
            set
            {
                Driver.SetSerialNumber(Port.PortNumber, value);
            }
        }

        /// <see cref="IOneWireNetwork.FamilySearchSetup"/>
        public virtual void FamilySearchSetup(byte searchFamily)
        {
            Driver.FamilySearchSetup(Port.PortNumber, searchFamily);
        }

        /// <see cref="IOneWireNetwork.SkipFamily"/>
        public virtual void SkipFamily()
        {
            Driver.SkipFamily(Port.PortNumber);
        }

        /// <summary>
        /// Reset the 1-Wire Net and send a MATCH Serial Number command containing the current serial number
        /// (<see cref="SerialNumber"/>), whereupon the device with that serial number is ready to receive commands.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device with the active serial number has been found and is ready for commands;
        /// <c>false</c> otherwise.
        /// </returns>
        public virtual bool AccessDevice()
        {
            // reset the 1-wire
            if (!Port.Link.TouchReset())
            {
                throw OneWireOperationException.Create(
                    "error accessing device",
                    ErrorCode.NoDevicesOnNet
                );
            }

            // assemble the command
            var cmd = new List<byte>(9);
            // match Serial Number command 0x55
            cmd.Add(0x55);

            // add the serial number (Little Endian)
            ulong serial = SerialNumber;
            for (int i = 0; i < 8; ++i)
            {
                cmd.Add((byte)((serial >> i) & 0xFF));
            }

            // send!
            byte[] received = Port.Transport.TransferBlock(false, cmd);
            if (received == null || received.Length != cmd.Count)
            {
                throw OneWireOperationException.Create(
                    "error accessing device",
                    ErrorCode.BlockFailed
                );
            }

            // command correct?
            if (cmd[0] != received[0])
            {
                throw OneWireOperationException.Create(
                    "error accessing device",
                    ErrorCode.WriteVerifyFailed
                );
            }

            // serial number correct?
            return cmd.Skip(1).SequenceEqual(received.Skip(1));
        }

        /// <summary>
        /// Returns whether the currently active device is in contact with the 1-Wire Net.
        /// </summary>
        /// <remarks>
        /// The currently active device is stored in <see cref="SerialNumber"/>.
        /// </remarks>
        /// <param name="alarmOnly">If <c>true</c>, it is also checked whether the device is in an alarm state.</param>
        /// <returns>
        /// If <paramref name="alarmOnly"/> is <c>false</c>, returns <c>true</c> if the currently active device is in
        /// contact with the 1-Wire Net, and <c>false</c> otherwise. If <paramref name="alarmOnly"/> is true, returns
        /// <c>true</c> if the currently active device is in contact with the 1-Wire Net and is in an alarm state, and
        /// <c>false</c> otherwise.
        /// </returns>
        public virtual bool VerifyDevice(bool alarmOnly)
        {
            // construct the search ROM command
            var cmd = new List<byte>(50);

            if (alarmOnly)
            {
                // alarm search
                cmd.Add(0xEC);
            }
            else
            {
                // search
                cmd.Add(0xF0);
            }

            // encode the serial number SSSS... as 11S11S11S11S...
            // a 64-bit serial number requires 192 bits (24 bytes)
            ulong serial = SerialNumber;
            var serialBits = new BitStringBuffer(192);
            for (int i = 0; i < 64; ++i)
            {
                serialBits[3*i + 0] = true;
                serialBits[3*i + 1] = true;
                serialBits[3*i + 2] = (((serial >> i) & 0x1) == 0x1);
            }

            // add that to the command
            cmd.AddRange(serialBits.Buffer);

            // transfer it
            var blockResponse = Port.Transport.TransferBlock(true, cmd);
            if (blockResponse == null)
            {
                throw OneWireOperationException.Create(
                    "error verifying device presence",
                    ErrorCode.BlockFailed
                );
            }

            // check the results
            var serialResponseBits = new BitStringBuffer(192);
            for (int i = 0; i < 64; ++i)
            {
                serialResponseBits.Buffer[i] = blockResponse[i+1];
            }

            int goodBits = 0;
            for (int i = 0; i < 64; ++i)
            {
                bool responseTopBit = serialResponseBits[3*i];
                bool responseBottomBit = serialResponseBits[3*i + 1];

                bool serialBit = serialBits[3*i + 2];

                if (responseTopBit && responseBottomBit)
                {
                    // no device on line
                    goodBits = 0;
                    break;
                }

                if ((serialBit && responseTopBit && !responseBottomBit) ||
                    (!serialBit && !responseTopBit && responseBottomBit))
                {
                    // correct bit
                    ++goodBits;
                }
            }

            if (goodBits > 8)
            {
                // enough to be successful
                return true;
            }

            // not good enough
            return false;
        }

        /// <summary>
        /// Reset the 1-Wire Net and send an overdrive MATCH Serial Number command containing the current serial number
        /// (<see cref="SerialNumber"/>), whereupon the device with that serial number is ready to receive commands.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device with the active serial number has been found and is ready for commands;
        /// <c>false</c> otherwise.
        /// </returns>
        public virtual bool OverdriveAccessDevice()
        {
            // TODO
            throw new NotImplementedException();

            // set power level and comm speed to normal
            Port.Link.SetLineLevel(LineLevel.Normal);
            Port.Link.SetPortSpeed(NetSpeed.Normal);
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

        ~NetworkLayer()
        {
            Dispose(false);
        }
        #endregion
    }
}
