using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Quaver.Helpers
{
    internal static class StringHelper
    {
        /// <summary>
        /// Converts score to string (1234567) format
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static string ScoreToString(int score)
        {
            return score.ToString("0000000");
        }

        /// <summary>
        ///     Makes a string safe to be written as a file name.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string FileNameSafeString(string str)
        {
            var invalidPathChars = Path.GetInvalidFileNameChars();
            return invalidPathChars.Aggregate(str, (current, invalidChar) => current.Replace(invalidChar.ToString(), ""));
        }

        /// <summary>
        ///     Returns a color to a string
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        internal static string ColorToString(Color color)
        {
            return $"{color.R},{color.G},{color.G},{color.A}";
        }
    }
}
