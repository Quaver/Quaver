using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Helpers;

namespace Quaver.Graphics
{
    public static class Colors
    {
        /// <summary>
        ///     Color for dead long notes.
        /// </summary>
        public static readonly Color DeadLongNote = new Color(50, 50, 50);

        #region QUAVER_COLORS

        /// <summary>
        ///     Main Accent Color
        /// </summary>
        public static readonly Color MainAccent = new Color(81, 197, 249);
        public static readonly Color MainAccentInactive = new Color(6, 71, 122);

        /// <summary>
        ///     Secondary Accent Color
        /// </summary>
        public static readonly Color SecondaryAccent = new Color(255, 222, 124);
        public static readonly Color SecondaryAccentInactive = new Color(128, 97, 1);

        /// <summary>
        ///     Negative color (Red)
        /// </summary>
        public static readonly Color Negative = new Color(255, 152, 164);
        public static readonly Color NegativeInactive = new Color(119, 20, 31);

        #endregion

        /// <summary>
        ///     Dark gray color, usually used for headers.
        /// </summary>
        public static readonly Color DarkGray = ColorHelper.HexToColor("#252a3e");

        /// <summary>
        ///     Legend has it, a legendary legend used this color.
        /// </summary>
        public static readonly Color Swan = ColorHelper.HexToColor("#db88c2");

        /// <summary>
        ///     Converts Xna Color to SystemDrawing Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Drawing.Color XnaToSystemDrawing(Color color) => System.Drawing.Color.FromArgb(color.R, color.G, color.B);

        /// <summary>
        ///     Converts SystemDrawing Color to Xna Color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color SystemDrawingToXna(Color color) => new Color(color.R, color.G, color.B, color.A);
    }
}
