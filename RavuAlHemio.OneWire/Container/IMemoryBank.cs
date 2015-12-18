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
using RavuAlHemio.OneWire.Adapter;

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
        /// Reads memory in this bank with no CRC checking (device or data).
        /// <para>
        /// The resulting data from this API may or may not be what is actually stored on the 1-Wire device. It is
        /// recommended that the data contain some kind of checking, such as the CRC verification in
        /// <see cref="IPagedMemoryBank.ReadPagePacket"/>. Some memory banks calculate and transmit a CRC
        /// automatically; these devices return <c>true</c> for <see cref="IPagedMemoryBank.HasPageAutoCRC"/> and allow
        /// a checked read via  <see cref="IPagedMemoryBank.ReadPageCRC"/>. If neither is an option, this method should
        /// be called at least once to verify that the same data is read each time.
        /// </para>
        /// </summary>
        /// <param name="startAddress">The address at which to start reading.</param>
        /// <param name="readContinue">
        /// Whether this read is a direct continuation of the previous one and both reads are performed within the same
        /// block of exclusive access to the port.
        /// </param>
        /// <param name="readBuf">The pre-allocated buffer into which to read the data.</param>
        /// <param name="offset">
        /// The offset from the beginning of <paramref name="readBuf"/> at which to store the result.
        /// </param>
        /// <param name="len">Number of bytes to read and store into <paramref name="readBuf"/>.</param>
        /// <exception cref="OneWireIOException">
        /// Thrown if a 1-Wire communication error occurs, e.g. a physical interruption or a new device broadcasting a
        /// presence pulse.
        /// </exception>
        /// <exception cref="OneWireException">Thrown if the adapter is not ready.</exception>
        void Read(int startAddress, bool readContinue, IList<byte> readBuf, int offset, int len);

        /// <summary>
        /// Writes memory in this bank.
        /// <para>
        /// It is recommended that the data is stored with some kind of error checking to ensure data integrity on
        /// read. For example, <see cref="IPagedMemoryBank.WritePagePacket"/> automatically wraps the data with length
        /// and CRC information and can be read back with <see cref="IPagedMemoryBank.ReadPagePacket"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Note that when writing to a Write-Once memory bank, the resulting data is the bitwise AND of the already
        /// stored value and the value to be written. If such a memory bank is written to again, the data that is read
        /// back will therefore not equal the data that was transmitted to be written, and verification will fail.
        /// This verification can be turned off by setting <see cref="WriteVerification"/> to <c>false</c>.
        /// </remarks>
        /// <param name="startAddress">The address at which to start writing.</param>
        /// <param name="data">The data to write.</param>
        void Write(int startAddress, IEnumerable<byte> data);
    }
}
