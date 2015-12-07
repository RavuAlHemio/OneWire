namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// The line level of a 1-Wire Net.
    /// </summary>
    public enum LineLevel
    {
        /// <summary>
        /// The normal line level of a 1-Wire Net (5V, weak pull-up).
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// The power delivery line level of a 1-Wire Net (5V, strong pull-up).
        /// </summary>
        PowerDelivery = 0x02,

        /// <summary>
        /// The EPROM programming line level of a 1-Wire Net (12V).
        /// </summary>
        /// <remarks>Unsupported on some link types.</remarks>
        Program = 0x04,

        /// <summary>
        /// Sends a short break to the 1-Wire Net.
        /// </summary>
        /// <remarks>Unsupported on many link types.</remarks>
        Break = 0x08
    }
}
