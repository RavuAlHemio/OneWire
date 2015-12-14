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
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire
{
    /// <summary>
    /// This is the general exception thrown by the iButton and 1-Wire operations.
    /// </summary>
    public class OneWireException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="OneWireException"/> with the specified (optional) detail message and the
        /// specified (optional) inner exception.
        /// </summary>
        /// <param name="message">
        /// The message of this exception, or <c>null</c> if no message should be specified.
        /// </param>
        /// <param name="innerException">
        /// The exception wrapped by this exception, or <c>null</c> if no exception is being wrapped.
        /// </param>
        public OneWireException([CanBeNull] string message = null, [CanBeNull] Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}

