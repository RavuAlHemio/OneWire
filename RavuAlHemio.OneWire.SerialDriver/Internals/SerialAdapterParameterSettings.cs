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

namespace RavuAlHemio.OneWire.SerialDriver.Internals
{
    public class SerialAdapterParameterSettings
    {
        /// <summary>The pulldown slew rate for this mode.</summary>
        public SlewRate PullDownSlewRate { get; set; }

        /// <summary>12V programming pulse time.</summary>
        public ProgramPulseTime12 Pulse12VoltTime { get; set; }

        /// <summary>5V programming pulse time.</summary>
        public ProgramPulseTime5 Pulse5VoltTime { get; set; }

        /// <summary>Write 1 low time.</summary>
        public WriteOneLowTime Write1LowTime { get; set; }

        /// <summary>Data sample offset and write 0 recovery time expressed in microseconds.</summary>
        public SampleOffsetTime SampleOffsetTime { get; set; }
    }
}
