using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Skinning.V2
{
    internal static class SkinV2Color
    {
        public static Color Parse(string value)
        {
            var hex = value.Substring(1);
            var red = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var green = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var blue = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var alpha = hex.Length == 8
                ? byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                : byte.MaxValue;
            return new Color(red, green, blue, alpha);
        }
    }
}
