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
