﻿//---------------------------------------------------------------------------
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RavuAlHemio.OneWire.Adapter;

namespace RavuAlHemio.OneWire.Container
{
    /// <summary>
    /// A <see cref="OneWireContainer"/> encapsulates the <see cref="DSPortAdapter"/>, network address, and methods to
    /// communicate with a specific 1-Wire device, whether it is packaged in a stainless stell armored can (an
    /// "iButton") or in standard IC plastic packaging. This class can be used to perform operations common to all
    /// 1-Wire devices; more specific subclasses are available to control specific types of 1-Wire devices.
    /// </summary>
    public class OneWireContainer
    {
        /// <summary>
        /// The requested communication speed.
        /// </summary>
        /// <remarks>
        /// Can be modified directly by subclasses. The public interface is the <see cref="Speed"/> property (for
        /// obtaining) and <see cref="SetSpeed(NetworkSpeed, bool)"/> method (for modifying) the value.
        /// </remarks>
        protected NetworkSpeed PortSpeed;

        /// <summary>
        /// Whether falling back to a slower speed is okay.
        /// </summary>
        protected bool SpeedFallbackOK;

        /// <summary>
        /// A reference to the port adapter used to communicate with this 1-Wire device.
        /// </summary>
        /// <value>The port adapter used to communicate with this device.</value>
        public DSPortAdapter Adapter { get; protected set; }

        /// <summary>
        /// The address of this 1-Wire device.
        /// </summary>
        /// <value>The address of this device.</value>
        public OneWireAddress Address { get; protected set; }

        /// <summary>
        /// Constructs a container for the 1-Wire device with the specified address contactable via the port controlled
        /// by the specified adapter.
        /// </summary>
        /// <param name="adapter">The port adapter used for communication with the device.</param>
        /// <param name="address">The address of the device.</param>
        public OneWireContainer(DSPortAdapter adapter, OneWireAddress address)
        {
            Adapter = adapter;
            Address = address;

            PortSpeed = NetworkSpeed.Regular;
            SpeedFallbackOK = false;
        }

        /// <summary>
        /// The name of the 1-Wire device type, e.g. <c>"Crypto iButton"</c> or <c>"DS1992"</c>.
        /// </summary>
        /// <value>The name of the device type.</value>
        public virtual string Name => string.Format(CultureInfo.InvariantCulture, "Device type: {0:X2}", Address.FamilyPortion);

        /// <summary>
        /// A collection of alternate names for this 1-Wire device type, such as other product numbers or nicknames.
        /// </summary>
        /// <remarks>
        /// The returned collection should either be immutable or its modification should have no effect on subsequent
        /// reads from this property.
        /// </remarks>
        /// <value>A collection of alternate names for this device type.</value>
        public virtual IReadOnly<string> AlternateNames => new List<string>();

        /// <summary>
        /// A short description of this 1-Wire device type.
        /// </summary>
        /// <value>A short description of this device type.</value>
        public virtual string Description => "No description available.";

        /// <summary>
        /// Sets the maximum communication speed for this device container (which may be slower than the maximum speed
        /// supported by the device itself).
        /// </summary>
        /// <param name="newSpeed">The new maximum communication speed.</param>
        /// <param name="fallbackOK">
        /// <c>true</c> if falling back to a slower communication speed is acceptable; <c>false</c> otherwise.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="newSpeed"/> is greater than <see cref="MaxSpeed"/>.
        /// </exception>
        public virtual void SetSpeed(NetworkSpeed newSpeed, bool fallbackOK)
        {
            PortSpeed = newSpeed;
            SpeedFallbackOK = fallbackOK;
        }

        /// <summary>
        /// The requested communication speed.
        /// </summary>
        /// <value>The speed.</value>
        public NetworkSpeed Speed => PortSpeed;

        /// <summary>
        /// The maximum speed at which this 1-Wire device can communicate.
        /// </summary>
        /// <value>The maximum communication speed of this device.</value>
        public virtual NetworkSpeed MaxSpeed => NetworkSpeed.Regular;

        /// <summary>
        /// An enumeration of memory banks accessible by this device.
        /// </summary>
        /// <value>The memory banks.</value>
        public virtual IEnumerable<IMemoryBank> MemoryBanks => Enumerable.Empty<IMemoryBank>();
    }
}
