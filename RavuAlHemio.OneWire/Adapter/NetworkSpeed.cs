using System;

namespace RavuAlHemio.OneWire.Adapter
{
    /// <summary>
    /// Speed modes for 1-Wire Network.
    /// </summary>
    public enum NetworkSpeed : int
    {
        /// <summary>
        /// Normal communication speed.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Flexible communication speed, useful for long lines.
        /// </summary>
        Flex = 1,

        /// <summary>
        /// Overdrive speed.
        /// </summary>
        Overdrive = 2,

        /// <summary>
        /// Hyperdrive speed.
        /// </summary>
        Hyperdrive = 3
    }
}

