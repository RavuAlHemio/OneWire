using System;
using RavuAlHemio.OneWire;
using Xunit;

namespace OneWireTests
{
    public class CrcTests
    {
        [Fact]
        public void AppNote27Crc16Example()
        {
            // https://www.maximintegrated.com/en/app-notes/index.mvp/id/27

            ushort currentCrc = 0x90F1;
            byte dataByte = 0x75;

            Assert.Equal(0x6390, Crc16.Calc(dataByte, currentCrc));
        }
    }
}

