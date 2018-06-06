using Microsoft.Xna.Framework;

namespace Quaver.Helpers
{
    internal static class ColorHelper
    {
        /// <summary>
        ///     Converts a difficulty rating to a color.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
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
    }
}