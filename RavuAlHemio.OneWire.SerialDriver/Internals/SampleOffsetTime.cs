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
    /// Data sample offset and write 0 recovery times.
    /// </summary>
    public enum SampleOffsetTime : byte
    {
        /// <summary>4 μs</summary>
        Us4 = 0x00,

        /// <summary>5 μs</summary>
        Us5 = 0x02,

        /// <summary>6 μs</summary>
        Us6 = 0x04,

        /// <summary>7 μs</summary>
        Us7 = 0x06,

        /// <summary>8 μs</summary>
        Us8 = 0x08,

        /// <summary>9 μs</summary>
        Us9 = 0x0A,

        /// <summary>10 μs</summary>
        Us10 = 0x0C,

        /// <summary>11 μs</summary>
        Us11 = 0x0E
    }
}
