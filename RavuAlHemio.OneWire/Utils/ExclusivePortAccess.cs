//---------------------------------------------------------------------------
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
using RavuAlHemio.OneWire.Adapter;

namespace RavuAlHemio.OneWire.Utils
{
    public sealed class ExclusivePortAccess : IDisposable
    {
        private readonly DSPortAdapter _portAdapter;

        /// <summary>
        /// Acquires exclusive access to the supplied port adapter and holds on to it until this object is disposed.
        /// </summary>
        /// <param name="portAdapter">The port adapter over which to acquire exclusive access.</param>
        /// <param name="timeout">
        /// The time after which to give up attempting to obtain exclusive access, or <c>null</c> to block
        /// indefinitely.
        /// </param>
        /// <exception cref="TimeoutException">
        /// Thrown if <paramref cref="timeout"/> is not <c>null</c> and the exclusive access acquisition operation
        /// timed out before it could be completed.
        /// </exception>
        public ExclusivePortAccess(DSPortAdapter portAdapter, TimeSpan? timeout = null)
        {
            _portAdapter = portAdapter;

            if (!_portAdapter.BeginExclusive(timeout))
            {
                throw new TimeoutException();
            }
        }

        public void Dispose()
        {
            _portAdapter.EndExclusive();
        }
    }
}
