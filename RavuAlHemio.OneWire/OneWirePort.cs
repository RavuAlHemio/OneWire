using System;

namespace RavuAlHemio.OneWire
{
    public class OneWirePort : IDisposable
    {
        private bool _disposed = false;

        protected IOneWireLink Link { get; private set; }
        protected int PortNumber { get; private set; }
        public string PortName { get; private set; }

        /// <summary>
        /// Creates a new 1-Wire port using the given link and acquiring the port using
        /// <see cref="IOneWireLink.AcquireEx"/>.
        /// </summary>
        /// <param name="link">The link to use.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        public OneWirePort(IOneWireLink link, string portName)
        {
            Link = link;
            PortName = portName;
            PortNumber = link.AcquireEx(portName);
            if (PortNumber == -1)
            {
                throw OneWireOperationException.Create("failed to acquire port " + portName, link);
            }
        }

        /// <summary>
        /// Creates a new 1-Wire port using the given link and optionally acquiring the port using
        /// <see cref="IOneWireLink.Acquire"/>.
        /// </summary>
        /// <param name="link">The link to use.</param>
        /// <param name="portNumber">The port number to use.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <param name="acquire">
        /// Whether to actively acquire the port. If <c>false</c>, the port must have already been acquired previously.
        /// Setting this argument to <c>false</c> is strongly discouraged.
        /// </param>
        public OneWirePort(IOneWireLink link, int portNumber, string portName, bool acquire)
        {
            Link = link;
            PortName = portName;
            PortNumber = portNumber;
            if (acquire)
            {
                if (!link.Acquire(portNumber, portName))
                {
                    throw OneWireOperationException.Create("failed to acquire port " + portName + " using number " + portNumber, link);
                }
            }
        }

        /// <see cref="IOneWireLink.TouchReset"/>
        public bool TouchReset()
        {
            return Link.TouchReset(PortNumber);
        }

        /// <see cref="IOneWireLink.TouchBit"/> 
        public bool TouchBit(bool bitToSend)
        {
            return Link.TouchBit(PortNumber, bitToSend);
        }

        /// <see cref="IOneWireLink.TouchByte"/> 
        public byte TouchByte(byte byteToSend)
        {
            return Link.TouchByte(PortNumber, byteToSend);
        }

        /// <see cref="IOneWireLink.SetPortSpeed"/> 
        public NetSpeed SetPortSpeed(NetSpeed newSpeed)
        {
            return Link.SetPortSpeed(PortNumber, newSpeed);
        }

        /// <see cref="IOneWireLink.SetLineLevel"/>
        public LineLevel SetLineLevel(LineLevel newLevel)
        {
            return Link.SetLineLevel(PortNumber, newLevel);
        }

        /// <see cref="IOneWireLink.ProgramPulse"/>
        public bool ProgramPulse()
        {
            return Link.ProgramPulse(PortNumber);
        }

        /// <see cref="IOneWireLink.TouchByteAndSetPower"/>
        public byte TouchByteAndSetPower(byte byteToSend)
        {
            return Link.TouchByteAndSetPower(PortNumber, byteToSend);
        }

        /// <see cref="IOneWireLink.ReadBitAndSetPower"/>
        public bool ReadBitAndSetPower(bool applyPowerResponse)
        {
            return Link.ReadBitAndSetPower(PortNumber, applyPowerResponse);
        }

        /// <see cref="IOneWireLink.PortSupportsPowerDelivery"/>
        public bool PortSupportsPowerDelivery => Link.PortSupportsPowerDelivery(PortNumber);

        /// <see cref="IOneWireLink.PortSupportsOverdrive"/>
        public bool PortSupportsOverdrive => Link.PortSupportsOverdrive(PortNumber);

        /// <see cref="IOneWireLink.PortSupportsProgramPulse"/>
        public bool PortSupportsProgramPulse => Link.PortSupportsProgramPulse(PortNumber);

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
                Link.Release(PortNumber);
                Link.Dispose();
            }

            // release unmanaged resources

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OneWirePort()
        {
            Dispose(false);
        }
        #endregion
    }
}

