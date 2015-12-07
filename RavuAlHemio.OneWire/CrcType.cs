namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// Type of cyclic redundancy check to use.
    /// </summary>
    public enum CRCType : byte
    {
        /// <summary>
        /// Cyclic Redundancy Check for 8 bits of data.
        /// </summary>
        CRC8 = 0x00,

        /// <summary>
        /// Cyclic Redundancy Check for 16 bits of data.
        /// </summary>
        CRC16 = 0x01
    }
}
