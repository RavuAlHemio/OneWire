using System.Collections.Generic;
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// Transport-level interface to a 1-Wire Net.
    /// </summary>
    public interface IOneWireTransport
    {
        /// <summary>
        /// Transfer a block of data to and from the 1-Wire Net, optionally performing a reset beforehand. Up to 64
        /// bytes can be transferred at once.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="resetFirst">Whether to reset the 1-Wire Net before transferring the block of data.</param>
        /// <param name="bytes">The bytes to transfer.</param>
        /// <returns>
        /// The bytes read back from the 1-Wire Net, or <c>null</c> if <paramref name="resetFirst"/> was <c>true</c>
        /// and the device did not respond to the reset.
        /// </returns>
        [CanBeNull]
        byte[] TransferBlock(int portNumber, bool resetFirst, [NotNull] IList<byte> bytes);

        /*
        /// <summary>
        /// Write a byte to an EPROM 1-Wire device.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="writeByte">The byte to program into the EPROM.</param>
        /// <param name="address">The address of the byte to program.</param>
        /// <param name="writeCommand">The command used to write the byte.</param>
        /// <param name="crcType">The type of cyclic redundancy check used.</param>
        /// <param name="selectAddressFirst">
        /// Whether the address written to should be explicitly selected first. Pass <c>false</c> only if you have
        /// previously passed <c>true</c> and are storing subsequent bytes.
        /// </param>
        /// <returns>
        /// The resulting byte from the programming attempt, or <c>0xFF</c> if the device is not connected or the
        /// programming pulse voltage is not available.
        /// </returns>
        byte ProgramByte(int portNumber, byte writeByte, ushort address, byte writeCommand, CRCType crcType,
            bool selectAddressFirst);
        */
    }
}
