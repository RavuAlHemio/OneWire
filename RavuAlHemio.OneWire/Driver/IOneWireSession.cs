using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// Session-level interface to a 1-Wire Net.
    /// </summary>
    public interface IOneWireSession
    {
        /// <summary>
        /// Attempts to acquire a 1-Wire Net port adapter and bind it to a given symbolic port number.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number to use for this port, if possible.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <returns>
        /// <c>true</c> if the port adapter was acquired and assigned the given port number successfully; <c>false</c>
        /// otherwise.
        /// </returns>
        /// <seealso cref="AcquireEx"/>
        /// <seealso cref="Release"/>
        bool Acquire(int portNumber, [NotNull] string portName);

        /// <summary>
        /// Attempts to acquire a 1-Wire Net port adapter and returns its symbolic port number.
        /// </summary>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <returns>
        /// The non-zero port number if the port was successfully acquired; <c>-1</c> otherwise.
        /// </returns>
        /// <seealso cref="Acquire"/>
        /// <seealso cref="Release"/>
        int AcquireEx([NotNull] string portName);

        /// <summary>
        /// Releases a previously acquired 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">The symbolic port number of the port to release.</param>
        /// <seealso cref="Acquire"/>
        /// <seealso cref="AcquireEx"/>
        void Release(int portNumber);
    }
}
