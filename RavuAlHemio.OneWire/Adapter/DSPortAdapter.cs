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
using JetBrains.Annotations;
using RavuAlHemio.OneWire.Container;

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
        /// Speed modes for 1-Wire Network.
        /// </summary>
        public enum Speed : int
        {
            /// <summary>
            /// Regular speed.
            /// </summary>
            Regular = 0,

            /// <summary>
            /// Flexible for long lines speed.
            /// </summary>
            Flex = 1,

            /// <summary>
            /// Overdrive speed.
            /// </summary>
            Overdrive = 2,

            /// <summary>
            /// Hyperdrive speed.
            /// </summary>
            Hyperdrive = 3
        }

        /// <summary>
        /// 1-Wire Network level.
        /// </summary>
        public enum Level : byte
        {
            /// <summary>
            /// Normal level (weak 5Volt pullup).
            /// </summary>
            Normal = 0,

            /// <summary>
            /// The power delivery level (strong 5Volt pullup).
            /// </summary>
            PowerDelivery = 1,

            /// <summary>
            /// Reset power level (strong pulldown to 0Volts, resets 1-Wire).
            /// </summary>
            Break = 2,

            /// <summary>
            /// EPROM programming level (strong 12Volt pullup).
            /// </summary>
            Program = 3
        }

        /// <summary>
        /// 1-Wire Network reset result.
        /// </summary>
        public enum ResetResult : int
        {
            /// <summary>
            /// No presence.
            /// </summary>
            NoPresence = 0x00,

            /// <summary>
            /// Presence.
            /// </summary>
            Presence = 0x01,

            /// <summary>
            /// Alarm.
            /// </summary>
            Alarm = 0x02,

            /// <summary>
            /// Short circuit.
            /// </summary>
            Short = 0x03
        }

        /// <summary>
        /// Condition for power state change.
        /// </summary>
        public enum PowerStateChangeCondition : int
        {
            /// <summary>
            /// Immediate power state change.
            /// </summary>
            Now = 0,

            /// <summary>
            /// Change after next bit communication.
            /// </summary>
            AfterBit = 1,
                
            /// <summary>
            /// Change after next byte communication.
            /// </summary>
            AfterByte = 2
        }

        /// <summary>
        /// Duration used in delivering power to the 1-Wire.
        /// </summary>
        public enum PowerDeliveryDuration : int
        {
            /// <summary>
            /// 1/2 second power delivery.
            /// </summary>
            HalfSecond = 0,

            /// <summary>
            /// 1 second power delivery.
            /// </summary>
            OneSecond = 1,

            /// <summary>
            /// 2 second power delivery.
            /// </summary>
            TwoSeconds = 2,

            /// <summary>
            /// 4 second power delivery.
            /// </summary>
            FourSeconds = 3,

            /// <summary>
            /// Smart complete power delivery.
            /// </summary>
            SmartDone = 4,

            /// <summary>
            /// Infinite power delivery.
            /// </summary>
            Infinite = 5,

            /// <summary>
            /// Current detect power delivery.
            /// </summary>
            CurrentDetect = 6,

            /// <summary>
            /// 480 µs power delivery.
            /// </summary>
            EPROM = 7
        }

        /// <summary>
        /// Dictionary to contain the user-replaced OneWireContainers.
        /// </summary>
        [NotNull]
        private readonly Dictionary<int, Type> _registeredOneWireContainerClasses = new Dictionary<int, Type>(5);

        /// <summary>
        /// Byte array of families to include in search.
        /// </summary>
        private byte[] _include;

        /// <summary>
        /// Byte array of families to exclude from search.
        /// </summary>
        private byte[] _exclude;

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
        /// <see cref="SelectPort"/> before any other communication methods can be used. Using a communcation method
        /// before <see cref="SelectPort"/> will result in a <see cref="OneWireException"/>.
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
        public void RegisterOneWireContainerClass(int family, [CanBeNull] Type oneWireContainerClass)
        {
            Type defaultIBC = typeof(OneWireContainer);

            if (oneWireContainerClass == null)
            {
                // If null is passed, remove the old container class.
                _registeredOneWireContainerClasses.Remove(family);
            }
            else
            {
                if (defaultIBC.IsAssignableFrom(oneWireContainerClass))
                {
                    _registeredOneWireContainerClasses[family] = oneWireContainerClass;
                }
                else
                {
                    throw new InvalidCastException("oneWireContainerClass does not extend " + defaultIBC.FullName);
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
        public virtual string AdapterVersion
        {
            get
            {
                return "<na>";
            }
        }

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
        public virtual string AdapterAddress
        {
            get
            {
                return "<na>";
            }
        }

        /// <summary>
        /// Returns whether this adapter physically supports the given speed.
        /// </summary>
        /// <param name="speed">The speed whose support to check.</param>
        /// <returns><c>true</c> if the supplied speed is supported; <c>false</c> otherwise.</returns>
        public virtual bool SupportsSpeed(Speed speed)
        {
            if (speed == Speed.Regular)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this adapter physically supports the given speed.
        /// </summary>
        /// <param name="speed">The speed whose support to check.</param>
        /// <returns><c>true</c> if the supplied speed is supported; <c>false</c> otherwise.</returns>
        public virtual bool SupportsLevel(Level level)
        {
            if (level == Level.Normal)
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
        public virtual bool CanDeliverSmartPower
        {
            get
            {
                return false;
            }
        }

        #if HALF_IMPLEMENTED
        /// <summary>
        /// Returns an <see cref="IEnumerable`1"/> of <see cref="OneWireContainer"/> objects corresponding to all of the
        /// iButtons or 1-Wire devices found on the 1-Wire Network. If no devices are found, then an empty enumerable
        /// will be returned. In most cases, all further communication iwth the device is done through the
        /// <see cref="OneWireContainer"/>.
        /// </summary>
        /// <returns>
        /// <see cref="IEnumerable`1"/> of <see cref="OneWireContainer"/> objects found on the 1-Wire Network.
        /// </returns>
        /// <exception cref="OneWireIOException">Thrown on a 1-Wire communication error with the adapter.</exception>
        /// <exception cref="OneWireException">Thrown on a 1-Wire setup error with the adapter.</exception>
        [NotNull]
        public IEnumerable<OneWireContainer> GetAllDeviceContainers()
        {
            var ibuttonList = new List<OneWireContainer>();
            OneWireContainer tempIButton;

            tempIButton = GetFirstDeviceContainer();
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
        public OneWireContainer GetFirstDeviceContainer()
        {
            if (FindFirstDevice())
            {
                return GetDeviceContainer();
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
                return GetDeviceContainer();
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
        public bool IsPresent(OneWireAddress address)
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
        public bool IsAlarming(OneWireAddress address)
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
        public bool Select(OneWireAddress address)
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
            DataBlock(sendPacket);

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
        #endif

        #if JAVACODE

            //--------
            //-------- Finding iButton/1-Wire device options
            //--------

            /**
    * Sets the 1-Wire Network search to find only iButtons and 1-Wire
    * devices that are in an 'Alarm' state that signals a need for
    * attention.  Not all iButton types
    * have this feature.  Some that do: DS1994, DS1920, DS2407.
    * This selective searching can be canceled with the
    * 'setSearchAllDevices()' method.
    *
    * @see #setNoResetSearch
    */
            public abstract void setSearchOnlyAlarmingDevices ();

            /**
    * Sets the 1-Wire Network search to not perform a 1-Wire
    * reset before a search.  This feature is chiefly used with
    * the DS2409 1-Wire coupler.
    * The normal reset before each search can be restored with the
    * 'setSearchAllDevices()' method.
    */
            public abstract void setNoResetSearch ();

            /**
    * Sets the 1-Wire Network search to find all iButtons and 1-Wire
    * devices whether they are in an 'Alarm' state or not and
    * restores the default setting of providing a 1-Wire reset
    * command before each search. (see setNoResetSearch() method).
    *
    * @see #setNoResetSearch
    */
            public abstract void setSearchAllDevices ();

            /**
    * Removes any selectivity during a search for iButtons or 1-Wire devices
    * by family type.  The unique address for each iButton and 1-Wire device
    * contains a family descriptor that indicates the capabilities of the
    * device.
    * @see    #targetFamily
    * @see    #targetFamily(byte[])
    * @see    #excludeFamily
    * @see    #excludeFamily(byte[])
    */
            public void targetAllFamilies ()
            {
                include = null;
                exclude = null;
            }

            /**
    * Takes an integer to selectively search for this desired family type.
    * If this method is used, then no devices of other families will be
    * found by any of the search methods.
    *
    * @param  family   the code of the family type to target for searches
    * @see   com.dalsemi.onewire.utils.Address
    * @see    #targetAllFamilies
    */
            public void targetFamily (int family)
            {
                if ((include == null) || (include.length != 1))
                    include = new byte [1];

                include [0] = ( byte ) family;
            }

            /**
    * Takes an array of bytes to use for selectively searching for acceptable
    * family codes.  If used, only devices with family codes in this array
    * will be found by any of the search methods.
    *
    * @param  family  array of the family types to target for searches
    * @see   com.dalsemi.onewire.utils.Address
    * @see    #targetAllFamilies
    */
            public void targetFamily (byte family [])
            {
                if ((include == null) || (include.length != family.length))
                    include = new byte [family.length];

                System.arraycopy(family, 0, include, 0, family.length);
            }

            /**
    * Takes an integer family code to avoid when searching for iButtons.
    * or 1-Wire devices.
    * If this method is used, then no devices of this family will be
    * found by any of the search methods.
    *
    * @param  family   the code of the family type NOT to target in searches
    * @see   com.dalsemi.onewire.utils.Address
    * @see    #targetAllFamilies
    */
            public void excludeFamily (int family)
            {
                if ((exclude == null) || (exclude.length != 1))
                    exclude = new byte [1];

                exclude [0] = ( byte ) family;
            }

            /**
    * Takes an array of bytes containing family codes to avoid when finding
    * iButtons or 1-Wire devices.  If used, then no devices with family
    * codes in this array will be found by any of the search methods.
    *
    * @param  family  array of family cods NOT to target for searches
    * @see   com.dalsemi.onewire.utils.Address
    * @see    #targetAllFamilies
    */
            public void excludeFamily (byte family [])
            {
                if ((exclude == null) || (exclude.length != family.length))
                    exclude = new byte [family.length];

                System.arraycopy(family, 0, exclude, 0, family.length);
            }

            //--------
            //-------- 1-Wire Network Semaphore methods
            //--------

            /**
    * Gets exclusive use of the 1-Wire to communicate with an iButton or
    * 1-Wire Device.
    * This method should be used for critical sections of code where a
    * sequence of commands must not be interrupted by communication of
    * threads with other iButtons, and it is permissible to sustain
    * a delay in the special case that another thread has already been
    * granted exclusive access and this access has not yet been
    * relinquished. <p>
    *
    * It can be called through the OneWireContainer
    * class by the end application if they want to ensure exclusive
    * use.  If it is not called around several methods then it
    * will be called inside each method.
    *
    * @param blocking <code>true</code> if want to block waiting
    *                 for an excluse access to the adapter
    * @return <code>true</code> if blocking was false and a
    *         exclusive session with the adapter was aquired
    *
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract boolean beginExclusive (boolean blocking)
            throws OneWireException;

            /**
    * Relinquishes exclusive control of the 1-Wire Network.
    * This command dynamically marks the end of a critical section and
    * should be used when exclusive control is no longer needed.
    */
            public abstract void endExclusive ();

            //--------
            //-------- Primitive 1-Wire Network data methods
            //--------

            /**
    * Sends a bit to the 1-Wire Network.
    *
    * @param  bitValue  the bit value to send to the 1-Wire Network.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract void putBit (boolean bitValue)
            throws OneWireIOException, OneWireException;

            /**
    * Gets a bit from the 1-Wire Network.
    *
    * @return  the bit value recieved from the the 1-Wire Network.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract boolean getBit ()
            throws OneWireIOException, OneWireException;

            /**
    * Sends a byte to the 1-Wire Network.
    *
    * @param  byteValue  the byte value to send to the 1-Wire Network.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract void putByte (int byteValue)
            throws OneWireIOException, OneWireException;

            /**
    * Gets a byte from the 1-Wire Network.
    *
    * @return  the byte value received from the the 1-Wire Network.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract int getByte ()
            throws OneWireIOException, OneWireException;

            /**
    * Gets a block of data from the 1-Wire Network.
    *
    * @param  len  length of data bytes to receive
    *
    * @return  the data received from the 1-Wire Network.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract byte[] getBlock (int len)
            throws OneWireIOException, OneWireException;

            /**
    * Gets a block of data from the 1-Wire Network and write it into
    * the provided array.
    *
    * @param  arr     array in which to write the received bytes
    * @param  len     length of data bytes to receive
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract void getBlock (byte[] arr, int len)
            throws OneWireIOException, OneWireException;

            /**
    * Gets a block of data from the 1-Wire Network and write it into
    * the provided array.
    *
    * @param  arr     array in which to write the received bytes
    * @param  off     offset into the array to start
    * @param  len     length of data bytes to receive
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract void getBlock (byte[] arr, int off, int len)
            throws OneWireIOException, OneWireException;

            /**
    * Sends a block of data and returns the data received in the same array.
    * This method is used when sending a block that contains reads and writes.
    * The 'read' portions of the data block need to be pre-loaded with 0xFF's.
    * It starts sending data from the index at offset 'off' for length 'len'.
    *
    * @param  dataBlock  array of data to transfer to and from the 1-Wire Network.
    * @param  off        offset into the array of data to start
    * @param  len        length of data to send / receive starting at 'off'
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract void dataBlock (byte dataBlock [], int off, int len)
            throws OneWireIOException, OneWireException;

            /**
    * Sends a Reset to the 1-Wire Network.
    *
    * @return  the result of the reset. Potential results are:
    * <ul>
    * <li> 0 (RESET_NOPRESENCE) no devices present on the 1-Wire Network.
    * <li> 1 (RESET_PRESENCE) normal presence pulse detected on the 1-Wire
    *        Network indicating there is a device present.
    * <li> 2 (RESET_ALARM) alarming presence pulse detected on the 1-Wire
    *        Network indicating there is a device present and it is in the
    *        alarm condition.  This is only provided by the DS1994/DS2404
    *        devices.
    * <li> 3 (RESET_SHORT) indicates 1-Wire appears shorted.  This can be
    *        transient conditions in a 1-Wire Network.  Not all adapter types
    *        can detect this condition.
    * </ul>
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public abstract int reset ()
            throws OneWireIOException, OneWireException;

            //--------
            //-------- 1-Wire Network power methods
            //--------

            /**
    * Sets the duration to supply power to the 1-Wire Network.
    * This method takes a time parameter that indicates the program
    * pulse length when the method startPowerDelivery().<p>
    *
    * Note: to avoid getting an exception,
    * use the canDeliverPower() and canDeliverSmartPower()
    * method to check it's availability. <p>
    *
    * @param timeFactor
    * <ul>
    * <li>   0 (DELIVERY_HALF_SECOND) provide power for 1/2 second.
    * <li>   1 (DELIVERY_ONE_SECOND) provide power for 1 second.
    * <li>   2 (DELIVERY_TWO_SECONDS) provide power for 2 seconds.
    * <li>   3 (DELIVERY_FOUR_SECONDS) provide power for 4 seconds.
    * <li>   4 (DELIVERY_SMART_DONE) provide power until the
    *          the device is no longer drawing significant power.
    * <li>   5 (DELIVERY_INFINITE) provide power until the
    *          setPowerNormal() method is called.
    * </ul>
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public void setPowerDuration (int timeFactor)
            throws OneWireIOException, OneWireException
            {
                throw new OneWireException(
                    "Power delivery not supported by this adapter type");
            }

            /**
    * Sets the 1-Wire Network voltage to supply power to a 1-Wire device.
    * This method takes a time parameter that indicates whether the
    * power delivery should be done immediately, or after certain
    * conditions have been met. <p>
    *
    * Note: to avoid getting an exception,
    * use the canDeliverPower() and canDeliverSmartPower()
    * method to check it's availability. <p>
    *
    * @param changeCondition
    * <ul>
    * <li>   0 (CONDITION_NOW) operation should occur immediately.
    * <li>   1 (CONDITION_AFTER_BIT) operation should be pending
    *           execution immediately after the next bit is sent.
    * <li>   2 (CONDITION_AFTER_BYTE) operation should be pending
    *           execution immediately after next byte is sent.
    * </ul>
    *
    * @return <code>true</code> if the voltage change was successful,
    * <code>false</code> otherwise.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public boolean startPowerDelivery (int changeCondition)
            throws OneWireIOException, OneWireException
            {
                throw new OneWireException(
                    "Power delivery not supported by this adapter type");
            }

            /**
    * Sets the duration for providing a program pulse on the
    * 1-Wire Network.
    * This method takes a time parameter that indicates the program
    * pulse length when the method startProgramPulse().<p>
    *
    * Note: to avoid getting an exception,
    * use the canDeliverPower() method to check it's
    * availability. <p>
    *
    * @param timeFactor
    * <ul>
    * <li>   7 (DELIVERY_EPROM) provide program pulse for 480 microseconds
    * <li>   5 (DELIVERY_INFINITE) provide power until the
    *          setPowerNormal() method is called.
    * </ul>
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    */
            public void setProgramPulseDuration (int timeFactor)
            throws OneWireIOException, OneWireException
            {
                throw new OneWireException(
                    "Program pulse delivery not supported by this adapter type");
            }

            /**
    * Sets the 1-Wire Network voltage to eprom programming level.
    * This method takes a time parameter that indicates whether the
    * power delivery should be done immediately, or after certain
    * conditions have been met. <p>
    *
    * Note: to avoid getting an exception,
    * use the canProgram() method to check it's
    * availability. <p>
    *
    * @param changeCondition
    * <ul>
    * <li>   0 (CONDITION_NOW) operation should occur immediately.
    * <li>   1 (CONDITION_AFTER_BIT) operation should be pending
    *           execution immediately after the next bit is sent.
    * <li>   2 (CONDITION_AFTER_BYTE) operation should be pending
    *           execution immediately after next byte is sent.
    * </ul>
    *
    * @return <code>true</code> if the voltage change was successful,
    * <code>false</code> otherwise.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    *         or the adapter does not support this operation
    */
            public boolean startProgramPulse (int changeCondition)
            throws OneWireIOException, OneWireException
            {
                throw new OneWireException(
                    "Program pulse delivery not supported by this adapter type");
            }

            /**
    * Sets the 1-Wire Network voltage to 0 volts.  This method is used
    * rob all 1-Wire Network devices of parasite power delivery to force
    * them into a hard reset.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    *         or the adapter does not support this operation
    */
            public void startBreak ()
            throws OneWireIOException, OneWireException
            {
                throw new OneWireException(
                    "Break delivery not supported by this adapter type");
            }

            /**
    * Sets the 1-Wire Network voltage to normal level.  This method is used
    * to disable 1-Wire conditions created by startPowerDelivery and
    * startProgramPulse.  This method will automatically be called if
    * a communication method is called while an outstanding power
    * command is taking place.
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    *         or the adapter does not support this operation
    */
            public void setPowerNormal ()
            throws OneWireIOException, OneWireException
            {
                return;
            }

            //--------
            //-------- 1-Wire Network speed methods
            //--------

            /**
    * Sets the new speed of data
    * transfer on the 1-Wire Network. <p>
    *
    * @param speed
    * <ul>
    * <li>     0 (SPEED_REGULAR) set to normal communciation speed
    * <li>     1 (SPEED_FLEX) set to flexible communciation speed used
    *            for long lines
    * <li>     2 (SPEED_OVERDRIVE) set to normal communciation speed to
    *            overdrive
    * <li>     3 (SPEED_HYPERDRIVE) set to normal communciation speed to
    *            hyperdrive
    * <li>    >3 future speeds
    * </ul>
    *
    * @throws OneWireIOException on a 1-Wire communication error
    * @throws OneWireException on a setup error with the 1-Wire adapter
    *         or the adapter does not support this operation
    */
            public void setSpeed (int speed)
            throws OneWireIOException, OneWireException
            {
                if (speed != SPEED_REGULAR)
                    throw new OneWireException(
                        "Non-regular 1-Wire speed not supported by this adapter type");
            }

            /**
    * Returns the current data transfer speed on the 1-Wire Network. <p>
    *
    * @return <code>int</code> representing the current 1-Wire speed
    * <ul>
    * <li>     0 (SPEED_REGULAR) set to normal communication speed
    * <li>     1 (SPEED_FLEX) set to flexible communication speed used
    *            for long lines
    * <li>     2 (SPEED_OVERDRIVE) set to normal communication speed to
    *            overdrive
    * <li>     3 (SPEED_HYPERDRIVE) set to normal communication speed to
    *            hyperdrive
    * <li>    >3 future speeds
    * </ul>
    */
            public int getSpeed ()
            {
                return SPEED_REGULAR;
            }

            //--------
            //-------- Misc
            //--------

            /**
    * Constructs a <code>OneWireContainer</code> object with a user supplied 1-Wire network address.
    *
    * @param  address  device address with which to create a new container
    *
    * @return  The <code>OneWireContainer</code> object
    * @see   com.dalsemi.onewire.utils.Address
    */
            public OneWireContainer getDeviceContainer (byte[] address)
            {
                int              family_code   = address [0] & 0x7F;
                String           family_string =
                    ((family_code) < 16)
                    ? ("0" + Integer.toHexString(family_code)).toUpperCase()
                    : (Integer.toHexString(family_code)).toUpperCase();
                Class            ibutton_class = null;
                OneWireContainer new_ibutton;

                // If any user registered button exist, check the hashtable.
                if (!registeredOneWireContainerClasses.isEmpty())
                {
                    Integer familyInt = new Integer(family_code);

                    // Try and get a user provided container class first.
                    ibutton_class =
                        ( Class ) registeredOneWireContainerClasses.get(familyInt);
                }

                // If we don't get one, do the normal lookup method.
                if (ibutton_class == null)
                {

                    // try to load the ibutton container class
                    try
                    {
                        ibutton_class =
                            Class.forName("com.dalsemi.onewire.container.OneWireContainer"
                                + family_string);
                    }
                    catch (Exception e)
                    {
                        ibutton_class = null;
                    }

                    // if did not get specific container try the general one
                    if (ibutton_class == null)
                    {

                        // try to load the ibutton container class
                        try
                        {
                            ibutton_class = Class.forName(
                                "com.dalsemi.onewire.container.OneWireContainer");
                        }
                        catch (Exception e)
                        {
                            System.out.println("EXCEPTION: Unable to load OneWireContainer"
                                + e);
                            return null;
                        }
                    }
                }

                // try to load the ibutton container class
                try
                {

                    // create the iButton container with a reference to this adapter
                    new_ibutton = ( OneWireContainer ) ibutton_class.newInstance();

                    new_ibutton.setupContainer(this, address);
                }
                catch (Exception e)
                {
                    System.out.println(
                        "EXCEPTION: Unable to instantiate OneWireContainer "
                        + ibutton_class + ": " + e);
                    e.printStackTrace();

                    return null;
                }

                // return this new container
                return new_ibutton;
            }

            /**
    * Constructs a <code>OneWireContainer</code> object with a user supplied 1-Wire network address.
    *
    * @param  address  device address with which to create a new container
    *
    * @return  The <code>OneWireContainer</code> object
    * @see   com.dalsemi.onewire.utils.Address
    */
            public OneWireContainer getDeviceContainer (long address)
            {
                return getDeviceContainer(Address.toByteArray(address));
            }

            /**
    * Constructs a <code>OneWireContainer</code> object with a user supplied 1-Wire network address.
    *
    * @param  address  device address with which to create a new container
    *
    * @return  The <code>OneWireContainer</code> object
    * @see   com.dalsemi.onewire.utils.Address
    */
            public OneWireContainer getDeviceContainer (String address)
            {
                return getDeviceContainer(Address.toByteArray(address));
            }

            /**
    * Constructs a <code>OneWireContainer</code> object using the current 1-Wire network address.
    * The internal state of the port adapter keeps track of the last
    * address found and is able to create container objects from this
    * state.
    *
    * @return  the <code>OneWireContainer</code> object
    */
            public OneWireContainer getDeviceContainer ()
            {

                // Mask off the upper bit.
                byte[] address = new byte [8];

                getAddress(address);

                return getDeviceContainer(address);
            }

            /**
    * Checks to see if the family found is in the desired
    * include group.
    *
    * @return  <code>true</code> if in include group
    */
            protected boolean isValidFamily (byte[] address)
            {
                byte familyCode = address [0];

                if (exclude != null)
                {
                    for (int i = 0; i < exclude.length; i++)
                    {
                        if (familyCode == exclude [i])
                        {
                            return false;
                        }
                    }
                }

                if (include != null)
                {
                    for (int i = 0; i < include.length; i++)
                    {
                        if (familyCode == include [i])
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return true;
            }

            /**
    * Performs a 'strongAccess' with the provided 1-Wire address.
    * 1-Wire Network has already been reset and the 'search'
    * command sent before this is called.
    *
    * @param  address  device address to do strongAccess on
    *
    * @return  true if device participated and was present
    *         in the strongAccess search
    */
            private boolean strongAccess (byte[] address)
            throws OneWireIOException, OneWireException
            {
                byte[] send_packet = new byte [24];
                int    i;

                // set all bits at first
                for (i = 0; i < 24; i++)
                    send_packet [i] = ( byte ) 0xFF;

                // now set or clear apropriate bits for search
                for (i = 0; i < 64; i++)
                    arrayWriteBit(arrayReadBit(i, address), (i + 1) * 3 - 1,
                        send_packet);

                // send to 1-Wire Net
                dataBlock(send_packet, 0, 24);

                // check the results of last 8 triplets (should be no conflicts)
                int cnt = 56, goodbits = 0, tst, s;

                for (i = 168; i < 192; i += 3)
                {
                    tst = (arrayReadBit(i, send_packet) << 1)
                        | arrayReadBit(i + 1, send_packet);
                    s   = arrayReadBit(cnt++, address);

                    if (tst == 0x03)   // no device on line
                    {
                        goodbits = 0;   // number of good bits set to zero

                        break;          // quit
                    }

                    if (((s == 0x01) && (tst == 0x02)) || ((s == 0x00) && (tst == 0x01)))   // correct bit
                        goodbits++;   // count as a good bit
                }

                // check too see if there were enough good bits to be successful
                return (goodbits >= 8);
            }

            /**
    * Writes the bit state in a byte array.
    *
    * @param state new state of the bit 1, 0
    * @param index bit index into byte array
    * @param buf byte array to manipulate
    */
            private void arrayWriteBit (int state, int index, byte[] buf)
            {
                int nbyt = (index >>> 3);
                int nbit = index - (nbyt << 3);

                if (state == 1)
                    buf [nbyt] |= (0x01 << nbit);
                else
                    buf [nbyt] &= ~(0x01 << nbit);
            }

            /**
    * Reads a bit state in a byte array.
    *
    * @param index bit index into byte array
    * @param buf byte array to read from
    *
    * @return bit state 1 or 0
    */
            private int arrayReadBit (int index, byte[] buf)
            {
                int nbyt = (index >>> 3);
                int nbit = index - (nbyt << 3);

                return ((buf [nbyt] >>> nbit) & 0x01);
            }

            //--------
            //-------- java.lang.Object methods
            //--------

            /**
    * Returns a hashcode for this object
    * @return a hascode for this object
    */
            /*public int hashCode()
            {
                return this.toString().hashCode();
            }*/

            /**
            * Returns true if the given object is the same or equivalent
                * to this DSPortAdapter.
                *
                * @param o the Object to compare this DSPortAdapter to
                * @return true if the given object is the same or equivalent
                    * to this DSPortAdapter.
                    */
                    public boolean equals(Object o)
                {
                    if(o!=null && o instanceof DSPortAdapter)
                    {
                        if(o==this || o.toString().equals(this.toString()))
                        {
                            return true;
                        }
                    }
                    return false;
                }

            /**
    * Returns a string representation of this DSPortAdapter, in the format
    * of "<adapter name> <port name>".
    *
    * @return a string representation of this DSPortAdapter
    */
            public String toString()
            {
                try
                {
                    return this.getAdapterName() + " " + this.getPortName();
                }
                catch(OneWireException owe)
                {
                    return this.getAdapterName() + " Unknown Port";
                }
        }
        #endif
    }
}

