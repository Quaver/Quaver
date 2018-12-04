using Microsoft.Xna.Framework;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Graphics
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
        ///     Converts an XNA color to System.Drawing
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Drawing.Color XnaToSystemDrawing(Color color) => System.Drawing.Color.FromArgb(color.R, color.G, color.B);

        /// <summary>
        ///     Gets the chat color of a given user
        /// </summary>
        /// <param name="userGroups"></param>
        /// <returns></returns>
        public static Color GetUserChatColor(UserGroups userGroups)
        {
            if (userGroups.HasFlag(UserGroups.Developer))
                return Swan;
            if (userGroups.HasFlag(UserGroups.Bot))
                return ColorHelper.HexToColor($"#f8ff97");
            if (userGroups.HasFlag(UserGroups.Admin))
                return MainAccent;
            if (userGroups.HasFlag(UserGroups.Moderator))
                return ColorHelper.HexToColor($"#9d84ec");
            if (userGroups.HasFlag(UserGroups.Normal))
                return Color.White;

            return Color.White;
        }
    }
}
