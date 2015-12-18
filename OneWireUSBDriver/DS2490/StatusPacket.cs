using System;

namespace OneWireUSBDriver.DS2490
{
    internal struct StatusPacket
    {
        public const byte OneWireDeviceDetect = 0xA5;

        public static class EnableFlag
        {
            public const byte StrongPullUpEnabled = 0x01;
            public const byte ProgrammingPulseEnabled = 0x02;
            public const byte SpeedChangeEnabled = 0x04;
        }

        [Flags]
        public enum StatusFlag : byte
        {
            StrongPullUpActive = 0x01,
            ProgrammingPulseActive = 0x02,
            TwelveVoltProgrammingVoltage = 0x04,
            IsPowered = 0x08,
            IsHalted = 0x10,
            IsIdle = 0x20
        }

        public static class ResultRegister
        {
            /// <summary>
            /// No presence pulse after a RESET or SET PATH command.
            /// </summary>
            public const byte NRS = 0x01; // Nothing after ReSet?

            /// <summary>
            /// Short circuit after a RESET or SET PATH command.
            /// </summary>
            public const byte SH = 0x02; // SHort

            /// <summary>
            /// Alarming Presence Pulse after a RESET command.
            /// </summary>
            public const byte APP = 0x04; // Alarming Presence Pulse

            /// <summary>
            /// Voltage not set to 12V when sending a PULSE with TYPE=1 or a WRITE EPROM command.
            /// </summary>
            public const byte VPP = 0x08; // Voltage Programming Pulse?

            /// <summary>
            /// Error reading confirmation byte of SET PATH or error performing WRITE EPROM.
            /// </summary>
            public const byte CMP = 0x10; // CoMParison?

            /// <summary>
            /// CRC error when performing WRITE SRAM PAGE, WRITE EPROM, READ EPROM, READ CRC PROT PAGE or READ
            /// REDIRECT PAGE WITH CRC.
            /// </summary>
            public const byte CRC = 0x20;

            /// <summary>
            /// READ REDIRECT PAGE WITH CRC encountered a redirected page.
            /// </summary>
            public const byte RDP = 0x40; // ReDirected Page

            /// <summary>
            /// SEARCH ACCESS with SM=1 ended sooner than expected with too few ROM IDs.
            /// </summary>
            public const byte EOS = 0x80; // End Of Search?
        }

        public static class Duration
        {
            public const ushort StrongPullupMillisecondsMultiple = 128;
            public const ushort StrongPullupDefaultMultiplier = 512 / StrongPullupMillisecondsMultiple;

            public const ushort ProgrammingPulseMillisecondsMultiple = 8;
            public const ushort ProgrammingPulseDefaultMultiplier = 512 / ProgrammingPulseMillisecondsMultiple;
        }

        public byte EnableFlags;
        public byte OneWireSpeed;
        public byte StrongPullUpDuration;
        public byte ProgrammingPulseDuration;
        public byte PullDownSlewRate;
        public byte Write1LowTime;
        public byte DSOW0RecoveryTime;
        public byte Reserved1;
        public StatusFlag StatusFlags;
        public byte CurrentCommunicateCommand1;
        public byte CurrentCommunicateCommand2;
        /// <summary>
        /// Status of the buffer for Communicate commands.
        /// </summary>
        public byte CommunicateBufferStatus;
        /// <summary>
        /// Status of the buffer we write to.
        /// </summary>
        public byte WriteBufferStatus;
        /// <summary>
        /// Status of the buffer we read from.
        /// </summary>
        public byte ReadBufferStatus;
        public byte Reserved2;
        public byte Reserved3;
        public byte[] CommunicateResultCodes;
    }
}
