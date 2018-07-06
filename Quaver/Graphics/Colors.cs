using Microsoft.Xna.Framework;
using Quaver.Helpers;

namespace Quaver.Graphics
{
    public struct Colors
    {
        /// <summary>
        ///     Color for dead long notes.
        /// </summary>
        public static readonly Color DeadLongNote = new Color(50, 50, 50);

 #region QUAVER_COLORS

        /// <summary>
        ///     Main Accent Color
        /// </summary>
        public static readonly Color MainAccent = new Color(81,197,249);
        public static readonly Color MainAccentInactive = new Color(6,71,122);
        
        /// <summary>
        ///     Secondary Accent Color
        /// </summary>
        public static readonly Color SecondaryAccent = new Color(255,222,124);
        public static readonly Color SecondaryAccentInactive = new Color(128,97,1);
        
        /// <summary>
        ///     Negative color (Red)
        /// </summary>
        public static readonly Color Negative = new Color(255,152,164);
        public static readonly Color NegativeInactive = new Color(119,20,31);        

#endregion        

        /// <summary>
        ///     Dark gray color, usually used for headers.
        /// </summary>
        public static readonly Color DarkGray = ColorHelper.HexToColor("#252a3e");

        /// <summary>
        ///     Legend has it, a legendary legend used this color.
        /// </summary>
        public static readonly Color Swan = ColorHelper.HexToColor("#db88c2");
    }
}
