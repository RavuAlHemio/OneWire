using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Container;

namespace RavuAlHemio.OneWire.Adapter
{
    /// <summary>
    /// This <see cref="DSPortAdapter"/> class was designed to be used for testing purposes. The
    /// <see cref="DumbAdapter"/> allows programmers to add and remove <see cref="OneWireContainer"/> objects that will
    /// be found in its search.
    /// </summary>
    /// <remarks>
    /// Note that methods such as <see cref="TrySelectPort(string)"/> do nothing by default. This class is mainly meant
    /// for debugging using an emulated iButton.
    /// </remarks>
    public class DumbAdapter : DSPortAdapter
    {
        private readonly List<OneWireContainer> _containers = new List<OneWireContainer>();
        private int _containerIndex = 0;
        private readonly object _lock = new object();

        public void AddContainer([NotNull] OneWireContainer container)
        {
            lock (_containers)
            {
                _containers.Add(container);
            }
        }

        public void RemoveContainer([NotNull] OneWireContainer container)
        {
            lock (_containers)
            {
                _containers.Remove(container);
            }
        }

        public override string AdapterName => "DumbAdapter";
        public override string PortTypeDescription => "Virtual Emulated Port";
        public override string ClassVersion => "0.00";

        public override IEnumerable<string> PortNames
        {
            get { yield return "NULL0"; }
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void RegisterOneWireContainerClass(byte family, Type oneWireContainerClass)
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override bool TrySelectPort(string portName)
        {
            return true;
        }

        public override void FreePort()
        {
            // woohoo
        }

        /// <summary>
        /// Retrieves the name of the selected port. Always <c>"NULL0"</c>.
        /// </summary>
        public override string PortName => "NULL0";

        /// <summary>
        /// In <see cref="DumbAdapter"/>, the adapter is always detected.
        /// </summary>
        public override bool AdapterDetected => true;

        /// <summary>
        /// To make sure that a wide variety of applications can use it, <see cref="DumbAdapter"/>'s implementation of
        /// <see cref="SupportsSpeed"/> always returns <c>true</c>.
        /// </summary>
        public override bool SupportsSpeed(NetworkSpeed speed)
        {
            return true;
        }

        /// <summary>
        /// To make sure that a wide variety of applications can use it, <see cref="DumbAdapter"/>'s implementation of
        /// <see cref="SupportsLevel"/> always returns <c>true</c>.
        /// </summary>
        public override bool SupportsLevel(Level level)
        {
            return true;
        }

        public override bool FindFirstDevice()
        {
            lock (_containers)
            {
                if (_containers.Count > 0)
                {
                    _containerIndex = 1;
                    return true;
                }

                return false;
            }
        }

        public override bool FindNextDevice()
        {
            lock (_containers)
            {
                if (_containers.Count > _containerIndex)
                {
                    ++_containerIndex;
                    return true;
                }

                return false;
            }
        }

        public override OneWireAddress GetAddress()
        {
            OneWireContainer container;
            lock (_containers)
            {
                container = _containers[_containerIndex - 1];
            }

            return container.Address;
        }

        public override bool IsPresent(OneWireAddress address)
        {
            lock (_containers)
            {
                return _containers.Any(c => c.Address == address);
            }
        }

        public override bool Select(OneWireAddress address)
        {
            return IsPresent(address);
        }

        /// <summary>
        /// <see cref="DumbAdapter"/> does not support alarm states and always returns <c>false</c>.
        /// </summary>
        public override bool IsAlarming(OneWireAddress address)
        {
            return false;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void SetSearchOnlyAlarmingDevices()
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void SetNoResetSearch()
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void SetSearchAllDevices()
        {
        }
        
        public override bool BeginExclusive(TimeSpan? timeout = null)
        {
            if (timeout.HasValue)
            {
                return Monitor.TryEnter(_lock, timeout.Value);
            }
            else
            {
                Monitor.Enter(_lock);
                return true;
            }
        }

        public override void EndExclusive()
        {
            Monitor.Exit(_lock);
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void PutBit(bool bitValue)
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        /// <returns><c>true</c></returns>
        public override bool GetBit()
        {
            return true;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void PutByte(byte byteValue)
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        /// <returns><c>0xFF</c></returns>
        public override byte GetByte()
        {
            return 0xFF;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void DataBlock(IList<byte> bytes, int offset, int len)
        {
        }

        public override ResetResult Reset()
        {
            if (_containers.Count > 0)
            {
                return ResetResult.Presence;
            }
            return ResetResult.NoPresence;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void SetPowerDuration(PowerDeliveryDuration deliveryDuration)
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        /// <returns><c>true</c></returns>
        public override bool StartPowerDelivery(PowerStateChangeCondition when)
        {
            return true;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void SetProgramPulseDuration(PowerDeliveryDuration deliveryDuration)
        {
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        /// <returns><c>true</c></returns>
        public override bool StartProgramPulse(PowerStateChangeCondition when)
        {
            return true;
        }

        /// <summary>
        /// This method does nothing in <see cref="DumbAdapter"/>.
        /// </summary>
        public override void StartBreak()
        {
        }

        public override NetworkSpeed Speed { get; set; } = NetworkSpeed.Regular;

        public override OneWireContainer GetDeviceContainer(OneWireAddress address)
        {
            lock (_containers)
            {
                return _containers.FirstOrDefault(c => c.Address == address);
            }
        }
    }
}
