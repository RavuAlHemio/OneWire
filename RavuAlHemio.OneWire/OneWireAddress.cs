﻿//---------------------------------------------------------------------------
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

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Utils;

namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// The address of a 1-Wire bus device.
    /// </summary>
    /// <remarks>
    /// A 1-Wire Address is 64 bits long and consists of an 8-bit family code, 48 bits of serialized data and an 8-bit
    /// CRC of the first 56 bits.
    /// </remarks>
    public struct OneWireAddress
    {
        /// <summary>
        /// The actual bytes of the address.
        /// </summary>
        private readonly byte[] _address;

        /// <summary>
        /// Initializes a new 1-Wire address.
        /// </summary>
        /// <param name="address">The bytes of the address in little-endian order. Must be 8 bytes long.</param>
        public OneWireAddress([NotNull] ICollection<byte> address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var addressArray = address.ToArray();
            if (addressArray.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(address), nameof(address) + " must be exactly 8 bytes long");
            }
            _address = addressArray;

            if (!IsValid)
            {
                throw new ArgumentException(nameof(address) + " is not a valid 1-Wire address", nameof(address));
            }
        }

        /// <summary>
        /// Initializes a new 1-Wire address.
        /// </summary>
        /// <param name="address">The address as a 64-bit integer.</param>
        public OneWireAddress(long address)
        {
            var addressUL = unchecked((ulong)address);
            var addressBytes = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                addressBytes[i] = (byte)((addressUL >> (8*i)) & 0xFF);
            }
            _address = addressBytes;

            if (!IsValid)
            {
                throw new ArgumentException(nameof(address) + " is not a valid 1-Wire address", nameof(address));
            }
        }

        /// <summary>
        /// Initializes a new 1-Wire address.
        /// </summary>
        /// <param name="address">The address as a big-endian hexadecimal string.</param>
        public OneWireAddress([NotNull] string address)
        {
            if (address.Length != 16)
            {
                throw new ArgumentOutOfRangeException(nameof(address), nameof(address) + " must be 16 characters long");
            }

            var addressBytes = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                byte top = Hex.CharToNibble(address[2*i]);
                byte bottom = Hex.CharToNibble(address[2*i + 1]);
                addressBytes[7 - i] = (byte)((top << 4) | bottom);
            }
            _address = addressBytes;

            if (!IsValid)
            {
                throw new ArgumentException(nameof(address) + " is not a valid 1-Wire address", nameof(address));
            }
        }

        /// <summary>
        /// Returns the 1-Wire address as a byte array.
        /// </summary>
        /// <returns>The address as a byte array.</returns>
        public byte[] ToByteArray()
        {
            var ret = new byte[8];
            Array.Copy(_address, ret, 8);
            return ret;
        }

        /// <summary>
        /// Returns the 1-Wire address as a 64-bit integer.
        /// </summary>
        /// <returns>The address as a 64-bit integer.</returns>
        public long ToLong()
        {
            ulong ret = 0;
            for (int i = 7; i >= 0; --i)
            {
                ret |= (((ulong)_address[i]) << (8 * i));
            }
            return unchecked((long)ret);
        }

        /// <summary>
        /// Returns the 1-Wire address as a big-endian hexadecimal string.
        /// </summary>
        /// <returns>The address as a big-endian hexadecimal string.</returns>
        public override string ToString()
        {
            return string.Concat(_address.Reverse().Select(b => b.ToString("X2")));
        }

        /// <summary>
        /// Gets the byte at the given location of the address.
        /// </summary>
        /// <param name="idx">The index of the byte to return.</param>
        public byte this[int idx]
        {
            get
            {
                if (idx < 0 || idx > 7)
                {
                    throw new IndexOutOfRangeException();
                }

                return _address[idx];
            }
        }

        private bool IsValid
        {
            get
            {
                if (_address[0] != 0 && Utils.CRC8.Compute(_address) == 0x00)
                {
                    return true;
                }

                if ((_address[0] & 0x7F) == 0x1C)
                {
                    // The DS28E04 has a pin-selectable ROM ID input. However, the CRC8 for the ROM ID assumes that the
                    // selectable bits are all 1.
                    var modifiedAddress = new List<byte>(_address);
                    modifiedAddress[1] = 0x7F;
                    return (Utils.CRC8.Compute(modifiedAddress) == 0);
                }

                return false;
            }
        }

        public override int GetHashCode()
        {
            return ToLong().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is OneWireAddress) && (this == (OneWireAddress) obj);
        }

        public static bool operator ==(OneWireAddress l, OneWireAddress r)
        {
            return l._address.SequenceEqual(r._address);
        }

        public static bool operator !=(OneWireAddress l, OneWireAddress r)
        {
            return !(l == r);
        }

        /// <summary>
        /// The family portion of this address.
        /// </summary>
        public byte FamilyPortion => _address[0];

        /// <summary>
        /// The 8-bit CRC (<see cref="CRC8"/>) portion of this address.
        /// </summary>
        public byte CRC8 => _address[7];
    }
}
