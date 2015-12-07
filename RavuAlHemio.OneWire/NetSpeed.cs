namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// The communication rate of a 1-Wire Net.
    /// </summary>
    public enum NetSpeed : byte
    {
        /// <summary>
        /// Normal rate (16 kb/s).
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// Overdrive rate (142 kb/s).
        /// </summary>
        Overdrive = 0x01
    }
}
