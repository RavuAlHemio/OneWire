using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneWireUSBDriver.DS2490
{
    internal static class SetupPacket
    {
        public static class RequestType
        {
            public const byte Control = 0x00;
            public const byte Communicate = 0x01;
            public const byte Mode = 0x02;
            public const byte Test = 0x03;
        }

        public static class ControlValue
        {
            public const short ResetDevice = 0x0000;
            public const short StartExecution = 0x0001;
            public const short ResumeExecution = 0x0002;
            public const short HaltExecutionWhenIdle = 0x0003;
            public const short HaltExecutionWhenDone = 0x0004;
            public const short CancelCommand = 0x0005;
            public const short CancelMacro = 0x0006;
            public const short FlushCommunicationCommands = 0x0007;
            public const short FlushReceiveBuffer = 0x0008;
            public const short FlushTransmitBuffer = 0x0009;
            public const short GetCommunicationCommands = 0x000A;
        }

        public static class CommunicateValue
        {
            public static class Lower
            {
                public const short Type = 0x0008;
                public const short SE = 0x0008;
                public const short D = 0x0008;
                public const short Z = 0x0008;
                public const short CH = 0x0008;
                public const short R = 0x0008;
                public const short IM = 0x0001;
            }

            public static class Upper
            {
                public const short PS = 0x4000;
                public const short PST = 0x4000;
                public const short CIB = 0x4000;
                public const short RTS = 0x4000;
                public const short DT = 0x2000;
                public const short SPU = 0x1000;
                public const short F = 0x0800;
                public const short ICP = 0x0200;
                public const short RST = 0x0100;
            }

            public const short ErrorEscape = 0x0601;
            public const short SetDuration = 0x0012;
            public const short BitIO = 0x0020;
            public const short Pulse = 0x0030;
            public const short OneWireReset = 0x0042;
            public const short ByteIO = 0x0052;
            public const short MatchAccess = 0x0064;
            public const short BlockIO = 0x0074;
            public const short ReadStraight = 0x0080;
            public const short DoRelease = 0x6092;
            public const short SetPath = 0x00A2;
            public const short WriteSRAMPage = 0x00B2;
            public const short WriteEPROM = 0x00C4;
            public const short ReadCRCProtPage = 0x00D4;
            public const short ReadRedirectPageCRC = 0x21E4;
            public const short SearchAccess = 0x00F4;
        }

        public static class ReadStraightSpecialBits
        {
            public const short NTF = 0x0008;
            public const short ICP = 0x0004;
            public const short RST = 0x0002;
            public const short IM = 0x0001;
        }

        public static class ModeValue
        {
            public const short EnablePulse = 0x0000;
            public const short EnableSpeedChange = 0x0001;
            public const short OneWireSpeed = 0x0002;
            public const short StrongPullUpDuration = 0x0003;
            public const short PullDownSlewRate = 0x0004;
            public const short ProgrammingPulseDuration = 0x0005;
            public const short Write1LowTime = 0x0006;
            public const short Dsow0Trec = 0x0007;
        }

        public static class EnablePulse
        {
            // FIXME: C code contradicts itself:
            //> #define ENABLEPULSE_PRGE 0x01  // strong pull-up
            //> #define ENABLEPULSE_SPUE 0x02  // programming pulse
            // assume constants are correct

            public const byte Programming = 0x01;
            public const byte StrongPullUp = 0x02;
        }
    }
}
