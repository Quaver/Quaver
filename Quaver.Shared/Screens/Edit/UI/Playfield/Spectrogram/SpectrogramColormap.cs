using System;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram
{
    /// <summary>
    ///     Linear colormap that evenly interpolates between a series of colors
    /// </summary>
    public static class SpectrogramColormap
    {
        private static Color _startColor = new Color(13, 8, 135);
        private static Color _middleColor = new Color(203, 70, 121);
        private static Color _endColor = new Color(240, 249, 33);
        public static Color GetColor(float progress)
        {
            if (progress < 0.5f)
            {
                progress *= 2;
                return new Color
                {
                    R = (byte)(_startColor.R + (_middleColor.R - _startColor.R) * progress),
                    G = (byte)(_startColor.G + (_middleColor.G - _startColor.G) * progress),
                    B = (byte)(_startColor.B + (_middleColor.B - _startColor.B) * progress),
                    A = 255
                };
            }
            else
            {
                progress = (progress - 0.5f) * 2;
                return new Color
                {
                    R = (byte)(_middleColor.R + (_endColor.R - _middleColor.R) * progress),
                    G = (byte)(_middleColor.G + (_endColor.G - _middleColor.G) * progress),
                    B = (byte)(_middleColor.B + (_endColor.B - _middleColor.B) * progress),
                    A = 255
                };
            }
        }
    }
}