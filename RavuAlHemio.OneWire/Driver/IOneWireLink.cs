﻿using System;
using JetBrains.Annotations;

namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// Link-level interface to a 1-Wire Net.
    /// </summary>
    public interface IOneWireLink
    {
        /// <summary>
        /// Reset all of the devices on the 1-Wire Net and return the result.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns>
        /// <c>true</c> if presence pulse(s) detected and device(s) reset, <c>false</c> if no presence pulse(s)
        /// detected.
        /// </returns>
        bool TouchReset(int portNumber);

        /// <summary>
        /// Send 1 bit of communication to the 1-Wire Net and return the resulting 1 bit read from the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="bitToSend">The bit to send to the 1-Wire Net.</param>
        /// <returns>The bit read from the 1-Wire Net after <paramref name="bitToSend"/> was sent.</returns>
        bool TouchBit(int portNumber, bool bitToSend);

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net and return the resulting 8 bits read from the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>The byte read from the 1-Wire Net after the <paramref name="byteToSend"/> was sent.</returns>
        byte TouchByte(int portNumber, byte byteToSend);

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net, read the resulting 8 bits and return whether the sent and
        /// received bytes were equal.
        /// </summary>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>
        /// <c>true</c> if the byte read from the 1-Wire Net equalled the sent byte (<paramref name="byteToSend"/>);
        /// <c>false</c> if it did not.
        /// </returns>
        bool WriteByte(byte byteToSend);

        /// <summary>
        /// Sets the communication rate of the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="newSpeed">The communication rate to which to set the 1-Wire Net.</param>
        /// <returns>The current port speed, after it has been set.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown if this link does not support changes to the communication rate.
        /// </exception>
        NetSpeed SetPortSpeed(int portNumber, NetSpeed newSpeed);

        /// <summary>
        /// Sets the line level of the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="newLevel">The new line level to set.</param>
        /// <returns>The current line level, after it has been set.</returns>
        LineLevel SetLineLevel(int portNumber, LineLevel newLevel);

        /// <summary>
        /// Sends a fixed 480µs 12V pulse on the 1-Wire Net for programming EPROM iButtons.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns>
        /// <c>true</c> if the programming pulse was sent; <c>false</c> if the programming voltage was not available.
        /// </returns>
        bool ProgramPulse(int portNumber);

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net, read the resulting 8 bits, change the level of the 1-Wire
        /// Net to power delivery, and return whether the sent and received bytes were equal and the power level change
        /// succeeded.
        /// </summary>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>
        /// <c>true</c> if the byte read from the 1-Wire Net equalled the sent byte (<paramref name="byteToSend"/>) and
        /// the power level change succeeded; <c>false</c> if the bytes differed or the power level change failed (or
        /// both).
        /// </returns>
        bool WriteByteAndSetPower(byte byteToSend);

        /// <summary>
        /// Receive and return 8 bits of communication from the 1-Wire Net by writing the value <c>0xFF</c> and reading
        /// the response, then change the level of the 1-Wire Net to power delivery.
        /// </summary>
        /// <returns>The byte read from the 1-Wire Net (after sending <c>0xFF</c>).</returns>
        byte ReadByteAndSetPower();

        /// <summary>
        /// Read 1 bit of communication from the 1-Wire Net, verify that it matches the argument, and change the level
        /// of the 1-Wire Net to power delivery if it does.
        /// </summary>
        /// <remarks>
        /// Some implementations may raise the power level first, then reduce it if the response is incorrect.
        /// </remarks>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="applyPowerResponse">
        /// The response must equal this; otherwise, the power level is unchanged (or at least restored, depending on
        /// the implementation).
        /// </param>
        /// <returns>
        /// <c>true</c> if the bit read from the 1-Wire Net matched the value of <paramref name="applyPowerResponse"/>
        /// and the level was set to power delivery; <c>false</c> otherwise.
        /// </returns>
        bool ReadBitAndSetPower(int portNumber, bool applyPowerResponse);

        /// <summary>
        /// Returns whether the given port supports the power delivery line level.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns><c>true</c> if the port supports power delivery; <c>false</c> otherwise.</returns>
        /// <seealso cref="LineLevel.PowerDelivery"/>
        /// <seealso cref="SetPortSpeed"/>
        bool PortSupportsPowerDelivery(int portNumber);

        /// <summary>
        /// Returns whether the given port supports overdrive speed.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns><c>true</c> if the port supports overdrive speed; <c>false</c> otherwise.</returns>
        /// <seealso cref="NetSpeed.Overdrive"/>
        /// <seealso cref="SetLineLevel"/>
        bool PortSupportsOverdrive(int portNumber);

        /// <summary>
        /// Returns whether the given port supports sending a programming pulse.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns><c>true</c> if the port supports sending a programming pulse; <c>false</c> otherwise.</returns>
        /// <seealso cref="ProgramPulse"/>
        bool PortSupportsProgramPulse(int portNumber);
    }
}
