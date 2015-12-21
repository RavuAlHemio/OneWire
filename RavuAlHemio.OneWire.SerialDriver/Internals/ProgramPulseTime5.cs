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

// ReSharper disable InconsistentNaming
namespace RavuAlHemio.OneWire.SerialDriver.Internals
{
    /// <summary>
    /// 5-volt programming pulse times.
    /// </summary>
    public enum ProgramPulseTime5 : byte
    {
        /// <summary>16.4 ms</summary>
        Ms16p4 = 0x00,

        /// <summary>65.5 ms</summary>
        Ms65p5 = 0x02,

        /// <summary>131 ms</summary>
        Ms131 = 0x04,

        /// <summary>262 ms</summary>
        Ms262 = 0x06,

        /// <summary>524 ms</summary>
        Ms524 = 0x08,

        /// <summary>1.05 s</summary>
        S1p05 = 0x0A,

        /// <summary>2.10 s</summary>
        S2p10 = 0x0C,

        /// <summary>Dynamic current detection.</summary>
        Dynamic = 0x0C,  // sic! same as 2.10s

        /// <summary>Infinite (i.e. until cancelled manually).</summary>
        Infinite = 0x0E
    }
}
