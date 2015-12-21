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

using System.Collections.Generic;
using System.Linq;
using RavuAlHemio.OneWire.Utils;

namespace RavuAlHemio.OneWire.SerialDriver.Internals
{
    public class SerialPacketBuilder
    {
        /// <summary>
        /// DS9097U function commands.
        /// </summary>
        public static class Function
        {
            /// <summary>Transmit single bit.</summary>
            public const byte Bit = 0x81;

            /// <summary>Activate search mode.</summary>
            public const byte SearchOn = 0xB1;

            /// <summary>Deactivate search mode.</summary>
            public const byte SearchOff = 0xA1;

            /// <summary>Perform a 1-Wire Reset.</summary>
            public const byte Reset = 0xC1;

            /// <summary>Immediately perform a 5V pulse.</summary>
            public const byte Immediate5VPulse = 0xED;

            /// <summary>Immediately perform a 12V pulse.</summary>
            public const byte Immediate12VPulse = 0xFD;

            /// <summary>Perform a 5V pulse after the next byte.</summary>
            public const byte Arm5VPulse = 0xEF;

            /// <summary>Stop an ongoing pulse.</summary>
            public const byte StopPulse = 0xF1;
        }

        /// <summary>
        /// Bit polarity for the <see cref="Function.Bit"/> command.
        /// </summary>
        public static class Bit
        {
            /// <summary>Polarity 1.</summary>
            public const byte One = 0x10;

            /// <summary>Polarity 0.</summary>
            public const byte Zero = 0x00;
        }

        /// <summary>
        /// Flag to bitwise OR with <see cref="Function.Bit"/> command if the adapter should switch to 5V strong
        /// pull-up after the bit is transmitted.
        /// </summary>
        public const byte BitPrime5VFlag = 0x02;

        /// <summary>
        /// The list of packets to send.
        /// </summary>
        protected List<RawPacket> PacketList { get; set; }

        /// <summary>
        /// The packet currently being assembled.
        /// </summary>
        protected RawPacket Packet { get; set; }

        /// <summary>
        /// The current state of the serial adapter, passed into the constructor.
        /// </summary>
        protected SerialAdapterState AdapterState { get; set; }

        /// <summary>
        /// Whether only bit (no byte) commands should be sent to the adapter.
        /// </summary>
        protected virtual bool BitsOnly => false;

        /// <summary>
        /// Maximum number of bytes streamed at once.
        /// </summary>
        public const int MaxBytesStreamed = 64;

        public SerialPacketBuilder(SerialAdapterState adapterState)
        {
            PacketList = new List<RawPacket>();
            Packet = new RawPacket();
            AdapterState = adapterState;

            ResetBuilder();
        }

        /// <summary>
        /// Reset the packet builder to start constructing a new packet.
        /// </summary>
        public virtual void ResetBuilder()
        {
            // clear the packet list and the packet data
            PacketList.Clear();
            Packet.Clear();
        }

        /// <summary>
        /// Save the currently constructed packet into the list and prepare a new one.
        /// </summary>
        public virtual void SaveAndNewPacket()
        {
            PacketList.Add(Packet);
            Packet = new RawPacket();
        }

        /// <summary>
        /// Adds a 1-Wire Reset command to reset the 1-Wire Network at the current speed.
        /// </summary>
        public virtual void OneWireReset()
        {
            // switch to command mode
            SwitchToCommandMode();

            // append the reset command at the current speed
            Packet.Data.Add((byte)(Function.Reset | AdapterState.SpeedMode));
            ++Packet.ReturnLength;

            // are we streaming resets?
            if (!AdapterState.CanStreamResets)
            {
                SaveAndNewPacket();
            }

            // check for 2480 wait on extra bytes packet
            if (AdapterState.LongAlarmCheck && (
                AdapterState.SpeedMode == SerialAdapterState.Speed.Regular ||
                AdapterState.SpeedMode == SerialAdapterState.Speed.Flex
            ))
            {
                SaveAndNewPacket();
            }
        }

        /// <summary>
        /// Append data bytes (to read/write) to the packet.
        /// </summary>
        /// <param name="bytes">The enumerable of data bytes to read/write.</param>
        public virtual void DataBytes(IEnumerable<byte> bytes)
        {
            if (!BitsOnly)
            {
                SwitchToDataMode();
            }
            
            // check each byte to see if some need duplication
            var byteArray = bytes.ToArray();
            foreach (byte b in byteArray)
            {
                if (BitsOnly)
                {
                    // change bytes to bits
                    for (int j = 0; j < 8; ++j)
                    {
                        DataBit(((b >> j) & 0x1) == 0x1, false);
                    }
                }
                else
                {
                    // append the data
                    Packet.Data.Add(b);

                    // check for duplicates needed for special characters
                    if (b == SerialAdapterState.Mode.Command ||
                        (b == SerialAdapterState.Mode.Special && AdapterState.Revision == SerialAdapterState.ChipVersion1)
                    )
                    {
                        Packet.Data.Add(b);
                    }

                    // add a return byte
                    ++Packet.ReturnLength;

                    if (Packet.Data.Count > MaxBytesStreamed || !AdapterState.CanStreamBytes)
                    {
                        SaveAndNewPacket();
                    }
                }
            }
        }

        /// <summary>
        /// Append a byte (to read/write) to the packet.
        /// </summary>
        /// <param name="b">The data byte to read/write.</param>
        public virtual void DataByte(byte b)
        {
            DataBytes(Enumerable.Repeat(b, 1));
        }

        /// <summary>
        /// Append a byte (to read/write) to the packet, then perform a strong pull-up when the byte has been sent.
        /// </summary>
        /// <param name="b">The data byte to read/write.</param>
        public virtual void PrimedDataByte(byte b)
        {
            for (int i = 0; i < 8; ++i)
            {
                // activate pullup with last bit (7)
                DataBit(((b >> i) & 0x1) == 0x1, (i == 7));
            }
        }

        /// <summary>
        /// Append a bit (to read/write) to the packet.
        /// </summary>
        /// <param name="dataBit">The bit to append.</param>
        /// <param name="strong5V">Whether to switch to 5V strong pull-up after the bit is sent.</param>
        public virtual void DataBit(bool dataBit, bool strong5V)
        {
            SwitchToCommandMode();

            // append the bit with polarity and strong5V options
            var byteToAppend = (byte)(
                Function.Bit |
                AdapterState.SpeedMode |
                (dataBit ? Bit.One : Bit.Zero)
            );
            if (strong5V)
            {
                byteToAppend |= BitPrime5VFlag;
            }
            Packet.Data.Add(byteToAppend);
            ++Packet.ReturnLength;

            if (Packet.Data.Count > MaxBytesStreamed || !AdapterState.CanStreamBits)
            {
                SaveAndNewPacket();
            }
        }

        /// <summary>
        /// Append a Search to the packet. Assumes that any reset and Search commands have already been appended; this
        /// only adds the search logic itself.
        /// </summary>
        /// <param name="state">The current 1-Wire state.</param>
        public virtual void Search(OneWireSerialState state)
        {
            // enter command mode
            SwitchToCommandMode();

            // activate search mode
            Packet.Data.Add((byte)(Function.SearchOn | AdapterState.SpeedMode));

            // enter data mode
            SwitchToDataMode();

            // create the search sequence character array
            var searchSequence = new BitStringBuffer(128);
            var addressSequence = new BitStringBuffer(state.LastFoundAddress.ToByteArray(), 64);

            if (state.SearchLastDiscrepancy != 0xFF)
            {
                // set the bits
                for (int i = 0; i < 64; ++i)
                {
                    // if before last discrepancy, take from address
                    // if at last discrepancy, set to 1

                    searchSequence[2*i + 1] = (i < state.SearchLastDiscrepancy - 1)
                        ? addressSequence[i]
                        : true;
                }
            }

            // add this sequence
            Packet.Data.AddRange(searchSequence.Buffer);

            // enter command mode
            SwitchToCommandMode();

            // search mode off
            Packet.Data.Add((byte)(Function.SearchOff | AdapterState.SpeedMode));

            // add to the return byte count
            Packet.ReturnLength += 16;
        }
    }
}
