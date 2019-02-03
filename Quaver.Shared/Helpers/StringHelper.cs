/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Quaver.Shared.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Converts score to string (1234567) format
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        internal static string ScoreToString(int score) => score.ToString("0000000");

        /// <summary>
        ///     Converts an accuracy percentage into a string.
        /// </summary>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        internal static string AccuracyToString(float accuracy) => accuracy >= 100 ? "100.00%" : $"{accuracy:00.00}%";

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
        internal static string ColorToString(Color color) => $"{color.R},{color.G},{color.G},{color.A}";

        /// <summary>
        ///     Adds an ordinal to a string.
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string AddOrdinal(int num)
        {
            if( num <= 0 ) return num.ToString();

            switch(num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch(num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }
    }
}
