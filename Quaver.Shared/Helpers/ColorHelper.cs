/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Globalization;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        ///     Converts a difficulty rating to a color.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        internal static Color DifficultyToColor(float rating)
        {
            // Beginner
            if (rating < 1) return HexToColor("#D1FFFA");
            // Easy
            if (rating < 2.5) return HexToColor("#5EFF75");
            // Normal
            if (rating < 10) return HexToColor("#5EC4FF");
            // Hard
            if (rating < 20) return HexToColor("#F5B25B");
            // Insane
            if (rating < 30) return HexToColor("#F9645D");
            // Expert
            if (rating < 40) return HexToColor("#D761EB");
            // Expert+
            if (rating < 50) return HexToColor("#7B61EB");
            // ???
            return HexToColor("#B7B7B7");
        }

        /// <summary>
        ///     Converts an osu! star rating to Quaver's color system
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        internal static Color OsuStarRatingToColor(float rating)
        {
            // Beginnner
            if (rating < 1)
                return DifficultyToColor(0.9f);
            // Easy
            if (rating < 2)
                return DifficultyToColor(3.49f);
            // Normal
            if (rating < 2.70f)
                return DifficultyToColor(7.99f);
            // Hard
            if (rating < 4)
                return DifficultyToColor(18.9f);
            // Insane
            if (rating < 5.28)
                return DifficultyToColor(27.9f);

            // Expert
            return DifficultyToColor(999);
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

        /// <summary>
        ///     Converts a System.Drawing color to xna.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color ToXnaColor(System.Drawing.Color color) => new Color(color.R, color.G, color.B);

        public static Color BeatSnapToColor(int snap)
        {
            var color = Color.White;

            switch (snap)
            {
                case 1:
                    color = Color.White;
                    break;
                case 2:
                    color = HexToColor("#E10B01");
                    break;
                case 3:
                    color = HexToColor("#9B51E0");
                    break;
                case 4:
                    color = HexToColor("#0587E5");
                    break;
                case 6:
                    color = HexToColor("#BB6BD9");
                    break;
                case 8:
                    color = HexToColor("#E9B736");
                    break;
                case 12:
                    color = HexToColor("#D34B8C");
                    break;
                case 16:
                    color = HexToColor("#FFE76B");
                    break;
            }

            return color;
        }
    }
}
