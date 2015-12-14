using System;
using RavuAlHemio.OneWire;
using Xunit;

namespace OneWireTests
{
    public class OneWireAddressTests
    {
        [Fact]
        public void TestByteArrayRoundtrip()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            var addr = new OneWireAddress(addrBytes);
            var retBytes = addr.ToByteArray();
            Assert.Equal(addrBytes, retBytes);
        }

        [Fact]
        public void TestFromStringToByteArray()
        {
            var expectedBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const string addrStr = "F300000014E92810";

            var addr = new OneWireAddress(addrStr);
            var retBytes = addr.ToByteArray();
            Assert.Equal(expectedBytes, retBytes);
        }

        [Fact]
        public void TestFromLongToByteArray()
        {
            var expectedBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const ulong addrULong = 0xF300000014E92810L;
            const long addrLong = unchecked((long)addrULong);

            var addr = new OneWireAddress(addrLong);
            var retBytes = addr.ToByteArray();
            Assert.Equal(expectedBytes, retBytes);
        }

        [Fact]
        public void TestToString()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const string addrStr = "F300000014E92810";

            var addr = new OneWireAddress(addrBytes);
            var retStr = addr.ToString();
            Assert.Equal(addrStr, retStr);
        }

        [Fact]
        public void TestToLong()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const ulong addrULong = 0xF300000014E92810L;
            const long addrLong = unchecked((long)addrULong);

            var addr = new OneWireAddress(addrBytes);
            var retLong = addr.ToLong();
            Assert.Equal(addrLong, retLong);
        }

        [Fact]
        public void TestInvalidCRC()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xAB};
            const string addrStr = "AB00000014E92810";
            const ulong addrULong = 0xAB00000014E92810L;
            const long addrLong = unchecked((long)addrULong);

            Assert.Throws<ArgumentException>(() => new OneWireAddress(addrBytes));
            Assert.Throws<ArgumentException>(() => new OneWireAddress(addrStr));
            Assert.Throws<ArgumentException>(() => new OneWireAddress(addrLong));
        }

        [Fact]
        public void TestEqual()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const string addrStr = "F300000014E92810";
            const ulong addrULong = 0xF300000014E92810L;
            const long addrLong = unchecked((long)addrULong);

            var bAddr = new OneWireAddress(addrBytes);
            var sAddr = new OneWireAddress(addrStr);
            var lAddr = new OneWireAddress(addrLong);

            Assert.Equal(bAddr, bAddr);
            Assert.Equal(bAddr, sAddr);
            Assert.Equal(bAddr, lAddr);
            Assert.Equal(sAddr, bAddr);
            Assert.Equal(sAddr, sAddr);
            Assert.Equal(sAddr, lAddr);
            Assert.Equal(lAddr, bAddr);
            Assert.Equal(lAddr, sAddr);
            Assert.Equal(lAddr, lAddr);
        }

        [Fact]
        public void TestHashCode()
        {
            var addrBytes = new byte[] {0x10, 0x28, 0xE9, 0x14, 0x00, 0x00, 0x00, 0xF3};
            const string addrStr = "F300000014E92810";
            const ulong addrULong = 0xF300000014E92810L;
            const long addrLong = unchecked((long)addrULong);

            var bAddr = new OneWireAddress(addrBytes);
            var sAddr = new OneWireAddress(addrStr);
            var lAddr = new OneWireAddress(addrLong);

            Assert.Equal(bAddr.GetHashCode(), bAddr.GetHashCode());
            Assert.Equal(bAddr.GetHashCode(), sAddr.GetHashCode());
            Assert.Equal(bAddr.GetHashCode(), lAddr.GetHashCode());
            Assert.Equal(sAddr.GetHashCode(), bAddr.GetHashCode());
            Assert.Equal(sAddr.GetHashCode(), sAddr.GetHashCode());
            Assert.Equal(sAddr.GetHashCode(), lAddr.GetHashCode());
            Assert.Equal(lAddr.GetHashCode(), bAddr.GetHashCode());
            Assert.Equal(lAddr.GetHashCode(), sAddr.GetHashCode());
            Assert.Equal(lAddr.GetHashCode(), lAddr.GetHashCode());
        }
    }
}
