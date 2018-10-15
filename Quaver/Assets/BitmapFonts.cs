using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Resources;
using Wobble.Graphics.BitmapFonts;

namespace Quaver.Assets
{
    public static class BitmapFonts
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

        /// <summary>
        ///     Loads all bitmap fonts.
        /// </summary>
        public static void Load()
        {
            BitmapFontFactory.AddFont(Exo2Bold, QuaverResources.exo2_bold);
            BitmapFontFactory.AddFont(Exo2BoldItalic, QuaverResources.exo2_bolditalic);
            BitmapFontFactory.AddFont(Exo2Italic, QuaverResources.exo2_italic);
            BitmapFontFactory.AddFont(Exo2Light, QuaverResources.exo2_light);
            BitmapFontFactory.AddFont(Exo2Medium, QuaverResources.exo2_medium);
            BitmapFontFactory.AddFont(Exo2MediumItalic, QuaverResources.exo2_mediumitalic);
            BitmapFontFactory.AddFont(Exo2Regular, QuaverResources.exo2_regular);
            BitmapFontFactory.AddFont(Exo2SemiBold, QuaverResources.exo2_semibold);
            BitmapFontFactory.AddFont(Exo2SemiBoldItalic, QuaverResources.exo2_semibolditalic);
            BitmapFontFactory.AddFont(Exo2Thin, QuaverResources.exo2_thin);
            BitmapFontFactory.AddFont(Exo2ThinItalic, QuaverResources.exo2_thinitalic);
        }
    }
}