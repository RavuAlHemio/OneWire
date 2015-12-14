using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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
        private byte[] _address;

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
                addressBytes[i] = (byte)((address >> (8*i)) & 0xFF);
            }
            _address = addressBytes;
        }

        /// <summary>
        /// Initializes a new 1-Wire address.
        /// </summary>
        /// <param name="address">The address as a big-endian hexadecimal string.</param>
        public OneWireAddress(string address)
        {
            var addressBytes = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                byte top = Utils.Hex.CharToNibble(address[2*i]);
                byte bottom = Utils.Hex.CharToNibble(address[2*i + 1]);
                addressBytes[7 - i] = (byte)((top << 4) | bottom);
            }
            _address = addressBytes;
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
    }
}
