using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        ///     Converts a difficulty rating to a color.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        [Obsolete("Please use Colokrs class in Quaver.API")]
        internal static Color DifficultyToColor(float rating)
        {
            // Easy
            if (rating < 15)
                return new Color(0, 255, 0);
            // Medium
            if (rating < 30)
                return new Color(255, 255, 0);
            // Hard
            return new Color(255, 0, 0);
        }

        /// <summary>
        ///     Converts a hex color code into an XNA color.
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Color HexToColor(string hexColor)
        {
            //Remove # if present
            if (hexColor.IndexOf('#') != -1)
                hexColor = hexColor.Replace("#", "");

            var red = 0;
            var green = 0;
            var blue = 0;

            switch (hexColor.Length)
            {
                case 6:
                    //#RRGGBB
                    red = int.Parse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                    green = int.Parse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                    blue = int.Parse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                    break;
                case 3:
                    //#RGB
                    red = int.Parse(hexColor[0].ToString() + hexColor[0], NumberStyles.AllowHexSpecifier);
                    green = int.Parse(hexColor[1].ToString() + hexColor[1], NumberStyles.AllowHexSpecifier);
                    blue = int.Parse(hexColor[2].ToString() + hexColor[2], NumberStyles.AllowHexSpecifier);
                    break;
            }

            return new Color(red, green, blue);
        }
    }
}
