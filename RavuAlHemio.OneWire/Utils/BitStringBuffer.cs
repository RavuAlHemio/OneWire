//---------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Utils
{
    /// <summary>
    /// A fixed-length mutable bit string. Bytes are ordered lower-first, bits within a byte are LSB-first.
    /// </summary>
    /// <remarks>
    /// Assuming a two-byte buffer, the bit with index 0 is the LSB of byte 0, the bit with index 7 is the MSB of byte
    /// 0, the bit with index 8 is the LSB of byte 1, etc.
    /// </remarks>
    public class BitStringBuffer
    {
        /// <summary>
        /// The byte buffer backing this bit string.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// The length of this bit string, in bits.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Constructs a new zeroed bit string buffer with the given length.
        /// </summary>
        /// <param name="length">The length of the bit string, in bits.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="length"/> is less than 0.
        /// </exception>
        public BitStringBuffer(int length)
            : this(Enumerable.Repeat((byte)0, length / 8 + 1), length)
        {
        }

        /// <summary>
        /// Constructs a new bit string buffer with a copy of the given data.
        /// </summary>
        /// <param name="initialBits">The initial bits of the bit string.</param>
        /// <param name="length">The length of the bit string, in bits.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="length"/> is less than 0.
        /// </exception>
        public BitStringBuffer([NotNull] IEnumerable<byte> initialBits, int length)
        {
            if (initialBits == null)
            {
                throw new ArgumentNullException(nameof(initialBits));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length less than zero");
            }

            int bufLen = length / 8;
            if (length % 8 != 0)
            {
                ++bufLen;
            }

            Buffer = initialBits.Take(bufLen).ToArray();
            Length = length;
        }

        /// <summary>
        /// Gets or sets the bit at the given index (lower-byte-first, LSB-first).
        /// </summary>
        /// <param name="index">The index of the bit to get/set.</param>
        /// <returns>The bit at <paramref name="index"/>.</returns>
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return (((Buffer[index / 8] >> (index % 8)) & 0x1) == 0x1);
            }

            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                if (value)
                {
                    Buffer[index / 8] |= (byte)(0x1 << (index % 8));
                }
                else
                {
                    Buffer[index / 8] &= (byte)(~(0x1 << (index % 8)));
                }
            }
        }
    }
}
