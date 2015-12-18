using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RavuAlHemio.OneWire;
using RavuAlHemio.OneWire.Adapter;

namespace OneWireSerialDriver
{
    public class SerialPortAdapter : DSPortAdapter
    {
        private SerialPort _serial;
        private bool _adapterPresent;

        public override string AdapterName => "DS9097U";
        public override string PortTypeDescription => "Serial";
        public override string ClassVersion => "0.01";

        public override IEnumerable<string> PortNames => SerialPort.GetPortNames();

        public override bool TrySelectPort(string portName)
        {
            _serial?.Close();

            _serial = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
            {
                ReadBufferSize = 4096,
                WriteBufferSize = 16,
                ReadTimeout = 500,
                WriteTimeout = -1,
                DiscardNull = false
            };
            try
            {
                _serial.Open();
                _serial.DtrEnable = true;
                _serial.RtsEnable = true;

                Thread.Sleep(20);


            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public override void FreePort()
        {
            throw new NotImplementedException();
        }

        public override string PortName { get; }
        public override bool AdapterDetected { get; }
        public override bool FindFirstDevice()
        {
            throw new NotImplementedException();
        }

        public override bool FindNextDevice()
        {
            throw new NotImplementedException();
        }

        public override OneWireAddress GetAddress()
        {
            throw new NotImplementedException();
        }

        public override void SetSearchOnlyAlarmingDevices()
        {
            throw new NotImplementedException();
        }

        public override void SetNoResetSearch()
        {
            throw new NotImplementedException();
        }

        public override void SetSearchAllDevices()
        {
            throw new NotImplementedException();
        }

        public override bool BeginExclusive(TimeSpan? timeout = null)
        {
            throw new NotImplementedException();
        }

        public override void EndExclusive()
        {
            throw new NotImplementedException();
        }

        public override void PutBit(bool bitValue)
        {
            throw new NotImplementedException();
        }

        public override bool GetBit()
        {
            throw new NotImplementedException();
        }

        public override void PutByte(byte byteValue)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte()
        {
            throw new NotImplementedException();
        }

        public override void DataBlock(IList<byte> bytes, int offset, int len)
        {
            throw new NotImplementedException();
        }

        public override ResetResult Reset()
        {
            throw new NotImplementedException();
        }

        private bool IsAdapterPresent
        {
            get
            {
                if (_adapterPresent)
                {
                    // adapter already verified to be present
                    return true;
                }

                // do a master reset
                PortMasterReset();

                // check
                if (PortVerify())
                {
                    _adapterPresent = true;
                    return true;
                }

                // do a master reset again
                PortMasterReset();

                // check
                if (PortVerify())
                {
                    _adapterPresent = true;
                    return true;
                }

                // do a power reset and try again
                PortPowerReset();

                // check one last time
                _adapterPresent = PortVerify();
                return _adapterPresent;
            }
        }

        private void PortMasterReset()
        {
            try
            {
                _serial.BaudRate = 9600;
                
            }
            catch (IOException e)
            {
                throw new OneWireIOException("failed to perform port master reset", e);
            }
        }
    }
}
