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

using System.Collections.Generic;

namespace RavuAlHemio.OneWire.Container
{
    /// <summary>
    /// Memory bank interface for basic memory communication with 1-Wire devices. The property
    /// <see cref="OneWireContainer.MemoryBanks"/> returns a list of memory banks that are accessible through this
    /// interface. Some memory banks implement <see cref="IPagedMemoryBank"/> or <see cref="IOTPMemoryBank"/>,
    /// providing additional functionality.
    /// </summary>
    public interface IMemoryBank
    {
        /// <summary>
        /// A string description of this memory bank.
        /// </summary>
        string BankDescription { get; }

        /// <summary>
        /// Whether this memory bank is general-purpose user memory. If not, it might be a mapped section of memory
        /// which influences the behavior of the 1-Wire device.
        /// </summary>
        /// <value>
        /// <c>true</c> if this memory bank is general-purpose user memory; <c>false</c> if it is special-purpose.
        /// </value>
        bool IsGeneralPurposeMemory { get; }

        /// <summary>
        /// The size of this memory bank in bytes.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Whether this memory bank can be both written and read.
        /// </summary>
        bool IsReadWrite { get; }

        /// <summary>
        /// Whether this memory bank can only be written once.
        /// </summary>
        bool IsWriteOnce { get; }

        /// <summary>
        /// Whether this memory bank can only be read.
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// Whether this memory bank is nonvolatile. Nonvolatile memory retains its value even after the device is
        /// removed from the 1-Wire Network.
        /// </summary>
        bool IsNonVolatile { get; }

        /// <summary>
        /// Whether this memory bank requires a programming pulse in order to be written.
        /// </summary>
        bool WriteNeedsProgramPulse { get; }

        /// <summary>
        /// Whether this memory bank requires the power delivery 1-Wire voltage in order to be written.
        /// </summary>
        bool WriteNeedsPowerDelivery { get; }

        /// <summary>
        /// The starting physical address of this bank. Physical banks might be subdivided into logical banks, each
        /// fulfilling a different purpose.
        /// </summary>
        /// <remarks>
        /// The value is purely informative; the <see cref="Read"/> and <see cref="Write"/> methods calculate the
        /// physical address automatically when writing to a logical memory bank.
        /// </remarks>
        int StartPhysicalAddress { get; }

        /// <summary>
        /// Whether the <see cref="Write"/> method should verify writes after they are performed. This must be set to
        /// <c>false</c> when manipulating Write-Once bits.
        /// </summary>
        /// <value><c>true</c> (default) if <see cref="Write"/> should verify the written data; <c>false</c> if it
        /// must not.</value>
        bool WriteVerification { get; set; }

        /// <summary>
        /// Reads memory in this bank with no CRC checking (device or data). The resulting data from this API may or
        /// may not be what is actually stored on the 1-Wire device. It is recommended that the data contain some kind
        /// of checking, such as the CRC verification in <see cref="IPagedMemoryBank.ReadPagePacket"/>. Some memory
        /// banks calculate and transmit a CRC automatically; these devices return <c>true</c> for
        /// <see cref="IPagedMemoryBank.HasPageAutoCRC"/> and perform
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="readContinue"></param>
        /// <param name="readBuf"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        void Read(int startAddress, bool readContinue, IList<byte> readBuf, int offset, int len);
    }
}
