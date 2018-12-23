/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Globalization;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Helpers
{
    public static class ColorHelper
    {
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
        ///     Convert System.Drawing Color to Microsoft.Xna Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color SystemToXnaColor(System.Drawing.Color color) => new Color(color.R, color.G, color.B);
    }
}
