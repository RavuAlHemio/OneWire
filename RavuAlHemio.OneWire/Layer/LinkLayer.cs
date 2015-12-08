using System;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire.Layer
{
    public class LinkLayer : IDisposable
    {
        private bool _disposed;

        public OneWirePort Port { get; set; }
        protected IOneWireLink Driver { get; set; }

        public LinkLayer(OneWirePort port, IOneWireLink driver)
        {
            Port = port;
            Driver = driver;
        }

        /// <see cref="IOneWireLink.TouchReset"/>
        public virtual bool TouchReset()
        {
            return Driver.TouchReset(Port.PortNumber);
        }

        /// <see cref="IOneWireLink.TouchBit"/>
        public virtual bool TouchBit(bool bitToSend)
        {
            return Driver.TouchBit(Port.PortNumber, bitToSend);
        }

        /// <see cref="IOneWireLink.TouchByte"/>
        public virtual byte TouchByte(byte byteToSend)
        {
            return Driver.TouchByte(Port.PortNumber, byteToSend);
        }

        /// <see cref="IOneWireLink.WriteByte"/>
        public virtual bool WriteByte(byte byteToSend)
        {
            return Driver.WriteByte(byteToSend);
        }

        /// <summary>
        /// Receive and return 8 bits of communication from the 1-Wire Net by writing the value <c>0xFF</c> and reading
        /// the response.
        /// </summary>
        /// <returns>The byte read from the 1-Wire Net (after sending <c>0xFF</c>).</returns>
        public virtual byte ReadByte()
        {
            return TouchByte(0xFF);
        }

        /// <see cref="IOneWireLink.SetPortSpeed"/>
        public virtual NetSpeed SetPortSpeed(NetSpeed newSpeed)
        {
            return Driver.SetPortSpeed(Port.PortNumber, newSpeed);
        }

        /// <see cref="IOneWireLink.SetLineLevel"/>
        public virtual LineLevel SetLineLevel(LineLevel newLevel)
        {
            return Driver.SetLineLevel(Port.PortNumber, newLevel);
        }

        /// <see cref="IOneWireLink.ProgramPulse"/>
        public virtual bool ProgramPulse()
        {
            return Driver.ProgramPulse(Port.PortNumber);
        }

        /// <see cref="IOneWireLink.WriteByteAndSetPower"/>
        public virtual bool WriteByteAndSetPower(byte byteToSend)
        {
            return Driver.WriteByteAndSetPower(byteToSend);
        }

        /// <see cref="IOneWireLink.ReadByteAndSetPower"/>
        public virtual byte ReadByteAndSetPower()
        {
            return Driver.ReadByteAndSetPower();
        }

        /// <see cref="IOneWireLink.ReadBitAndSetPower"/>
        public virtual bool ReadBitAndSetPower(bool applyPowerResponse)
        {
            return Driver.ReadBitAndSetPower(Port.PortNumber, applyPowerResponse);
        }

        /// <see cref="IOneWireLink.PortSupportsPowerDelivery"/>
        public virtual bool PortSupportsPowerDelivery => Driver.PortSupportsPowerDelivery(Port.PortNumber);

        /// <see cref="IOneWireLink.PortSupportsOverdrive"/>
        public virtual bool PortSupportsOverdrive => Driver.PortSupportsOverdrive(Port.PortNumber);

        /// <see cref="IOneWireLink.PortSupportsProgramPulse"/>
        public virtual bool PortSupportsProgramPulse => Driver.PortSupportsProgramPulse(Port.PortNumber);

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

        ~LinkLayer()
        {
            Dispose(false);
        }
        #endregion
    }
}
