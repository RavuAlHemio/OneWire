//---------------------------------------------------------------------------
// Copyright © 1999, 2000 Maxim Integrated Products, All Rights Reserved.
// Copyright © 2015 Ondřej Hošek <ondra.hosek@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY,  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL MAXIM INTEGRATED PRODUCTS BE LIABLE FOR ANY CLAIM, DAMAGES
// OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Except as contained in this notice, the name of Maxim Integrated Products
// shall not be used except as stated in the Maxim Integrated Products
// Branding Policy.
//---------------------------------------------------------------------------

using System.Collections.Generic;

namespace RavuAlHemio.OneWire.Utils
{
    /// <summary>
    /// An implementation of the 8-bit Cyclic Redundancy Check used in 1-Wire Networks.
    /// </summary>
    /// <remarks>
    /// The polynomial is X^8 + X^5 + X^4 + 1.
    /// </remarks>
    public static class CRC8
    {
        /// <summary>
        /// CRC8 lookup table.
        /// </summary>
        private static readonly byte[] lookupTable;

        static CRC8()
        {
            lookupTable = new byte[256];

            for (int i = 0; i < lookupTable.Length; ++i)
            {
                byte acc = (byte)i;
                byte crc = 0;

                for (int j = 0; j < 8; ++j)
                {
                    if (((acc ^ crc) & 0x01) == 0x01)
                    {
                        crc = (byte)(((crc ^ 0x18) >> 1) | 0x80);
                    }
                    else
                    {
                        crc >>= 1;
                    }

                    acc >>= 1;
                }

                lookupTable[i] = crc;
            }
        }

        /// <summary>
        /// Calculate the CRC8 on the given data element, optionally based on a previous CRC value.
        /// </summary>
        /// <param name="data">The data element on which to calculate the CRC8.</param>
        /// <param name="seed">
        /// An optional previous value on which to base this calculation, e.g. the CRC8 of the preceding bytes. If this
        /// calculation should be independent, pass the default value (<c>0x00</c>).
        /// </param>
        /// <returns>The CRC8 calculated from <paramref name="data"/> and optionally <paramref name="seed"/>.</returns>
        public static byte Compute(byte data, byte seed = 0x00)
        {
            return lookupTable[data ^ seed];
        }

        /// <summary>
        /// Calculate the CRC8 on the given bytes, optionally based on a previous CRC value.
        /// </summary>
        /// <param name="data">The data elements on which to calculate the CRC8.</param>
        /// <param name="seed">
        /// An optional previous value on which to base this calculation, e.g. the CRC8 of the preceding bytes. If this
        /// calculation should be independent, pass the default value (<c>0x00</c>).
        /// </param>
        /// <returns>The CRC8 calculated from <paramref name="data"/> and optionally <paramref name="seed"/>.</returns>
        public static byte Compute(IEnumerable<byte> data, byte seed = 0x00)
        {
            byte val = seed;
            foreach (byte d in data)
            {
                val = Compute(d, val);
            }
            return val;
        }
    }
}
