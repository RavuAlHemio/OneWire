using System;

namespace RavuAlHemio.OneWire
{
    public class BitStringBuffer
    {
        public byte[] Buffer { get; private set; }
        public int Length { get; private set; }

        public BitStringBuffer(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "length less than zero");
            }

            int bufLen = length / 8;
            if (length % 8 != 0)
            {
                ++bufLen;
            }

            Buffer = new byte[bufLen];
            Length = length;
        }

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

