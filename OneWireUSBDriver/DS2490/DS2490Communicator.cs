using System;
using System.Diagnostics;
using System.Linq;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace OneWireUSBDriver.DS2490
{
    internal class DS2490Communicator
    {
        private const int UsbTimeout = 5000;
        public const ReadEndpointID Endpoint1 = (ReadEndpointID)0x81;
        public const WriteEndpointID Endpoint2 = (WriteEndpointID)0x02;
        public const ReadEndpointID Endpoint3 = (ReadEndpointID)0x83;

        private UsbDevice _device;

        public DS2490Communicator(UsbDevice device)
        {
            _device = device;
        }

        private bool ControlTransferNoData(UsbSetupPacket packet)
        {
            int transferred;
            return _device.ControlTransfer(ref packet, IntPtr.Zero, 0, out transferred);
        }

        private static UsbSetupPacket MakeSetupPacket(byte request, short value, short index = 0)
        {
            return new UsbSetupPacket
            {
                RequestType = 0x40,
                Request = request,
                Value = value,
                Index = index,
                Length = 0
            };
        }

        /// <summary>
        /// Attempts to resync and detect a DS2490.
        /// </summary>
        /// <returns><c>true</c> if the DS2490 was detected successfully; <c>false</c> otherwise.</returns>
        public bool Detect()
        {
            // reset the DS2490
            Reset();

            // set strong pullup duration to infinite
            var infiniteStrongPullup = MakeSetupPacket(
                request: SetupPacket.RequestType.Communicate,
                value: SetupPacket.CommunicateValue.SetDuration | SetupPacket.CommunicateValue.Lower.IM
            );
            ControlTransferNoData(infiniteStrongPullup);

            // set 12V pullup duration to 512µs
            var twelveVoltPullup512Us = MakeSetupPacket(
                request: SetupPacket.RequestType.Communicate,
                value: SetupPacket.CommunicateValue.SetDuration | SetupPacket.CommunicateValue.Lower.IM | SetupPacket.CommunicateValue.Lower.Type,
                index: 0x0040
            );
            ControlTransferNoData(twelveVoltPullup512Us);

            // disable strong pullup but leave program pulse enabled (faster)
            var disableStrongPullup = MakeSetupPacket(
                request: SetupPacket.RequestType.Mode,
                value: SetupPacket.ModeValue.EnablePulse,
                index: SetupPacket.EnablePulse.Programming
            );
            ControlTransferNoData(disableStrongPullup);

            // return result of short-circuit check
            bool present, programmingVoltage;
            return ShortCheck(out present, out programmingVoltage);
        }

        /// <summary>
        /// Check if there is a short circuit on the 1-Wire bus. Used to stop communication with the DS2490 while the
        /// short circuit is in effect to not overrun the buffers.
        /// </summary>
        /// <param name="present">Whether a device is present on the 1-Wire bus.</param>
        /// <param name="programmingVoltage">Whether programming voltage was detected.</param>
        /// <returns>
        /// <c>true</c> if there is no short circuit on the 1-Wire Network; <c>false</c> if the 1-Wire Network is
        /// short-circuited or the DS2490 could not be detected.
        /// </returns>
        public bool ShortCheck(out bool present, out bool programmingVoltage)
        {
            // pre-set
            present = false;
            programmingVoltage = false;

            StatusPacket? maybeStatus = GetStatus();
            if (!maybeStatus.HasValue)
            {
                return false;
            }

            var status = maybeStatus.Value;

            // is programming voltage present?
            programmingVoltage = status.StatusFlags.HasFlag(StatusPacket.StatusFlag.TwelveVoltProgrammingVoltage);

            // check for short-circuit
            if (status.CommunicateBufferStatus != 0)
            {
                // broken
                return false;
            }

            // check for SH bit
            if (status.CommunicateResultCodes.Any(c => ((c & StatusPacket.ResultRegister.SH) != 0)))
            {
                // shorted
                return false;
            }

            // device might be present
            present = true;

            // check if there is a status packet that isn't a ONEWIREDEVICEDETECT pack and has a NRS FLAG
            if (status.CommunicateResultCodes.Any(c =>
                c != StatusPacket.OneWireDeviceDetect &&
                (c & StatusPacket.ResultRegister.NRS) != 0)
            )
            {
                // crud
                present = false;
            }

            return true;
        }

        /// <summary>
        /// Stop any ongoing pulses.
        /// </summary>
        /// <returns><c>true</c> if the pulse was stopped; <c>false</c> if stopping the pulse failed.</returns>
        public bool HaltPulse()
        {
            long startTime = Stopwatch.GetTimestamp();
            TimeSpan duration;
            var maxDuration = TimeSpan.FromMilliseconds(500);

            do
            {
                // perform Halt Execution When Idle, then Resume Execution, to stop an infinite pulse

                var haltExecutionWhenIdle = MakeSetupPacket(
                    request: SetupPacket.RequestType.Control,
                    value: SetupPacket.ControlValue.HaltExecutionWhenIdle
                );
                if (!ControlTransferNoData(haltExecutionWhenIdle))
                {
                    return false;
                }

                var resumeExecution = MakeSetupPacket(
                    request: SetupPacket.RequestType.Control,
                    value: SetupPacket.ControlValue.ResumeExecution
                );
                if (!ControlTransferNoData(resumeExecution))
                {
                    return false;
                }

                long nowTime = Stopwatch.GetTimestamp();
                duration = TimeSpan.FromSeconds((nowTime - startTime)/(double)Stopwatch.Frequency);

                // check if the pulse stopped
                StatusPacket? maybeStatus = GetStatus();
                if (!maybeStatus.HasValue)
                {
                    return false;
                }
                if (!maybeStatus.Value.StatusFlags.HasFlag(StatusPacket.StatusFlag.StrongPullUpActive))
                {
                    // success; disable both pulse types
                    var disablePulses = MakeSetupPacket(
                        request: SetupPacket.RequestType.Mode,
                        value: SetupPacket.ModeValue.EnablePulse
                    );
                    ControlTransferNoData(disablePulses);

                    return true;
                }

            } while (duration < maxDuration);

            return false;
        }

        /// <summary>
        /// Obtains status information about the DS2490 device.
        /// </summary>
        /// <returns>
        /// Status information about this device, or <c>null</c> if fetching the status information failed.
        /// </returns>
        public StatusPacket? GetStatus()
        {
            int transferLength;
            var buffer = new byte[32];

            using (var reader = _device.OpenEndpointReader(Endpoint1))
            {
                if (reader.Read(buffer, UsbTimeout, out transferLength) != ErrorCode.Success)
                {
                    return null;
                }
            }

            var packet = new StatusPacket
            {
                EnableFlags = buffer[0],
                OneWireSpeed = buffer[1],
                StrongPullUpDuration = buffer[2],
                ProgrammingPulseDuration = buffer[3],
                PullDownSlewRate = buffer[4],
                Write1LowTime = buffer[5],
                DSOW0RecoveryTime = buffer[6],
                Reserved1 = buffer[7],
                StatusFlags = (StatusPacket.StatusFlag)buffer[8],
                CurrentCommunicateCommand1 = buffer[9],
                CurrentCommunicateCommand2 = buffer[10],
                CommunicateBufferStatus = buffer[11],
                WriteBufferStatus = buffer[12],
                ReadBufferStatus = buffer[13],
                Reserved2 = buffer[14],
                Reserved3 = buffer[15]
            };

            // take care of the CommunicationResultCodes, too
            int resultCodeCount = Math.Max(16 - transferLength, 0);
            packet.CommunicateResultCodes = new byte[resultCodeCount];
            for (int i = 0; i < resultCodeCount; ++i)
            {
                packet.CommunicateResultCodes[i] = buffer[16 + i];
            }

            return packet;
        }

        /// <summary>
        /// Performs a hardware reset of the DS2490 equivalent to a power-on reset.
        /// </summary>
        /// <returns><c>true</c> if the DS2490 was reset successfully; <c>false</c> otherwise.</returns>
        public bool Reset()
        {
            var setup = new UsbSetupPacket
            {
                RequestType = 0x40,
                Request = SetupPacket.RequestType.Control,
                Value = SetupPacket.ControlValue.ResetDevice,
                Index = 0x00,
                Length = 0x00
            };
            return ControlTransferNoData(setup);
        }

        /// <summary>
        /// Reads data from endpoint 3.
        /// </summary>
        /// <param name="device">The DS2490 to contact.</param>
        /// <param name="buf">The buffer into which to store the data.</param>
        /// <param name="count">The length of the buffer.</param>
        /// <returns>The number of bytes read, or <c>-1</c> if reading failed.</returns>
        public int Read(IUsbDevice device, byte[] buf, int count)
        {
            int transferLength;

            using (var reader = device.OpenEndpointReader(Endpoint3))
            {
                if (reader.Read(buf, 0, count, UsbTimeout, out transferLength) != ErrorCode.Success)
                {
                    return -1;
                }
            }

            return transferLength;
        }

        /// <summary>
        /// Writes data to endpoint 2.
        /// </summary>
        /// <param name="device">The DS2490 to contact.</param>
        /// <param name="buf">The buffer from which to read the data.</param>
        /// <param name="count">The length of the buffer.</param>
        /// <returns>The number of bytes written, or <c>-1</c> if writing failed.</returns>
        public int Write(IUsbDevice device, byte[] buf, int count)
        {
            int transferLength;

            using (var writer = device.OpenEndpointWriter(Endpoint2))
            {
                if (writer.Write(buf, 0, count, UsbTimeout, out transferLength) != ErrorCode.Success)
                {
                    return -1;
                }
            }

            return transferLength;
        }
    }
}
