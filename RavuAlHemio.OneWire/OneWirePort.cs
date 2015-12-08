using System;
using RavuAlHemio.OneWire.Driver;
using RavuAlHemio.OneWire.Layer;

namespace RavuAlHemio.OneWire
{
    public class OneWirePort : IDisposable
    {
        private bool _disposed = false;

        protected IOneWireConnection Connection { get; private set; }

        public int PortNumber { get; private set; }
        public string PortName { get; private set; }

        public LinkLayer Link { get; protected set; }
        public NetworkLayer Network { get; protected set; }
        public TransportLayer Transport { get; protected set; }

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

            Link = new LinkLayer(this, Connection.Link);
            Network = new NetworkLayer(this, Connection.Network);
            Transport = new TransportLayer(this, Connection.Transport);
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
            PortNumber = connection.Session.AcquireEx(portName);

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

            Link = new LinkLayer(this, Connection.Link);
            Network = new NetworkLayer(this, Connection.Network);
            Transport = new TransportLayer(this, Connection.Transport);
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

