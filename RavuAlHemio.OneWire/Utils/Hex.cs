using System;

namespace RavuAlHemio.OneWire.Utils
{
    public static class Hex
    {
        /// <summary>
        /// Parses the supplied character as a hexadecimal nibble and returns its value.
        /// </summary>
        /// <param name="c">The character to parse.</param>
        /// <returns><paramref name="c"/> parsed as a hexadecimal nibble.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="c"/> is not a valid hexadecimal nibble.
        /// </exception>
        public static byte CharToNibble(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            else if (c >= 'A' && c <= 'F')
            {
                return (byte)(c - 'A' + 10);
            }
            else if (c >= 'a' && c <= 'f')
            {
                return (byte)(c - 'a' + 10);
            }
            throw new ArgumentOutOfRangeException(nameof(c), $"'{c}' is not a valid hexadecimal nibble");
        }
    }
}

