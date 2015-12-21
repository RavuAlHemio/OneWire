//---------------------------------------------------------------------------
// Copyright © 2004 Maxim Integrated Products, All Rights Reserved.
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

using RavuAlHemio.OneWire.Adapter;

namespace RavuAlHemio.OneWire.SerialDriver.Internals
{
    public class OneWireSerialState
    {
        /// <summary>
        /// The current logical speed at which the 1-Wire Network is operating.
        /// </summary>
        public NetworkSpeed Speed { get; set; }

        /// <summary>
        /// The current logical power level at which the 1-Wire Network is operating.
        /// </summary>
        public PowerLevel Level { get; set; }

        /// <summary>
        /// Whether programming voltage is available.
        /// </summary>
        public bool CanProgram { get; set; }

        /// <summary>
        /// Whether the power level should be changed (to <see cref="PrimedLevelValue"/>) after the next bit is
        /// transferred.
        /// </summary>
        public bool LevelChangeOnNextBit { get; set; }

        /// <summary>
        /// Whether the power level should be changed (to <see cref="PrimedLevelValue"/>) after the next byte is
        /// transferred.
        /// </summary>
        public bool LevelChangeOnNextByte { get; set; }

        /// <summary>
        /// The power level to which to change on next bit or next byte (if <see cref="LevelChangeOnNextBit"/> or
        /// <see cref="LevelChangeOnNextByte"/> are set, respectively).
        /// </summary>
        public PowerLevel PrimedLevelValue { get; set; }

        /// <summary>
        /// The duration of the power delivery.
        /// </summary>
        public PowerDeliveryDuration LevelTimeFactor { get; set; }

        /// <summary>
        /// The value of the last discrepancy during the last search for a 1-Wire device.
        /// </summary>
        public int SearchLastDiscrepancy { get; set; }

        /// <summary>
        /// The value of the last discrepancy in the family code during the last search for a 1-Wire device.
        /// </summary>
        public int SearchFamilyLastDiscrepancy { get; set; }

        /// <summary>
        /// Whether the last device found is the last device on the 1-Wire Network.
        /// </summary>
        public bool SearchLastDevice { get; set; }

        /// <summary>
        /// Address of the last 1-Wire device found.
        /// </summary>
        public OneWireAddress LastFoundAddress { get; set; }

        /// <summary>
        /// Whether to search only for 1-Wire devices in an alarm state.
        /// </summary>
        public bool SearchOnlyAlarmingButtons { get; set; }

        /// <summary>
        /// Whether the next search should not be preceded by a 1-Wire reset.
        /// </summary>
        /// <value><c>false</c> to perform a reset before searching; <c>true</c> to not perform a reset.</value>
        public bool SkipResetOnSearch { get; set; }

        public OneWireSerialState()
        {
            Speed = NetworkSpeed.Regular;
            Level = PowerLevel.Normal;

            CanProgram = false;

            LevelChangeOnNextBit = false;
            LevelChangeOnNextByte = false;
            PrimedLevelValue = PowerLevel.Normal;
            LevelTimeFactor = PowerDeliveryDuration.Infinite;

            SearchLastDiscrepancy = 0;
            SearchFamilyLastDiscrepancy = 0;
            SearchLastDevice = false;

            LastFoundAddress = new OneWireAddress(0);

            SearchOnlyAlarmingButtons = false;
            SkipResetOnSearch = false;
        }
    }
}
