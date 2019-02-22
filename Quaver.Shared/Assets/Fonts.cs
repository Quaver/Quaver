/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Wobble;
using Wobble.Graphics.BitmapFonts;

namespace Quaver.Shared.Assets
{
    public static class Fonts
    {
        public static string Exo2Bold { get; } = "exo2-bold";
        public static string Exo2BoldItalic { get; } = "exo2-bolditalic";
        public static string Exo2Italic { get; } = "exo2-italic";
        public static string Exo2Light { get; } = "exo2-light";
        public static string Exo2Medium { get; } = "exo2-medium";
        public static string Exo2MediumItalic { get; } = "exo2-mediumitalic";
        public static string Exo2Regular { get; } = "exo2-regular";
        public static string Exo2SemiBold { get; } = "exo2-semibold";
        public static string Exo2SemiBoldItalic { get; } = "exo2-semibolditalic";
        public static string Exo2Thin { get; } = "exo2-thin";
        public static string Exo2ThinItalic { get; } = "exo2-thinitalic";
        public static string SourceSansProRegular { get; } = "sspro-regular";
        public static string SourceSansProBold { get; } = "sspro-bold";
        public static string SourceSansProSemiBold { get; } = "sspro-semibold";

        /// <summary>
        ///     Loads all bitmap fonts.
        /// </summary>
        public static void Load()
        {
            BitmapFontFactory.AddFont(Exo2Bold, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-bold.ttf"));
            BitmapFontFactory.AddFont(Exo2BoldItalic, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-bolditalic.ttf"));
            BitmapFontFactory.AddFont(Exo2Italic, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-italic.ttf"));
            BitmapFontFactory.AddFont(Exo2Light, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-light.ttf"));
            BitmapFontFactory.AddFont(Exo2Medium, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-medium.ttf"));
            BitmapFontFactory.AddFont(Exo2MediumItalic, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-mediumitalic.ttf"));
            BitmapFontFactory.AddFont(Exo2Regular, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-regular.ttf"));
            BitmapFontFactory.AddFont(Exo2SemiBold, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-semibold.ttf"));
            BitmapFontFactory.AddFont(Exo2SemiBoldItalic, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-semibolditalic.ttf"));
            BitmapFontFactory.AddFont(Exo2Thin, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-thin.ttf"));
            BitmapFontFactory.AddFont(Exo2ThinItalic, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/Exo2/exo2-thinitalic.ttf"));
            BitmapFontFactory.AddFont(SourceSansProRegular, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/SourceSansPro/sspro-regular.ttf"));
            BitmapFontFactory.AddFont(SourceSansProBold, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/SourceSansPro/sspro-bold.ttf"));
            BitmapFontFactory.AddFont(SourceSansProSemiBold, GameBase.Game.Resources.Get("Quaver.Resources/Fonts/SourceSansPro/sspro-semibold.ttf"));
        }
    }
}
