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
            for (int bytei = 0; bytei < 8; ++bytei)
            {
                byte cmdByte = 0;

            }

            // set all bytes by default
            for (int i = 0; i < 24; ++i)
            {
                cmd.Add(0xFF);
            }

            // TODO
            throw new NotImplementedException();

            /*
            for (int i = 0; i < 64; ++i)
            {
                cmdSerialBit[(i + 1) * 3 - 1] = serialBit[i];
            }

            cmdSerialBit[2] = serialBit[0];
            cmdSerialBit[5] = serialBit[1];
            cmdSerialBit[8] = serialBit[2];
            cmdSerialBit[11] = serialBit[3];
            */
        }

        /// <summary>
        /// Reset the 1-Wire Net and send an overdrive MATCH Serial Number command containing the current serial number
        /// (see <see cref="GetSerialNumber"/> and <see cref="SetSerialNumber"/>), whereupon the device with that
        /// serial number is ready to receive commands.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns>
        /// <c>true</c> if the device with the active serial number has been found and is ready for commands;
        /// <c>false</c> otherwise.
        /// </returns>
        //bool OverdriveAccessDevice(int portNumber);

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
