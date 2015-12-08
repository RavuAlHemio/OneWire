namespace RavuAlHemio.OneWire.Driver
{
    /// <summary>
    /// Network-level interface to a 1-Wire Net.
    /// </summary>
    public interface IOneWireNetwork
    {
        /// <summary>
        /// Find the first device on the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="resetBeforeSearch">Whether to reset the 1-Wire Net before searching.</param>
        /// <param name="alarmOnly">
        /// If <c>true</c>, searches only for devices in an alarm state. If <c>false</c>, searches for all devices.
        /// </param>
        /// <returns>
        /// Whether a device has been found. Its serial number can be retrieved using
        /// <see cref="GetSerialNumber"/>.
        /// </returns>
        bool FindFirstDevice(int portNumber, bool resetBeforeSearch, bool alarmOnly);

        /// <summary>
        /// Find another device on the 1-Wire Net (after <see cref="FindFirstDevice"/> has been called at least once).
        /// </summary>
        /// <remarks>
        /// To restart the search, call <see cref="FindFirstDevice"/> instead.
        /// </remarks>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="resetBeforeSearch">Whether to reset the 1-Wire Net before searching.</param>
        /// <param name="alarmOnly">
        /// If <c>true</c>, searches only for devices in an alarm state. If <c>false</c>, searches for all devices.
        /// </param>
        /// <returns>
        /// Whether a device has been found. Its serial number can be retrieved using
        /// <see cref="GetSerialNumber"/>.
        /// </returns>
        bool FindNextDevice(int portNumber, bool resetBeforeSearch, bool alarmOnly);

        /// <summary>
        /// Returns the serial number of the last-found device on the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <returns>The serial number of the last-found interface on the 1-Wire Net.</returns>
        ulong GetSerialNumber(int portNumber);

        /// <summary>
        /// Sets the serial number of the last-found device on the 1-Wire Net.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="serialNumber">The serial number to which to set the internal buffer.</param>
        void SetSerialNumber(int portNumber, ulong serialNumber);

        /// <summary>
        /// Setup the searching algorithm to find the device of a given device family when <see cref="FindNextDevice"/>
        /// is called.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        /// <param name="searchFamily">The device family for which to search.</param>
        void FamilySearchSetup(int portNumber, byte searchFamily);

        /// <summary>
        /// Setup the searching algorithm to find the device of the next device family when
        /// <see cref="FindNextDevice"/> is called.
        /// </summary>
        /// <param name="portNumber">Indicates the symbolic port number.</param>
        void SkipFamily(int portNumber);
    }
}
