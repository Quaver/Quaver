using System;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.V2.Profile
{
    /// <summary>
    ///  Logic ported from Website
    /// </summary>
    internal sealed class ProfileAccentPalette
    {
        public Color Accent { get; }

        public Color Readable { get; }

        public Color Content { get; }

        public ProfileAccentPalette(Color accent, Color readable, Color content)
        {
            Accent = accent;
            Readable = readable;
            Content = content;
        }
    }

    internal static class ProfileAccentColors
    {
        private static readonly Color ReadableBackground = new Color(54, 54, 54);

        private static readonly Color DarkContent = new Color(39, 39, 39);

        public static bool TryCreate(bool customizable, string accentHex, out ProfileAccentPalette palette)
        {
            palette = null;
            if (!customizable || !TryParseHex(accentHex, out var accent))
                return false;

            palette = new ProfileAccentPalette(accent, GetReadable(accent), GetContentColor(accent));
            return true;
        }

        private static Color GetReadable(Color accent)
        {
            if (GetContrastRatio(accent, ReadableBackground) >= 4.5)
                return accent;

            var low = 0d;
            var high = 1d;
            for (var iteration = 0; iteration < 20; iteration++)
            {
                var mix = (low + high) / 2;
                var mixed = MixWithWhite(accent, mix);

                if (GetContrastRatio(mixed, ReadableBackground) >= 4.5)
                    high = mix;
                else
                    low = mix;
            }

            return MixWithWhite(accent, high);
        }

        public static Color GetContentColor(Color accent) =>
            GetContrastRatio(accent, DarkContent) >= GetContrastRatio(accent, Color.White) ? DarkContent : Color.White;

        private static Color MixWithWhite(Color color, double amount) => new Color(
            (byte) Math.Round(color.R + (255 - color.R) * amount, MidpointRounding.AwayFromZero),
            (byte) Math.Round(color.G + (255 - color.G) * amount, MidpointRounding.AwayFromZero),
            (byte) Math.Round(color.B + (255 - color.B) * amount, MidpointRounding.AwayFromZero));

        private static double GetContrastRatio(Color foreground, Color background)
        {
            var foregroundLuminance = GetRelativeLuminance(foreground);
            var backgroundLuminance = GetRelativeLuminance(background);
            return (Math.Max(foregroundLuminance, backgroundLuminance) + 0.05) /
                   (Math.Min(foregroundLuminance, backgroundLuminance) + 0.05);
        }

        private static double GetRelativeLuminance(Color color) =>
            0.2126 * ToLinear(color.R) +
            0.7152 * ToLinear(color.G) +
            0.0722 * ToLinear(color.B);

        private static double ToLinear(byte channel)
        {
            var normalized = channel / 255d;
            return normalized <= 0.03928
                ? normalized / 12.92
                : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        private static bool TryParseHex(string value, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var hex = value.Trim();
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length != 6 ||
                !byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var red) ||
                !byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var green) ||
                !byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var blue))
                return false;

            color = new Color(red, green, blue);
            return true;
        }
    }
}
