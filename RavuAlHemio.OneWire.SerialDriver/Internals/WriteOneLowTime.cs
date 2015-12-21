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
    /// How long to hold the line low to write a 1 to the 1-Wire Network.
    /// </summary>
    public enum WriteOneLowTime : byte
    {
        /// <summary>8 μs</summary>
        Us8 = 0x00,

        /// <summary>9 μs</summary>
        Us9 = 0x02,

        /// <summary>10 μs</summary>
        Us10 = 0x04,

        /// <summary>11 μs</summary>
        Us11 = 0x06,

        /// <summary>12 μs</summary>
        Us12 = 0x08,

        /// <summary>13 μs</summary>
        Us13 = 0x0A,

        /// <summary>14 μs</summary>
        Us14 = 0x0C,

        /// <summary>15 μs</summary>
        Us15 = 0x0E
    }
}
