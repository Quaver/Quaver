/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Localization;
using Wobble;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Assets
{
    public static class Fonts
    {
        private const int NotoCjkWeight = FontWeight.SemiBold;

        public static string Exo2Bold { get; } = "exo2-bold";
        public static string Exo2BoldItalic { get; } = "exo2-bolditalic";
        public static string Exo2Medium { get; } = "exo2-medium";
        public static string Exo2Regular { get; } = "exo2-regular";
        public static string Exo2SemiBold { get; } = "exo2-semibold";

        #region NEW_FONTS
        public static string LatoRegular { get; } = "Lato-Regular";
        public static string LatoSemiBold { get; } = "Lato-Semibold";
        public static string LatoBold { get; } = "Lato-Bold";
        public static string LatoLight { get; } = "Lato-Light";
        public static string LatoHeavy { get; } = "Lato-Heavy";
        public static string LatoBlack { get; } = "Lato-Black";

        #endregion

        private const string Inter = "Inter";

        /// <summary>
        /// </summary>
        public static void LoadWobbleFonts()
        {
            const string folder = "Quaver.Resources/Fonts";

            // Load fallback fonts or fonts that are used across multiple WobbleFonts
            const string emojiString = "Emoji";
            var emojiFont = GameBase.Game.Resources.Get($@"{folder}/NotoColorEmoji/NotoColorEmoji.ttf");

            const string cjkString = "CJK";
            var notoCjkFont = new WobbleFontFace(
                GameBase.Game.Resources.Get($@"{folder}/NotoCJK/NotoSansCJK-VF.ttf.ttc"),
                QuaverLocalization.GetNotoCjkFaceIndex(ConfigManager.Language.Value),
                NotoCjkWeight);

            var interFont = GameBase.Game.Resources.Get($"{folder}/Inter/{Inter}.ttf");

            Dictionary<string, WobbleFontFace> CreateFallbacks() => new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                };

            void CacheInterFont(string name, int weight)
            {
                FontManager.CacheWobbleFont(name, new WobbleFontStore(20,
                    new WobbleFontFace(interFont, weight: weight), CreateFallbacks()));
            }

            CacheInterFont(LatoRegular, FontWeight.Regular);
            CacheInterFont(LatoSemiBold, FontWeight.SemiBold);
            CacheInterFont(LatoBold, FontWeight.Bold);
            CacheInterFont(LatoLight, FontWeight.Light);
            CacheInterFont(LatoHeavy, FontWeight.ExtraBold);
            CacheInterFont(LatoBlack, FontWeight.Bold);

            var dir = $"{WobbleGame.WorkingDirectory}/Fonts";
            Directory.CreateDirectory(dir);

            // Copy over
            File.WriteAllBytes($"{dir}/inter.ttf", interFont);
            File.WriteAllBytes($"{dir}/lato-black.ttf", interFont);
        }
    }
}
