namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// Error-handling interface for 1-Wire devices.
    /// </summary>
    public interface IOneWireErrors
    {
        /// <summary>
        /// Pops the error number of the most recent error from the error stack.
        /// </summary>
        /// <returns>The error number popped from the error stack.</returns>
        int PopErrorNumber();

        /// <summary>
        /// Whether the link has experienced an error.
        /// </summary>
        /// <value><c>true</c> if this link has experienced an error; <c>false </c> otherwise.</value>
        bool HasError { get; }
    }
}
