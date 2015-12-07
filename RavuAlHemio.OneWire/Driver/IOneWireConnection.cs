using System;
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// A connection to a 1-Wire Net.
    /// </summary>
    public interface IOneWireConnection : IDisposable
    {
        /// <summary>
        /// Link-level interface to a 1-Wire Net.
        /// </summary>
        [NotNull]
        IOneWireLink Link { get; }

        /// <summary>
        /// Network-level interface to a 1-Wire Net.
        /// </summary>
        [NotNull]
        IOneWireNetwork Network { get; }

        /// <summary>
        /// Transport-level interface to a 1-Wire Net.
        /// </summary>
        [NotNull]
        IOneWireTransport Transport { get; }

        /// <summary>
        /// Session-level interface to a 1-Wire Net.
        /// </summary>
        [NotNull]
        IOneWireSession Session { get; }

        /// <summary>
        /// Error interface to a 1-Wire device.
        /// </summary>
        [NotNull]
        IOneWireErrors Errors { get; }
    }
}
