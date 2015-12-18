//---------------------------------------------------------------------------
// Copyright © 1999, 2000 Maxim Integrated Products, All Rights Reserved.
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
using System.Globalization;
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Container;
using RavuAlHemio.OneWire.Utils;

namespace RavuAlHemio.OneWire.Adapter
{
    /// <summary>
    /// The abstract base class for all 1-Wire port adapter objects.  An implementation class of this type is therefore
    /// independent of the adapter type.  Instances of valid DSPortAdapters are retrieved from methods in
    /// <see cref="OneWireAccessProvider"/>.
    /// </summary>
    public abstract class DSPortAdapter
    {
        /// <summary>
        /// 1-Wire Network reset result.
        /// </summary>
        public enum ResetResult : int
        {
            /// <summary>
            /// No devices are present on the 1-Wire Network.
            /// </summary>
            NoPresence = 0x00,

            /// <summary>
            /// Normal presence pulse detected on the 1-Wire Network; a device is present.
            /// </summary>
            Presence = 0x01,

            /// <summary>
            /// Alarming presence pulse detected on the 1-Wire Network; a device in an alarm condition is present.
            /// </summary>
            Alarm = 0x02,

            /// <summary>
            /// The 1-Wire appears to be short-circuited. This can be a transient condition in a 1-Wire Network. Not
            /// all adapter types can detect this condition.
            /// </summary>
            Short = 0x03
        }

        /// <summary>
        /// Condition for power state change.
        /// </summary>
        public enum PowerStateChangeCondition : int
        {
            /// <summary>
            /// Change the power state immediately.
            /// </summary>
            Now = 0,

            /// <summary>
            /// Change the power state after the next bit is sent.
            /// </summary>
            AfterBit = 1,
                
            /// <summary>
            /// Change the power state after the next byte is sent.
            /// </summary>
            AfterByte = 2
        }

        /// <summary>
        /// Dictionary to contain the user-replaced OneWireContainers.
        /// </summary>
        [NotNull]
        private readonly Dictionary<byte, Type> _registeredOneWireContainerClasses = new Dictionary<byte, Type>(5);

        /// <summary>
        /// Families to include in search.
        /// </summary>
        protected ISet<byte> SearchIncludeFamilies { get; set; }

        /// <summary>
        /// Families to exclude from search.
        /// </summary>
        protected ISet<byte> SearchExcludeFamilies { get; set; }

        /// <summary>
        /// The name of the port adapter as a string. The "Adapter" is a device that connects to a "port" that allows
        /// one to communicate with an iButton or other 1-Wire device.  An example of a port adapter name is "DS9097U".
        /// </summary>
        /// <value>The name of the port adapter as a string.</value>
        public abstract string AdapterName { get; }

        /// <summary>
        /// A description of the port required by this port adapter. An example of such a port is "serial communication
        /// port".
        /// </summary>
        /// <value>A description of the port required by this port adapter.</value>
        public abstract string PortTypeDescription { get; }

        /// <summary>
        /// The version string for this class.
        /// </summary>
        /// <value>The version string for this class.</value>
        public abstract string ClassVersion { get; }

        /// <summary>
        /// A list of the platform-appropriate port names for this adapter. A port must be selected with the method
        /// <see cref="TrySelectPort"/> before any other communication methods can be used. Using a communcation method
        /// before <see cref="TrySelectPort"/> will result in a <see cref="OneWireException"/>.
        /// </summary>
        /// <value>A list of the platform-appropriate port names for this adapter.</value>
        public abstract IEnumerable<string> PortNames { get; }

        /// <summary>
        /// Registers a user-provided <see cref="OneWireContainer"/> class. Using this method will override the Maxim
        /// Integrated Products-provided container class when using the <see cref="DeviceContainer"/> property. The
        /// registered container state is only stored for the current instance of <see cref="DSPortAdapter"/>, and is
        /// not statically shared.
        ///
        /// The <paramref name="oneWireContainerClass"/> must extend
        /// <see cref="RavuAlHemio.OneWire.Container.OneWireContainer"/>; otherwise, an
        /// <see cref="InvalidCastException"/> is thrown.
        /// 
        /// If a class has been previously registered for the specified family, it is overwritten. Passing <c>null</c>
        /// as a parameter for <paramref name="oneWireContainerClass"/> will result in the removal of any entry
        /// associated with that family.
        /// </summary>
        /// <param name="family">The code of the family type to associate with this class.</param>
        /// <param name="oneWireContainerClass">The user-provided class to handle that family.</param>
        /// <exception cref="InvalidCastException">
        /// Thrown if <paramref name="oneWireContainerClass"/> does not extend
        /// <see cref="RavuAlHemio.OneWire.Container.OneWireContainer"/>.
        /// </exception>
        public virtual void RegisterOneWireContainerClass(byte family, [CanBeNull] Type oneWireContainerClass)
        {
            Type defaultIButtonClass = typeof(OneWireContainer);

            if (oneWireContainerClass == null)
            {
                // If null is passed, remove the old container class.
                _registeredOneWireContainerClasses.Remove(family);
            }
            else
            {
                if (defaultIButtonClass.IsAssignableFrom(oneWireContainerClass))
                {
                    _registeredOneWireContainerClasses[family] = oneWireContainerClass;
                }
                else
                {
                    throw new InvalidCastException("oneWireContainerClass does not extend " + defaultIButtonClass.FullName);
                }
            }
        }

        /// <summary>
        /// Attempts to acquire the port with the given name through this adapter. Note that even though the port has
        /// been selected, itss ownership may be relinquished if it is not currently held in an "exclusive" block. This
        /// class will then try to re-acquire the port when needed. If the port cannot be re-acquired, a
        /// <see cref="PortInUseException"/> is thrown.
        /// </summary>
        /// <param name="portName">Name of the port to select, retrieved from <see cref="PortNames"/>.</param>
        /// <returns><c>true</c> if the port was acquired, <c>false</c> if it is not available.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown if the port does not exist or the library was unable to communicate with it.
        /// </exception>
        public abstract bool TrySelectPort([NotNull] string portName);

        /// <summary>
        /// Relinquishes ownership of the currently selected port back to the system. This should only be called if the
        /// port does not have an adapter or when the application does not need the port anymore.
        /// </summary>
        /// <exception cref="OneWireException">If the currently selected port does not exist.</exception>
        public abstract void FreePort();

        /// <summary>
        /// The name of the currently selected port, or <c>null</c> if none has been selected-
        /// </summary>
        /// <value>The name of the currently selected port.</value>
        [CanBeNull]
        public abstract string PortName { get; }

        /// <summary>
        /// Whether an adapter is confirmed to be connected to the selected port.
        /// </summary>
        /// <value><c>true</c> if an adapter is connected; <c>false</c> otherwise.</value>
        public abstract bool AdapterDetected { get; }

        /// <summary>
        /// The version of the adapter.
        /// </summary>
        /// <value>
        /// The version of the adapter. <c>"&lt;na&gt;"</c> if the adapter version is unknown or cannot be known.
        /// </value>
        /// <exception cref="OneWireIOException">
        /// Thrown if a communication error occurs, e.g. no device is present. This can be caused by a physical
        /// interruption in the 1-Wire Network due to a short-circuit or a newly arriving 1-Wire device issuing a
        /// "presence pulse".
        /// </exception>
        /// <exception cref="OneWireException">
        /// Thrown on a communication or setup error with the 1-Wire adapter.
        /// </exception>
        [NotNull]
        public virtual string AdapterVersion => "<na>";

        /// <summary>
        /// The address of the adapter, if it has one.
        /// </summary>
        /// <value>
        /// The address of the adapter. <c>"&lt;na&gt;"</c> if the adapter does not have an address. The address is a
        /// string representation of a 1-Wire address.
        /// </value>
        /// <exception cref="OneWireIOException">
        /// Thrown if a communication error occurs, e.g. no device is present. This can be caused by a physical
        /// interruption in the 1-Wire Network due to a short-circuit or a newly arriving 1-Wire device issuing a
        /// "presence pulse".
        /// </exception>
        /// <exception cref="OneWireException">
        /// Thrown on a communication or setup error with the 1-Wire adapter.
        /// </exception>
        [NotNull]
        public virtual string AdapterAddress => "<na>";

        /// <summary>
        /// Returns whether this adapter physically supports the given speed.
        /// </summary>
        /// <param name="speed">The speed whose support to check.</param>
        /// <returns><c>true</c> if the supplied speed is supported; <c>false</c> otherwise.</returns>
        public virtual bool SupportsSpeed(NetworkSpeed speed)
        {
            if (speed == NetworkSpeed.Regular)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this adapter physically supports the given power level.
        /// </summary>
        /// <param name="level">The power level whose support to check.</param>
        /// <returns><c>true</c> if the supplied power level is supported; <c>false</c> otherwise.</returns>
        public virtual bool SupportsLevel(PowerLevel level)
        {
            if (level == PowerLevel.Normal)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether this adapter physically supports "smart" strong 5V power mode. "Smart" power delivery is the ability
        /// to deliver power until it is no longer needed. The current drop is detected and power delivery is stopped.
        /// </summary>
        /// <value><c>true</c> if this port adapter supports "smart" strong 5V power mode; <c>false</c> otherwise.</value>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool CanDeliverSmartPower => false;

        //#if HALF_IMPLEMENTED
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="OneWireContainer"/> objects corresponding to all of
        /// the iButtons or 1-Wire devices found on the 1-Wire Network. If no devices are found, then an empty
        /// enumerable will be returned. In most cases, all further communication iwth the device is done through the
        /// <see cref="OneWireContainer"/>.
        /// </summary>
        /// <returns>
        /// <see cref="IEnumerable{T}"/> of <see cref="OneWireContainer"/> objects found on the 1-Wire Network.
        /// </returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        [NotNull]
        public virtual IEnumerable<OneWireContainer> GetAllDeviceContainers()
        {
            var ibuttonList = new List<OneWireContainer>();

            OneWireContainer tempIButton = GetFirstDeviceContainer();
            if (tempIButton != null)
            {
                ibuttonList.Add(tempIButton);

                // loop to get all of the ibuttons
                do
                {
                    tempIButton = GetNextDeviceContainer();

                    if (tempIButton != null)
                    {
                        ibuttonList.Add(tempIButton);
                    }
                } while (tempIButton != null);
            }

            return ibuttonList;
        }

        /// <summary>
        /// Returns a <see cref="OneWireContainer"/> object corresponding to the first iButton or 1-Wire device found on
        /// the 1-Wire Network. If no devices are found, then <c>null</c> is returned. In most cases, all further
        /// communication with the device is done through the <see cref="OneWireContainer"/>.
        /// </summary>
        /// <returns>
        /// The first <see cref="OneWireContainer"/> object found on the 1-Wire Network, or <c>null</c> if no devices
        /// are found.
        /// </returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        [CanBeNull]
        public virtual OneWireContainer GetFirstDeviceContainer()
        {
            if (FindFirstDevice())
            {
                return DeviceContainer;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="OneWireContainer"/> object corresponding to the next iButton or 1-Wire device found on
        /// the 1-Wire Network. The previous 1-Wire device found is used as a starting point in the search. If no more
        /// devices are found, then <c>null</c> is returned. In most cases, all further communication with the device is
        /// done through the <see cref="OneWireContainer"/>.
        /// </summary>
        /// <returns>
        /// The next <see cref="OneWireContainer"/> object found on the 1-Wire Network, or <c>null</c> if no more
        /// devices are found.
        /// </returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        [CanBeNull]
        public OneWireContainer GetNextDeviceContainer()
        {
            if (FindNextDevice())
            {
                return DeviceContainer;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the first iButton or 1-Wire device is found on the 1-Wire Network. If no devices are
        /// found, then <c>false</c> is returned.
        /// </summary>
        /// <returns><c>true</c> if an iButton or 1-Wire device is found; <c>false</c> otherwise.</returns>
        public abstract bool FindFirstDevice();

        /// <summary>
        /// Returns <c>true</c> if the next iButton or 1-Wire device is found on the 1-Wire Network. If no more devices
        /// are found, then <c>false</c> is returned.
        /// </summary>
        /// <returns><c>true</c> if another iButton or 1-Wire device is found; <c>false</c> otherwise.</returns>
        public abstract bool FindNextDevice();

        /// <summary>
        /// Returns the address of the last found 1-Wire device.
        /// </summary>
        /// <returns>The address of the last found 1-Wire device.</returns>
        public abstract OneWireAddress GetAddress();

        /// <summary>
        /// Verifies that the 1-Wire device with the specified address is present on the 1-Wire Network.
        /// </summary>
        /// <remarks>
        /// This does not affect the "current" device state information used in searches (<see cref="FindFirstDevice"/>,
        /// <see cref="FindNextDevice"/>).
        /// </remarks>
        /// <param name="address">The address of the device whose presence to detect.</param>
        /// <returns><c>true</c> if the device is present; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool IsPresent(OneWireAddress address)
        {
            Reset();
            PutByte(0xF0);  // Search ROM command
            return StrongAccess(address);
        }

        /// <summary>
        /// Verifies that the 1-Wire device with the specified address is present on the 1-Wire Network and in an alarm
        /// state.
        /// </summary>
        /// <remarks>
        /// This does not affect the "current" device state information used in searches (<see cref="FindFirstDevice"/>,
        /// <see cref="FindNextDevice"/>).
        /// </remarks>
        /// <param name="address">The address of the device whose presence and alarm state to detect.</param>
        /// <returns><c>true</c> if the device is present and in an alarm state; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool IsAlarming(OneWireAddress address)
        {
            Reset();
            PutByte(0xEC);  // Conditional Search command
            return StrongAccess(address);
        }

        /// <summary>
        /// Selects the specified 1-Wire device by broadcasting its address.
        /// </summary>
        /// <remarks>
        /// This operation is referred to as a "MATCH ROM" operation in the 1-Wire device data sheets. It does not
        /// affect the "current" device state information used in searches (<see cref="FindFirstDevice"/>,
        /// <see cref="FindNextDevice"/>). Note that this operation does not verify whether the device is currently
        /// present on the 1-Wire Network (see <see cref="IsPresent"/>).
        /// </remarks>
        /// <param name="address">The address of the device to select.</param>
        /// <returns><c>true</c> if the device address was sent successfully; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool Select(OneWireAddress address)
        {
            // send 1-Wire Reset
            ResetResult resetStatus = Reset();

            // broadcast the MATCH ROM command and address
            var sendPacket = new byte[9];
            sendPacket[0] = 0x55; // MATCH ROM command
            for (int i = 0; i < 8; ++i)
            {
                sendPacket[i + 1] = address[i];
            }
            DataBlock(sendPacket, 0, sendPacket.Length);

            // success if any device present on 1-Wire Network
            return (resetStatus == ResetResult.Presence || resetStatus == ResetResult.Alarm);
        }

        /// <summary>
        /// Selects the specified 1-Wire device by broadcasting its address. If no device is found on the 1-Wire
        /// Network, throws a <see cref="OneWireException"/>.
        /// </summary>
        /// <remarks>
        /// This operation is referred to as a "MATCH ROM" operation in the 1-Wire device data sheets. It does not
        /// affect the "current" device state information used in searches (<see cref="FindFirstDevice"/>,
        /// <see cref="FindNextDevice"/>). Note that this operation does not verify whether the device is currently
        /// present on the 1-Wire Network (see <see cref="IsPresent"/>).
        /// </remarks>
        /// <param name="address">The address of the device to select.</param>
        /// <returns><c>true</c> if the device address was sent successfully; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public void AssertSelect(OneWireAddress address)
        {
            if (!Select(address))
            {
                throw new OneWireException("Device " + address + " not present.");
            }
        }
        //#endif

        /// <summary>
        /// Sets the 1-Wire Network search to find only 1-Wire devices that are in an "alarm" state that signals a need
        /// for attention. This selective searching can be cancelled using <see cref="SetSearchAllDevices()"/>.
        /// </summary>
        /// <remarks>
        /// Only some iButtons, e.g. DS1994, DS1920 and DS2407, can enter an alarm state.
        /// </remarks>
        public abstract void SetSearchOnlyAlarmingDevices();

        /// <summary>
        /// Sets the 1-Wire Network search to not perform a 1-Wire reset before a search. This feature is chiefly used
        /// with the DS2409 1-Wire coupler. The normal reset before each search can be restored using
        /// <see cref="SetSearchAllDevices()"/>.
        /// </summary>
        public abstract void SetNoResetSearch();

        /// <summary>
        /// Sets the 1-Wire Network search to find all 1-Wire devices (whether they are in an "Alarm" state or not) and
        /// restores the default setting of providing a 1-Wire reset command before each search (behavior that can be
        /// deactivated by <see cref="SetNoResetSearch()"/>).
        /// </summary>
        public abstract void SetSearchAllDevices();

        /// <summary>
        /// Removes any selectivity during a search for 1-Wire devices by family type. The unique address for each
        /// 1-Wire device contains a family descriptor that indicates the capabilities of the device.
        /// </summary>
        public void TargetAllFamilies()
        {
            SearchIncludeFamilies = null;
            SearchExcludeFamilies = null;
        }

        /// <summary>
        /// Limits searches to the supplied family type. If this method is used, no devices of other families will be
        /// found by any of the search methods. The behavior can be reset using <see cref="TargetAllFamilies()"/>.
        /// </summary>
        /// <remarks>
        /// Exclusions (<see cref="ExcludeFamily(byte)"/>, <see cref="ExcludeFamilies(IEnumerable{byte})"/>) take
        /// precedence over inclusions (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>).
        /// </remarks>
        /// <param name="family">The code of the family to target exclusively during searches.</param>
        public void TargetFamily(byte family)
        {
            if (SearchIncludeFamilies == null)
            {
                SearchIncludeFamilies = new HashSet<byte>();
            }
            SearchIncludeFamilies.Clear();
            SearchIncludeFamilies.Add(family);
        }

        /// <summary>
        /// Limits searches to the supplied list of family types. If this method is used, no devices of other families
        /// will be found by any of the search methods. The behavior can be reset using
        /// <see cref="TargetAllFamilies()"/>.
        /// </summary>
        /// <remarks>
        /// Exclusions (<see cref="ExcludeFamily(byte)"/>, <see cref="ExcludeFamilies(IEnumerable{byte})"/>) take
        /// precedence over inclusions (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>).
        /// </remarks>
        /// <param name="families">The codes of the families to target exclusively during searches.</param>
        public void TargetFamilies(IEnumerable<byte> families)
        {
            if (SearchIncludeFamilies == null)
            {
                SearchIncludeFamilies = new HashSet<byte>();
            }
            SearchIncludeFamilies.Clear();
            SearchIncludeFamilies.UnionWith(families);
        }

        /// <summary>
        /// Limits searches to any but the supplied family type. If this method is used, no devices of this family will
        /// be found by any of the search methods. The behavior can be reset using <see cref="TargetAllFamilies()"/>.
        /// </summary>
        /// <remarks>
        /// Exclusions (<see cref="ExcludeFamily(byte)"/>, <see cref="ExcludeFamilies(IEnumerable{byte})"/>) take
        /// precedence over inclusions (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>).
        /// </remarks>
        /// <param name="family">The code of the family to ignore during searches.</param>
        public void ExcludeFamily(byte family)
        {
            if (SearchIncludeFamilies == null)
            {
                SearchIncludeFamilies = new HashSet<byte>();
            }
            SearchIncludeFamilies.Clear();
            SearchIncludeFamilies.Add(family);
        }

        /// <summary>
        /// Limits searches to any but the supplied list of family types. If this method is used, no devices of these
        /// families will be found by any of the search methods. The behavior can be reset using
        /// <see cref="TargetAllFamilies()"/>.
        /// </summary>
        /// <remarks>
        /// Exclusions (<see cref="ExcludeFamily(byte)"/>, <see cref="ExcludeFamilies(IEnumerable{byte})"/>) take
        /// precedence over inclusions (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>).
        /// </remarks>
        /// <param name="families">The codes of the families to ignore during searches.</param>
        public void ExcludeFamilies(IEnumerable<byte> families)
        {
            if (SearchIncludeFamilies == null)
            {
                SearchIncludeFamilies = new HashSet<byte>();
            }
            SearchIncludeFamilies.Clear();
            SearchIncludeFamilies.UnionWith(families);
        }

        /// <summary>
        /// Gets exclusive use of the 1-Wire to communicate with a 1-Wire device. This method should be used for
        /// critical sections of code where a sequence of commands must not be interrupted by communication of threads
        /// with other 1-Wire devices, and it is permissible to sustain a delay in the special case that another thread
        /// has already been granted exclusive access and this access has not yet been relinquished.
        /// </summary>
        /// <remarks>
        /// This method may be called through the <see cref="OneWireContainer"/> class by the end application if they
        /// want to ensure exclusive access. If it is not called around several methods, it will be called inside each
        /// method.
        /// </remarks>
        /// <param name="timeout">
        /// The time after which to give up attempting to obtain exclusive access, or <c>null</c> to block
        /// indefinitely.
        /// </param>
        /// <returns>
        /// <c>true</c> if exclusive access has been granted; <c>false</c> if the operation timed out beforehand.
        /// </returns>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract bool BeginExclusive(TimeSpan? timeout = null);

        /// <summary>
        /// Relinquishes exclusive control of the 1-Wire Network. This command dynamically marks the end of a critical
        /// section and should be used when exclusive control is no longer needed.
        /// </summary>
        public abstract void EndExclusive();

        /// <summary>
        /// Sends a bit to the 1-Wire Network.
        /// </summary>
        /// <param name="bitValue">The bit value to send to the 1-Wire Network.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract void PutBit(bool bitValue);

        /// <summary>
        /// Gets a bit from the 1-Wire Network.
        /// </summary>
        /// <returns>The bit value received from the 1-Wire Network.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract bool GetBit();

        /// <summary>
        /// Sends a byte to the 1-Wire Network.
        /// </summary>
        /// <param name="byteValue">The byte value to send to the 1-Wire Network.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract void PutByte(byte byteValue);

        /// <summary>
        /// Gets a byte from the 1-Wire Network.
        /// </summary>
        /// <returns>The byte value received from the 1-Wire Network.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract byte GetByte();

        /// <summary>
        /// Gets a block of data from the 1-Wire Network.
        /// </summary>
        /// <param name="len">Number of data bytes to receive.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="len"/> is less than zero.</exception>
        /// <returns>The data received from the 1-Wire Network.</returns>
        public virtual byte[] GetBlock(int len)
        {
            var ret = new byte[len];
            GetBlock(ret, 0, len);
            return ret;
        }

        /// <summary>
        /// Gets a block of data from the 1-Wire Network.
        /// </summary>
        /// <param name="bytes">The (pre-allocated) list into which to store the received bytes.</param>
        /// <param name="len">Number of data bytes to receive.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="len"/> is less than zero.
        /// </exception>
        public virtual void GetBlock([NotNull] IList<byte> bytes, int len)
        {
            GetBlock(bytes, 0, len);
        }

        /// <summary>
        /// Gets a block of data from the 1-Wire Network.
        /// </summary>
        /// <param name="bytes">The (pre-allocated) list into which to store the received bytes.</param>
        /// <param name="offset">
        /// The index in the list at which to store the first received byte; the remaining bytes are stored
        /// consecutively after this one.
        /// </param>
        /// <param name="len">Number of data bytes to receive.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="offset"/> or <paramref name="len"/> are less than zero.
        /// </exception>
        public virtual void GetBlock([NotNull] IList<byte> bytes, int offset, int len)
        {
            // store 0xFF at the relevant locations to force a read
            for (int i = 0; i < len; ++i)
            {
                bytes[offset + i] = 0xFF;
            }

            // transfer the data
            DataBlock(bytes, offset, len);
        }

        /// <summary>
        /// Sends a block of data and returns the data received in the same list. This method is used when sending a
        /// block that contains reads and writes. The "read" portions of the data block need to be pre-loaded with
        /// <c>0xFF</c>. The method sends data from the index at offset <paramref name="offset"/> for length
        /// <paramref name="len"/>.
        /// </summary>
        /// <param name="bytes">
        /// The list whose contents to send to and into which to store the data read from the 1-Wire Network.
        /// </param>
        /// <param name="offset">
        /// The index in the list at which to start reading bytes to send and at which to store received bytes. The
        /// remaining bytes are read from and stored to the consecutive indices.
        /// </param>
        /// <param name="len">Number of data bytes to send and receive.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter or if there are no devices on the 1-Wire Network.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="offset"/> or <paramref name="len"/> are less than zero.
        /// </exception>
        public abstract void DataBlock([NotNull] IList<byte> bytes, int offset, int len);

        /// <summary>
        /// Sends a reset to the 1-Wire Network.
        /// </summary>
        /// <returns>The result of the reset operation.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public abstract ResetResult Reset();

        /// <summary>
        /// Sets the duration to supply power to the 1-Wire Network. This method takes a time parameter that indicates
        /// the program pulse length when <see cref="StartPowerDelivery(PowerStateChangeCondition)"/> is called.
        /// </summary>
        /// <remarks>
        /// Verify the result of <see cref="SupportsLevel(Level)"/> with <see cref="Level.PowerDelivery"/> and the
        /// value of <see cref="CanDeliverSmartPower"/> before calling this method to avoid exceptions being thrown.
        /// </remarks>
        /// <param name="deliveryDuration">The duration for which to deliver power.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="deliveryDuration"/> is not an explicit time value or
        /// <see cref="PowerDeliveryDuration.Infinite"/>.
        /// </exception>
        public virtual void SetPowerDuration(PowerDeliveryDuration deliveryDuration)
        {
            throw new OneWireException("Power delivery not supported by this adapter type");
        }

        /// <summary>
        /// Sets the 1-Wire Network voltage to supply power to a 1-Wire device. This method takes a parameter that
        /// indicates whether the power delivery should be started immediately or after certain conditions have been
        /// met.
        /// </summary>
        /// <param name="when">
        /// Whether to change the power level immediately or after a unit of data is transferred.
        /// </param>
        /// <returns><c>true</c> if the voltage change was successful; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool StartPowerDelivery(PowerStateChangeCondition when)
        {
            throw new OneWireException("Power delivery not supported by this adapter type");
        }

        /// <summary>
        /// Sets the duration for providing a program pulse on the 1-Wire Network. This method takes a time parameter
        /// that indicates the program pulse length when <see cref="StartProgramPulse(PowerStateChangeCondition)"/> is
        /// called.
        /// </summary>
        /// <remarks>
        /// Verify the result of <see cref="SupportsLevel(Level)"/> with <see cref="Level.Program"/> before calling
        /// this method to avoid exceptions being thrown.
        /// </remarks>
        /// <param name="deliveryDuration">The duration for which to deliver power.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="deliveryDuration"/> is neither <see cref="PowerDeliveryDuration.EPROM"/> nor
        /// <see cref="PowerDeliveryDuration.Infinite"/>.
        /// </exception>
        public virtual void SetProgramPulseDuration(PowerDeliveryDuration deliveryDuration)
        {
            throw new OneWireException("Power delivery not supported by this adapter type");
        }

        /// <summary>
        /// Sets the 1-Wire Network voltage to the EPROM programming level. This method takes a parameter that
        /// indicates whether the power delivery should be started immediately or after certain conditions have been
        /// met.
        /// </summary>
        /// <param name="when">
        /// Whether to change the power level immediately or after a unit of data is transferred.
        /// </param>
        /// <returns><c>true</c> if the voltage change was successful; <c>false</c> otherwise.</returns>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual bool StartProgramPulse(PowerStateChangeCondition when)
        {
            throw new OneWireException("Power delivery not supported by this adapter type");
        }

        /// <summary>
        /// Sets the 1-Wire Network voltage to 0 volts. This method is used to terminate parasite power delivery to all
        /// 1-Wire Network devices, forcing them to perform a hard reset.
        /// </summary>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual void StartBreak()
        {
            throw new OneWireException("Break delivery not supported by this adapter type");
        }

        /// <summary>
        /// Sets the 1-Wire Network voltage to normal level. This method is used to revert the bus to the state before
        /// <see cref="StartPowerDelivery(PowerStateChangeCondition)"/> or
        /// <see cref="StartProgramPulse(PowerStateChangeCondition)"/>. This method is called automatically if a
        /// communication method is called while an outstanding power command is taking place.
        /// </summary>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual void SetPowerNormal()
        {
            // nothing to do if the adapter does not support other power levels anyway
        }

        /// <summary>
        /// Obtains or changes the data transfer speed of the 1-Wire Network.
        /// </summary>
        /// <exception cref="OneWireIOException">
        /// Thrown on a 1-Wire communication error with the adapter.
        /// </exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        public virtual NetworkSpeed Speed
        {
            get
            {
                return NetworkSpeed.Regular;
            }

            set
            {
                if (value != NetworkSpeed.Regular)
                {
                    throw new OneWireException("Non-regular 1-Wire speed not supported by this adapter type");
                }
            }
        }

        /// <summary>
        /// Constructs a <see cref="OneWireContainer"/> object for the user-supplied 1-Wire network address.
        /// </summary>
        /// <param name="address">The address of the device for which to create a new container.</param>
        /// <returns>A <see cref="OneWireContainer"/> for the device with the given address.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the container class for the device family does not match the contract (subtype of
        /// <see cref="OneWireContainer"/>, contains a constructor accepting a <see cref="DSPortAdapter"/> and a
        /// <see cref="OneWireAddress"/> in that order).
        /// </exception>
        public virtual OneWireContainer GetDeviceContainer(OneWireAddress address)
        {
            byte familyCode = (byte)(address.FamilyPortion & 0x7F);
            string familyString = familyCode.ToString("X2", CultureInfo.InvariantCulture);
            Type iButtonType = null;

            // Does a user-registered button exist?
            if (_registeredOneWireContainerClasses.ContainsKey(familyCode))
            {
                iButtonType = _registeredOneWireContainerClasses[familyCode];
            }

            if (iButtonType == null)
            {
                // Perform the normal lookup.
                iButtonType = Type.GetType("RavuAlHemio.OneWire.Container.OneWireContainer" + familyString);
            }

            if (iButtonType == null)
            {
                // Fine; fetch the generic container.
                iButtonType = typeof (OneWireContainer);
            }

            if (!typeof (OneWireContainer).IsAssignableFrom(iButtonType))
            {
                throw new NotSupportedException(
                    $"type {iButtonType.FullName} is not a subtype of {typeof(OneWireContainer).FullName}"
                );
            }

            var ctor = iButtonType.GetConstructor(new [] { typeof(DSPortAdapter), typeof(OneWireAddress) });
            if (ctor == null)
            {
                throw new NotSupportedException(
                    $"type {iButtonType.FullName} does not have a constructor that takes a {typeof(DSPortAdapter).FullName} and a {typeof(OneWireAddress).FullName} in that order"
                );
            }
            return (OneWireContainer)ctor.Invoke(new object[] { this, address });
        }

        /// <summary>
        /// Constructs a <see cref="OneWireContainer"/> object for the most recently found device.
        /// </summary>
        /// <returns>A <see cref="OneWireContainer"/> for the device with the given address.</returns>
        public OneWireContainer DeviceContainer => GetDeviceContainer(GetAddress());

        /// <summary>
        /// Returns whether the device is not excluded by the user's filter (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>, <see cref="ExcludeFamily(byte)"/>,
        /// <see cref="ExcludeFamilies(IEnumerable{byte})"/>).
        /// </summary>
        /// <remarks>
        /// Exclusions (<see cref="ExcludeFamily(byte)"/>, <see cref="ExcludeFamilies(IEnumerable{byte})"/>) take
        /// precedence over inclusions (<see cref="TargetFamily(byte)"/>,
        /// <see cref="TargetFamilies(IEnumerable{byte})"/>).
        /// </remarks>
        /// <param name="address">The address of the device whose family to verify against the filter.</param>
        /// <returns>
        /// <c>true</c> if this device should be returned as part of the search results; <c>false</c> if not.
        /// </returns>
        protected bool IsValidFamily(OneWireAddress address)
        {
            byte familyCode = address.FamilyPortion;

            if (SearchExcludeFamilies != null)
            {
                if (SearchExcludeFamilies.Contains(familyCode))
                {
                    return false;
                }
            }

            if (SearchIncludeFamilies != null)
            {
                return SearchIncludeFamilies.Contains(familyCode);
            }

            return true;
        }

        /// <summary>
        /// Performs a "strong access" with the provided 1-Wire address. The method assumes the 1-Wire Network has
        /// already been reset and the "search" command has been sent before this method is called.
        /// </summary>
        /// <param name="address">The address of the device on which to perform the "strong access" on.</param>
        /// <returns>
        /// <c>true</c> if the device participated in the "strong access" search; <c>false</c> otherwise.
        /// </returns>
        private bool StrongAccess(OneWireAddress address)
        {
            // encode the address AAAA... bitwise as 11A11A11A11A...
            long addressLong = address.ToLong();
            var serialBits = new BitStringBuffer(192);
            for (int i = 0; i < 64; ++i)
            {
                serialBits[3 * i + 0] = true;
                serialBits[3 * i + 1] = true;
                serialBits[3 * i + 2] = (((addressLong >> i) & 0x1) == 0x1);
            }

            // send to 1-Wire Net
            DataBlock(serialBits.Buffer, 0, serialBits.Buffer.Length);

            // check the results of the last 8 triplets (there should be no conflicts)
            int goodBits = 0;
            for (int i = 168, counter = 56; i < 192; i += 3, ++counter)
            {
                bool topSet = serialBits[i];
                bool bottomSet = serialBits[i + 1];
                bool counterBit = serialBits[counter];

                if (topSet && bottomSet)
                {
                    // no device on line
                    goodBits = 0;
                    break;
                }

                if ((counterBit && topSet && !bottomSet) || (!counterBit && !topSet && bottomSet))
                {
                    // correct bit; count as a good bit
                    ++goodBits;
                }
            }

            // enough good bits to be successful?
            return (goodBits > 8);
        }

        public override string ToString()
        {
            try
            {
                return $"{AdapterName} {PortName}";
            }
            catch (OneWireException)
            {
                return $"{AdapterName} unknown port";
            }
        }
    }
}

