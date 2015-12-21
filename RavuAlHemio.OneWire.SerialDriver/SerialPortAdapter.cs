//---------------------------------------------------------------------------
// Copyright © 2004 Maxim Integrated Products, All Rights Reserved.
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
using System.IO;
using System.IO.Ports;
using System.Threading;
using RavuAlHemio.OneWire.Adapter;
using RavuAlHemio.OneWire.SerialDriver.Internals;
using RavuAlHemio.OneWire.Utils;

namespace RavuAlHemio.OneWire.SerialDriver
{
    public class SerialPortAdapter : DSPortAdapter
    {
        private SerialPort _serial;
        private bool _adapterPresent;

        protected OneWireSerialState State { get; set; }

        public override string AdapterName => "DS9097U";
        public override string PortTypeDescription => "Serial";
        public override string ClassVersion => "0.01";

        public override IEnumerable<string> PortNames => SerialPort.GetPortNames();

        public override bool TrySelectPort(string portName)
        {
            bool isPresent = false;

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

                isPresent = TryEnsuringAdapterPresent();
                if (isPresent)
                {
                    return true;
                }
            }
            finally
            {
                if (isPresent && _serial.IsOpen)
                {
                    _serial.Close();
                }
            }
            return false;
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
            // acquire exclusive use of the port
            using (new ExclusivePortAccess(this))
            {
                // make sure the adapter is present
                if (!TryEnsuringAdapterPresent())
                {
                    throw new OneWireException("error communicating with adapter");
                }

                // check for pending power conditions
                if (State.Level != PowerLevel.Normal)
                {
                    SetPowerNormal();
                }

                // build a message to read the baud rate from the U brick
                
            }
        }

        private bool TryEnsuringAdapterPresent()
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
