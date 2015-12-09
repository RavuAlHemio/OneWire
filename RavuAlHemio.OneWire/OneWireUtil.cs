using System;
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// 1-Wire Protocol utilities.
    /// </summary>
    public static class OneWireUtil
    {
        /// <summary>
        /// Emulates <see cref="IOneWireLink.TouchByte"/> using eight subsequent calls to
        /// <see cref="IOneWireLink.TouchBit"/>.
        /// </summary>
        /// <param name="link">The link on which to perform the byte I/O.</param>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>The byte read from the 1-Wire Net after the <paramref name="byteToSend"/> was sent.</returns>
        public static byte EmulatedTouchByte(this IOneWireLink link, int portNumber, byte byteToSend)
        {
            byte byteToReturn = 0;

            // LSB to MSB
            for (int i = 0; i < 8; ++i)
            {
                bool bitToSend = (((byteToSend >> i) & 1) == 1);
                bool bitReceived = link.TouchBit(portNumber, bitToSend);
                if (bitReceived)
                {
                    byteToReturn |= (byte)(1 << i);
                }
            }

            return byteToReturn;
        }

        /// <summary>
        /// Converts a 1-Wire serial number from an unsigned 64-bit integer to a byte array.
        /// </summary>
        /// <remarks>
        /// 1-Wire dictates that multi-byte integers be supplied in little-endian order.
        /// </remarks>
        /// <returns>The serial number as a byte array.</returns>
        /// <param name="serialNumber">The serial number to convert to a byte array.</param>
        [NotNull]
        public static byte[] SerialNumberToByteArray(ulong serialNumber)
        {
            var ret = new byte[8];
            for (int i = 0; i < 8; ++i)
            {
                ret[i] = (byte)((serialNumber >> (8*i)) & 0xFF);
            }
            return ret;
        }

        /// <summary>
        /// Converts a 1-Wire serial number from a byte array to an unsigned 64-bit integer.
        /// </summary>
        /// <remarks>
        /// 1-Wire dictates that multi-byte integers be supplied in little-endian order.
        /// </remarks>
        /// <returns>The serial number as an unsigned 64-bit integer.</returns>
        /// <param name="serialNumber">The serial number to convert to an unsigned 64-bit integer.</param>
        public static ulong SerialNumberToInteger([NotNull] byte[] serialNumber)
        {
            if (serialNumber == null)
            {
                throw new ArgumentNullException(nameof(serialNumber));
            }
            if (serialNumber.Length != 8)
            {
                throw new ArgumentException("serialNumber.Length must be 8", nameof(serialNumber));
            }

            ulong ret = 0;
            for (int i = 0; i < 8; ++i)
            {
                ret |= (((ulong)serialNumber[i]) << (8 * i));
            }

            return ret;
        }
    }
}
