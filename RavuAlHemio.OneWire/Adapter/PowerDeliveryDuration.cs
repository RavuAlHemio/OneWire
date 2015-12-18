using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavuAlHemio.OneWire.Adapter
{
    public enum PowerDeliveryDuration : int
    {
        /// <summary>
        /// Provide power for a 1/2 second.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.SupportsLevel"/> with <see cref="PowerLevel.PowerDelivery"/>.
        /// </remarks>
        HalfSecond = 0,

        /// <summary>
        /// Provide power for 1 second.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.SupportsLevel"/> with <see cref="PowerLevel.PowerDelivery"/>.
        /// </remarks>
        OneSecond = 1,

        /// <summary>
        /// Provide power for 2 seconds.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.SupportsLevel"/> with <see cref="PowerLevel.PowerDelivery"/>.
        /// </remarks>
        TwoSeconds = 2,

        /// <summary>
        /// Provide power for 4 seconds.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.SupportsLevel"/> with <see cref="PowerLevel.PowerDelivery"/>.
        /// </remarks>
        FourSeconds = 3,

        /// <summary>
        /// Provide power until the device is no longer drawing significant power.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.CanDeliverSmartPower"/>.
        /// </remarks>
        SmartDone = 4,

        /// <summary>
        /// Provide power until <see cref="DSPortAdapter.SetPowerNormal()"/> is called.
        /// </summary>
        /// <remarks>
        /// Check using <see cref="DSPortAdapter.SupportsLevel"/> with <see cref="PowerLevel.PowerDelivery"/>.
        /// </remarks>
        Infinite = 5,

        /// <summary>
        /// Current detect power delivery.
        /// </summary>
        CurrentDetect = 6,

        /// <summary>
        /// 480 µs power delivery.
        /// </summary>
        EPROM = 7
    }
}
