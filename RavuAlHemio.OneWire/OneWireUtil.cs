using System;

namespace RavuAlHemio.OneWire
{
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
    }
}

