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
    /// <summary>
    /// 12-volt programming pulse times.
    /// </summary>
    public enum ProgramPulseTime12 : byte
    {
        /// <summary>32 μs</summary>
        Us32 = 0x00,

        /// <summary>64 μs</summary>
        Us64 = 0x02,

        /// <summary>128 μs</summary>
        Us128 = 0x04,

        /// <summary>256 μs</summary>
        Us256 = 0x06,

        /// <summary>512 μs</summary>
        Us512 = 0x08,

        /// <summary>1024 μs</summary>
        Us1024 = 0x0A,

        /// <summary>2048 μs</summary>
        Us2048 = 0x0C,

        /// <summary>Infinite (i.e. until cancelled manually).</summary>
        Infinite = 0x0E
    }
}
