using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using LibUsbDotNet;
using LibUsbDotNet.LibUsb;
using OneWireUSBDriver.DS2490;
using RavuAlHemio.OneWire;
using RavuAlHemio.OneWire.Adapter;
using RavuAlHemio.OneWire.Utils;

namespace OneWireUSBDriver
{
    public class USBPortAdapter : DSPortAdapter
    {
        private PowerLevel _usbLevel;
        private NetworkSpeed _usbSpeed;
        private byte _usbVersion;
        private byte _usbVpp;
        
        [CanBeNull]
        private DS2490Communicator _communicator;

        [CanBeNull]
        private UsbDevice _usbDevice;

        [CanBeNull]
        private string _activePortName;

        private int _searchLastDiscrepancy = 0;
        private int _searchFamilyLastDiscrepancy = 0;
        private bool _searchLastDevice = false;
        private byte[] _searchAddressBytes = new byte[8];

        private static readonly Regex PortNameFormat = new Regex("^DS2490-(0|[1-9][0-9]{0,2})$");

        public override string AdapterName => "DS2490";
        public override string PortTypeDescription => "USB adapter";
        public override string ClassVersion => "0.01";

        public override IEnumerable<string> PortNames
        {
            get
            {
                // find all devices
                int devNum = 0;
                var ret = new List<string>();

                foreach (var device in LibUsbRegistry.DeviceList.Where(device => device.Vid == 0x04FA && device.Pid == 0x2490))
                {
                    ret.Add(string.Format(CultureInfo.InvariantCulture, "DS2490-{0}", devNum));
                    ++devNum;
                }

                return ret;
            }
        }

        public override bool TrySelectPort(string portName)
        {
            var portNameMatch = PortNameFormat.Match(portName);
            if (!portNameMatch.Success)
            {
                throw new OneWireIOException("Invalid USB adapter port name.");
            }

            var portNumber = int.Parse(portNameMatch.Groups[1].Value);

            using (new ExclusivePortAccess(this))
            {
                // find the device
                var device = LibUsbRegistry.DeviceList
                    .Where(d => d.Vid == 0x04FA && d.Pid == 0x2490)
                    .Skip(portNumber)
                    .FirstOrDefault();
                if (device == null)
                {
                    throw new OneWireIOException("USB adapter not found.");
                }

                // attempt to open the device
                if (!device.Open(out _usbDevice))
                {
                    throw new OneWireIOException("Failed to open USB device.");
                }

                var wholeDevice = (_usbDevice as IUsbDevice);
                if (wholeDevice != null)
                {
                    if (!wholeDevice.SetConfiguration(1))
                    {
                        _usbDevice.Close();
                        throw new OneWireIOException("Failed to set USB device configuration.");
                    }

                    if (!wholeDevice.ClaimInterface(0))
                    {
                        _usbDevice.Close();
                        throw new OneWireIOException("Failed to claim USB device interface.");
                    }

                    if (!wholeDevice.SetAltInterface(3))
                    {
                        wholeDevice.ReleaseInterface(0);
                        _usbDevice.Close();
                        throw new OneWireIOException("Failed to set alternative USB device interface.");
                    }
                }

                // clear endpoints before doing anything with them
                using (var reader = _usbDevice.OpenEndpointReader(DS2490Communicator.Endpoint1))
                {
                    reader.Reset();
                }
                using (var writer = _usbDevice.OpenEndpointWriter(DS2490Communicator.Endpoint2))
                {
                    writer.Reset();
                }
                using (var reader = _usbDevice.OpenEndpointReader(DS2490Communicator.Endpoint3))
                {
                    reader.Reset();
                }

                // verify that the adapter is working
                if (!AdapterRecover())
                {
                    wholeDevice?.ReleaseInterface(0);
                    _usbDevice.Close();
                    throw new OneWireIOException("USB adapter is not working.");
                }

                _communicator = new DS2490Communicator(_usbDevice);
                _activePortName = portName;
                return true;
            }
        }

        private bool AdapterRecover()
        {
            if (!_communicator.Detect())
            {
                return false;
            }

            _usbSpeed = NetworkSpeed.Regular;
            _usbLevel = PowerLevel.Normal;
            return true;
        }

        public override void FreePort()
        {
            (_usbDevice as IUsbDevice)?.ReleaseInterface(0);
            _usbDevice.Close();

            _communicator = null;
            _usbDevice = null;
            _activePortName = null;
        }

        public override string PortName => _activePortName;
        public override bool AdapterDetected => _usbDevice != null;

        public override bool FindFirstDevice()
        {
            using (new ExclusivePortAccess(this))
            {
                _searchLastDiscrepancy = 0;
                _searchFamilyLastDiscrepancy = 0;
                _searchLastDevice = false;

                return FindNextDevice();
            }
        }

        public override bool FindNextDevice()
        {
            using (new ExclusivePortAccess(this))
            {
                if (_searchLastDevice)
                {
                    _searchLastDiscrepancy = 0;
                    _searchFamilyLastDiscrepancy = 0;
                    _searchLastDevice = false;

                    return false;
                }

                // check for "first" and only 1 target
                if (_searchLastDiscrepancy == 0 && !_searchLastDevice && SearchIncludeFamilies.Count == 1)
                {
                    // set the search to find 1 target first
                    _searchLastDiscrepancy = 64;

                    // set an ID (family code and zero bytes)
                    _searchAddressBytes[0] = SearchIncludeFamilies.First();
                    for (int i = 1; i < 8; ++i)
                    {
                        _searchAddressBytes[i] = 0x00;
                    }
                }

                // loop until the correct type is found or no more devices
                bool found;
                do
                {
                    // perform a search and keep the result
                    found = Search();

                    if (found)
                    {
                        bool acceptable = true;

                        if (SearchExcludeFamilies != null && SearchExcludeFamilies.Contains(_searchAddressBytes[0]))
                        {
                            acceptable = false;
                        }

                        if (acceptable && SearchIncludeFamilies != null && !SearchIncludeFamilies.Contains(_searchAddressBytes[0]))
                        {
                            acceptable = false;
                        }

                        if (acceptable)
                        {
                            return true;
                        }
                    }

                    // skip the current type if this is not the last device
                    if (!_searchLastDevice && _searchFamilyLastDiscrepancy != 0)
                    {
                        _searchLastDiscrepancy = _searchFamilyLastDiscrepancy;
                        _searchFamilyLastDiscrepancy = 0;
                        _searchLastDevice = false;
                    }
                    else
                    {
                        // end of search; reset and return
                        _searchLastDiscrepancy = 0;
                        _searchFamilyLastDiscrepancy = 0;
                        _searchLastDevice = false;
                        found = false;
                    }

                } while (found);
            }
        }

        private bool Search()
        {
            if (_usbLevel != PowerLevel.Normal)
            {
                SetPowerNormal();
            }

            
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
    }
}
