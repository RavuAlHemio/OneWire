using System;

namespace RavuAlHemio.OneWire
{
    public interface IOneWireLink : IDisposable
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
        /// Attempts to acquire a 1-Wire Net port adapter and bind it to a given symbolic port number.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number to use for this port, if possible.</param>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <returns>
        /// <c>true</c> if the port adapter was acquired and assigned the given port number successfully; <c>false</c>
        /// otherwise.
        /// </returns>
        /// <seealso cref="AcquireEx"/>
        /// <seealso cref="Release"/>
        bool Acquire(int portNumber, string portName);

        /// <summary>
        /// Attempts to acquire a 1-Wire Net port adapter and returns its symbolic port number.
        /// </summary>
        /// <param name="portName">The name of the port to acquire.</param>
        /// <returns>
        /// The non-zero port number if the port was successfully acquired; <c>-1</c> otherwise.
        /// </returns>
        /// <seealso cref="Acquire"/>
        /// <seealso cref="Release"/>
        int AcquireEx(string portName);

        /// <summary>
        /// Releases a previously acquired 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">The symbolic port number of the port to release.</param>
        /// <seealso cref="Acquire"/>
        /// <seealso cref="AcquireEx"/>
        void Release(int portNumber);

        /// <summary>
        /// Send 8 bits of communication to the 1-Wire Net, change the level of the 1-Wire Net to power delivery, and
        /// return the resulting 8 bits read from the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="byteToSend">The byte to send to the 1-Wire Net.</param>
        /// <returns>
        /// The byte read from the 1-Wire Net after the <paramref name="byteToSend"/> was sent and the power level has
        /// been changed.
        /// </returns>
        byte TouchByteAndSetPower(int portNumber, byte byteToSend);

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

        /// <summary>
        /// Pops the error number of the most recent error from the error stack.
        /// </summary>
        /// <returns>The error number popped from the error stack.</returns>
        int PopErrorNumber();

        /// <summary>
        /// Whether the link has experienced an error.
        /// </summary>
        /// <value><c>true</c> if this link has experienced an error; <c>false </c> otherwise.</value>
        bool HasError { get; }
    }
}
