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
    /// Pull down slew rate.
    /// </summary>
    public enum SlewRate : byte
    {
        /// <summary>15 V/μs</summary>
        Vus15 = 0x00,

        /// <summary>2.2 V/μs</summary>
        Vus2p2 = 0x02,

        /// <summary>1.65 V/μs</summary>
        Vus1p65 = 0x04,

        /// <summary>1.37 V/μs</summary>
        Vus1p37 = 0x06,

        /// <summary>1.1 V/μs</summary>
        Vus1p1 = 0x08,

        /// <summary>0.83 V/μs</summary>
        Vus0p83 = 0x0A,

        /// <summary>0.7 V/μs</summary>
        Vus0p7 = 0x0C,

        /// <summary>0.55 V/μs</summary>
        Vus0p55 = 0x0E
    }
}
