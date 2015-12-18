using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavuAlHemio.OneWire.Adapter
{
    /// <summary>
    /// Power levels for 1-Wire Network.
    /// </summary>
    public enum PowerLevel : int
    {
        /// <summary>
        /// Normal level (weak 5Volt pullup).
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The power delivery level (strong 5Volt pullup).
        /// </summary>
        PowerDelivery = 1,

        /// <summary>
        /// Reset power level (strong pulldown to 0Volts, resets 1-Wire).
        /// </summary>
        Break = 2,

        /// <summary>
        /// EPROM programming level (strong 12Volt pullup).
        /// </summary>
        Program = 3
    }
}
