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

using System;
using System.Collections.Generic;
using RavuAlHemio.OneWire.Adapter;

namespace RavuAlHemio.OneWire.SerialDriver.Internals
{
    public class SerialAdapterState
    {
        /// <summary>
        /// DS9097U speed modes.
        /// </summary>
        public static class Speed
        {
            /// <summary>Regular speed.</summary>
            public const byte Regular = 0x00;

            /// <summary>Flexible speed for long lines.</summary>
            public const byte Flex = 0x04;

            /// <summary>Overdrive speed.</summary>
            public const byte Overdrive = 0x08;

            /// <summary>Pulse (for PROM programming and power delivery).</summary>
            public const byte Pulse = 0x0C;
        }

        /// <summary>
        /// DS9097U modes.
        /// </summary>
        public static class Mode
        {
            /// <summary>Data mode.</summary>
            public const byte Data = 0xE1;

            /// <summary>Command mode.</summary>
            public const byte Command = 0xE3;

            /// <summary>Pulse mode.</summary>
            public const byte StopPulse = 0xF1;

            /// <summary>Special mode (in revision 1 silicon only).</summary>
            public const byte Special = 0xF3;
        }

        /// <summary>Chip revision 1.</summary>
        public const byte ChipVersion1 = 0x04;

        /// <summary>Chip revision mask.</summary>
        public const byte ChipVersionMask = 0x1C;

        /// <summary>Program voltage available mask.</summary>
        public const byte ProgramVoltageMask = 0x20;

        /// <summary>Maximum number of alarms.</summary>
        public const int MaxAlarmCount = 3000;

        /// <summary>
        /// Paraeter settings for the three logical modes.
        /// </summary>
        public Dictionary<NetworkSpeed, SerialAdapterParameterSettings> SerialAdapterParameters { get; set; }

        /// <summary>
        /// The state of the 1-Wire Network.
        /// </summary>
        public OneWireSerialState SerialState { get; set; }

        /// <summary>
        /// <c>true</c> if we can stream bits.
        /// </summary>
        public bool CanStreamBits { get; set; }

        /// <summary>
        /// <c>true</c> if we can stream bytes.
        /// </summary>
        public bool CanStreamBytes { get; set; }

        /// <summary>
        /// <c>true</c> if we can stream Search commands.
        /// </summary>
        public bool CanStreamSearches { get; set; }

        /// <summary>
        /// <c>true</c> if we can stream resets.
        /// </summary>
        public bool CanStreamResets { get; set; }

        /// <summary>
        /// Current baud rate of the adapter.
        /// </summary>
        public AdapterBaud Baud { get; set; }

        /// <summary>
        /// This is the current "real" speed that the 1-Wire is operating at, in contrast to the logical speed of the
        /// adapter (<see cref="OneWireSerialState.Speed"/>).
        /// </summary>
        public byte SpeedMode { get; set; }

        /// <summary>
        /// Whether the adapter has programming voltage available.
        /// </summary>
        public bool ProgrammingVoltageAvailable { get; set; }
        
        /// <summary>
        /// <c>true</c> if the adapter is currently in command mode, <c>false</c> if it is in command mode.
        /// </summary>
        public bool InCommandMode { get; set; }

        /// <summary>
        /// The revision of the DS2480. Currently possible values are 1 and 2.
        /// </summary>
        public byte Revision { get; set; }

        /// <summary>
        /// Flag to indicate need to search for long alarm check.
        /// </summary>
        public bool LongAlarmCheck { get; set; }

        /// <summary>
        /// Count of how many resets have been seen without alarms.
        /// </summary>
        public int LastAlarmCount { get; set; }

        public SerialAdapterState(OneWireSerialState state)
        {
            // store the reference
            SerialState = state;

            // set the defaults
            Baud = AdapterBaud.Baud9600;
            SpeedMode = Speed.Flex;
            Revision = 0;
            InCommandMode = true;
            CanStreamBits = true;
            CanStreamBytes = true;
            CanStreamSearches = true;
            CanStreamResets = false;
            ProgrammingVoltageAvailable = false;
            LongAlarmCheck = false;
            LastAlarmCount = 0;

            // create the three speed logical parameter settings
            SerialAdapterParameters = new Dictionary<NetworkSpeed, SerialAdapterParameterSettings>();
            foreach (object val in Enum.GetValues(typeof(NetworkSpeed)))
            {
                SerialAdapterParameters[(NetworkSpeed)val] = new SerialAdapterParameterSettings
                {
                    PullDownSlewRate = SlewRate.Vus1p37,
                    Pulse12VoltTime = ProgramPulseTime12.Infinite,
                    Pulse5VoltTime = ProgramPulseTime5.Infinite,
                    Write1LowTime = WriteOneLowTime.Us10,
                    SampleOffsetTime = SampleOffsetTime.Us8
                };
            }

            // adjust flex time
            SerialAdapterParameters[NetworkSpeed.Flex].PullDownSlewRate = SlewRate.Vus0p83;
            SerialAdapterParameters[NetworkSpeed.Flex].Write1LowTime = WriteOneLowTime.Us12;
            SerialAdapterParameters[NetworkSpeed.Flex].SampleOffsetTime = SampleOffsetTime.Us10;
        }
    }
}
