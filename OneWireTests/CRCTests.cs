using System.Collections.Generic;
using System.Linq;
using RavuAlHemio.OneWire.Utils;
using Xunit;

namespace OneWireTests
{
    public class CRCTests
    {
        struct TestTriplet
        {
            public readonly string TestString;
            public readonly byte Crc8;
            public readonly ushort Crc16;

            public TestTriplet(string testString, byte crc8, ushort crc16)
            {
                TestString = testString;
                Crc8 = crc8;
                Crc16 = crc16;
            }

            // WARNING: absolutely not Unicode-compatible.
            public byte[] StringBytes => TestString.Select(c => (byte) c).ToArray();
        }

        [Fact]
        public void TestCrcEmpty()
        {
            var empty = new byte[0];
            const byte reference8 = 0x00;
            const ushort reference16 = 0x0000;

            Assert.Equal(reference8, CRC8.Compute(empty));
            Assert.Equal(reference16, CRC16.Compute(empty));
        }

        [Fact]
        public void TestCrcStrings()
        {
            var tests = new List<TestTriplet>
            {
                new TestTriplet("Hello, world!", 0x2A, 0x9A4A),
                new TestTriplet("Test string?", 0xCC, 0xE51F)
            };

            foreach (var test in tests)
            {
                Assert.Equal(test.Crc8, CRC8.Compute(test.StringBytes));
                Assert.Equal(test.Crc16, CRC16.Compute(test.StringBytes));
            }
        }
    }
}
