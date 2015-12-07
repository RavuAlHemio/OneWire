using System;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire
{
    public class OneWirePort : IDisposable
    {
        private bool _disposed = false;

        protected IOneWireConnection Connection { get; private set; }
        protected int PortNumber { get; private set; }
        public string PortName { get; private set; }

        /// <summary>
        /// Creates a new 1-Wire port using the given Connection and acquiring the port using
        /// <see cref="IOneWireSession.AcquireEx"/>.
        /// </summary>
        /// <param name="connection">The Connection to use.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        public OneWirePort(IOneWireConnection connection, string portName)
        {
            Connection = connection;
            PortName = portName;
            PortNumber = connection.Session.AcquireEx(portName);
            if (PortNumber == -1)
            {
                throw OneWireOperationException.Create("failed to acquire port " + portName, connection.Errors);
            }
        }

        /// <summary>
        /// Creates a new 1-Wire port using the given Connection and optionally acquiring the port using
        /// <see cref="IOneWireSession.Acquire"/>.
        /// </summary>
        /// <param name="connection">The Connection to use.</param>
        /// <param name="portNumber">The port number to use.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <param name="acquire">
        /// Whether to actively acquire the port. If <c>false</c>, the port must have already been acquired previously.
        /// Setting this argument to <c>false</c> is strongly discouraged.
        /// </param>
        public OneWirePort(IOneWireConnection connection, int portNumber, string portName, bool acquire)
        {
            Connection = connection;
            PortName = portName;
            PortNumber = portNumber;
            if (acquire)
            {
                if (!connection.Session.Acquire(portNumber, portName))
                {
                    throw OneWireOperationException.Create(
                        "failed to acquire port " + portName + " using number " + portNumber,
                        connection.Errors
                    );
                }
            }
        }

        /// <see cref="IOneWireLink.TouchReset"/>
        public bool TouchReset()
        {
            return Connection.Link.TouchReset(PortNumber);
        }

        /// <see cref="IOneWireLink.TouchBit"/> 
        public bool TouchBit(bool bitToSend)
        {
            return Connection.Link.TouchBit(PortNumber, bitToSend);
        }

        /// <see cref="IOneWireLink.TouchByte"/> 
        public byte TouchByte(byte byteToSend)
        {
            return Connection.Link.TouchByte(PortNumber, byteToSend);
        }

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net, read the resulting 8 bits and return whether the sent and
        /// received bytes were equal.
        /// </summary>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>
        /// <c>true</c> if the byte read from the 1-Wire Net equalled the sent byte (<paramref name="byteToSend"/>);
        /// <c>false</c> if it did not.
        /// </returns>
        public bool WriteByte(byte byteToSend)
        {
            return TouchByte(byteToSend) == byteToSend;
        }

        /// <summary>
        /// Receive and return 8 bits of communication from the 1-Wire Net by writing the value <c>0xFF</c> and reading
        /// the response.
        /// </summary>
        /// <returns>The byte read from the 1-Wire Net (after sending <c>0xFF</c>).</returns>
        public byte ReadByte()
        {
            return TouchByte(0xFF);
        }

        /// <see cref="IOneWireLink.SetPortSpeed"/> 
        public NetSpeed SetPortSpeed(NetSpeed newSpeed)
        {
            return Connection.Link.SetPortSpeed(PortNumber, newSpeed);
        }

        /// <see cref="IOneWireLink.SetLineLevel"/>
        public LineLevel SetLineLevel(LineLevel newLevel)
        {
            return Connection.Link.SetLineLevel(PortNumber, newLevel);
        }

        /// <see cref="IOneWireLink.ProgramPulse"/>
        public bool ProgramPulse()
        {
            return Connection.Link.ProgramPulse(PortNumber);
        }

        /// <see cref="IOneWireLink.TouchByteAndSetPower"/>
        public byte TouchByteAndSetPower(byte byteToSend)
        {
            return Connection.Link.TouchByteAndSetPower(PortNumber, byteToSend);
        }

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net, read the resulting 8 bits, change the level of the 1-Wire
        /// Net to power delivery, and return whether the sent and received bytes were equal and the power level change
        /// succeeded.
        /// </summary>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>
        /// <c>true</c> if the byte read from the 1-Wire Net equalled the sent byte (<paramref name="byteToSend"/>) and
        /// the power level change succeeded; <c>false</c> if the bytes differed or the power level change failed (or
        /// both).
        /// </returns>
        public bool WriteByteAndSetPower(byte byteToSend)
        {
            return TouchByteAndSetPower(byteToSend) == byteToSend;
        }

        /// <summary>
        /// Receive and return 8 bits of communication from the 1-Wire Net by writing the value <c>0xFF</c> and reading
        /// the response, then change the level of the 1-Wire Net to power delivery.
        /// </summary>
        /// <returns>The byte read from the 1-Wire Net (after sending <c>0xFF</c>).</returns>
        public byte ReadByteAndSetPower()
        {
            return TouchByteAndSetPower(0xFF);
        }

        /// <see cref="IOneWireLink.ReadBitAndSetPower"/>
        public bool ReadBitAndSetPower(bool applyPowerResponse)
        {
            return Connection.Link.ReadBitAndSetPower(PortNumber, applyPowerResponse);
        }

        /// <see cref="IOneWireLink.PortSupportsPowerDelivery"/>
        public bool PortSupportsPowerDelivery => Connection.Link.PortSupportsPowerDelivery(PortNumber);

        /// <see cref="IOneWireLink.PortSupportsOverdrive"/>
        public bool PortSupportsOverdrive => Connection.Link.PortSupportsOverdrive(PortNumber);

        /// <see cref="IOneWireLink.PortSupportsProgramPulse"/>
        public bool PortSupportsProgramPulse => Connection.Link.PortSupportsProgramPulse(PortNumber);

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
                Connection.Session.Release(PortNumber);
                Connection.Dispose();
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

