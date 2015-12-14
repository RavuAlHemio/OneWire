using System.Collections.Generic;

namespace RavuAlHemio.OneWire.Utils
{
    /// <summary>
    /// An implementation of the 16-bit Cyclic Redundancy Check used in 1-Wire Networks.
    /// </summary>
    /// <remarks>
    /// The polynomial is X^16 + X^15 + X^2 + 1.
    /// </remarks>
    public static class CRC16
    {
        /// <summary>
        /// CRC16 lookup table.
        /// </summary>
        private static readonly byte[] oddParity = {0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0};

        /// <summary>
        /// Calculate the CRC16 on the given data element, optionally based on a previous CRC value.
        /// </summary>
        /// <param name="data">The data element on which to calculate the CRC16.</param>
        /// <param name="seed">
        /// An optional previous value on which to base this calculation, e.g. the CRC16 of the preceding bytes. If
        /// this calculation should be independent, pass the default value (<c>0x0000</c>).
        /// </param>
        /// <returns>The CRC16 calculated from <paramref name="data"/> and optionally <paramref name="seed"/>.</returns>
        public static ushort Compute(byte data, ushort seed = 0x0000)
        {
            var dat = (ushort)((data ^ (seed & 0xFF)) & 0xFF);

            seed = (ushort)((seed & 0xFFFF) >> 8);

            int index1 = (dat & 0x0F);
            int index2 = (dat >> 4);

            if ((oddParity[index1] ^ oddParity[index2]) == 1)
            {
                seed ^= 0xC001;
            }

            dat <<= 6;
            seed ^= dat;
            dat <<= 1;
            seed ^= dat;

            return seed;
        }

        /// <summary>
        /// Calculate the CRC16 on the given bytes, optionally based on a previous CRC value.
        /// </summary>
        /// <param name="data">The data elements on which to calculate the CRC16.</param>
        /// <param name="seed">
        /// An optional previous value on which to base this calculation, e.g. the CRC16 of the preceding bytes. If
        /// this calculation should be independent, pass the default value (<c>0x0000</c>).
        /// </param>
        /// <returns>The CRC16 calculated from <paramref name="data"/> and optionally <paramref name="seed"/>.</returns>
        public static ushort Compute(IEnumerable<byte> data, ushort seed = 0x0000)
        {
            ushort val = seed;
            foreach (byte d in data)
            {
                val = Compute(d, val);
            }
            return val;
        }
    }
}
