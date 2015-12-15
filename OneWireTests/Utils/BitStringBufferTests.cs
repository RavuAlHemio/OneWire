using System;
using RavuAlHemio.OneWire.Utils;
using Xunit;

namespace OneWireTests.Utils
{
    public class BitStringBufferTests
    {
        [Fact]
        public void ConstructNegativeBitCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BitStringBuffer(-1));
        }

        [Fact]
        public void ConstructEvenBitCount()
        {
            var bsb = new BitStringBuffer(8);

            Assert.Equal(8, bsb.Length);
            for (int i = 0; i < 8; ++i)
            {
                Assert.Equal(false, bsb[i]);
            }

            Assert.Equal(1, bsb.Buffer.Length);
            Assert.Equal(0x00, bsb.Buffer[0]);
        }

        [Fact]
        public void ConstructOddBitCount()
        {
            var bsb = new BitStringBuffer(9);

            Assert.Equal(9, bsb.Length);
            for (int i = 0; i < 9; ++i)
            {
                Assert.Equal(false, bsb[i]);
            }

            Assert.Equal(2, bsb.Buffer.Length);
            Assert.Equal(0x00, bsb.Buffer[0]);
            Assert.Equal(0x00, bsb.Buffer[1]);
        }

        [Fact]
        public void OutOfBoundsAccessThrows()
        {
            var bsb = new BitStringBuffer(9);

            Assert.Throws<IndexOutOfRangeException>(() => bsb[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => bsb[9]);
        }

        [Fact]
        public void SetBits()
        {
            var bsb = new BitStringBuffer(22);
            bsb[12] = true;
            bsb[14] = true;

            Assert.Equal(22, bsb.Length);
            for (int i = 0; i < 12; ++i)
            {
                Assert.Equal(false, bsb[i]);
            }
            Assert.Equal(true, bsb[12]);
            Assert.Equal(false, bsb[13]);
            Assert.Equal(true, bsb[14]);
            for (int i = 15; i < 22; ++i)
            {
                Assert.Equal(false, bsb[i]);
            }

            Assert.Equal(3, bsb.Buffer.Length);
            // 07-00: 00000000
            // 15-08: 01010000
            // 22-16:   000000
            Assert.Equal(0x00, bsb.Buffer[0]);
            Assert.Equal(0x50, bsb.Buffer[1]);
            Assert.Equal(0x00, bsb.Buffer[2]);
        }
    }
}
