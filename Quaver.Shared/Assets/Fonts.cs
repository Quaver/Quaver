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
        private const int NotoCjkWeight = 600;

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

            // Lato-Regular
            FontManager.CacheWobbleFont(LatoRegular, new WobbleFontStore(20,
                GameBase.Game.Resources.Get($"{folder}/Lato/{LatoRegular}.ttf"), new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            // Lato-Semibold
            FontManager.CacheWobbleFont(LatoSemiBold, new WobbleFontStore(20,
                GameBase.Game.Resources.Get($"{folder}/Lato/{LatoSemiBold}.ttf"), new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            // Lato-Bold
            FontManager.CacheWobbleFont(LatoBold, new WobbleFontStore(20,
                GameBase.Game.Resources.Get($"{folder}/Lato/{LatoBold}.ttf"), new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            // Lato-Light
            FontManager.CacheWobbleFont(LatoLight, new WobbleFontStore(20,
                GameBase.Game.Resources.Get($"{folder}/Lato/{LatoLight}.ttf"), new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            // Lato-Heavy
            FontManager.CacheWobbleFont(LatoHeavy, new WobbleFontStore(20,
                GameBase.Game.Resources.Get($"{folder}/Lato/{LatoHeavy}.ttf"), new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            var latoBlack = GameBase.Game.Resources.Get($"{folder}/Lato/{LatoBlack}.ttf");

            // Lato-Black
            FontManager.CacheWobbleFont(LatoBlack, new WobbleFontStore(20, latoBlack, new Dictionary<string, WobbleFontFace>
                {
                    {emojiString, new WobbleFontFace(emojiFont)},
                    {cjkString, notoCjkFont}
                }));

            var dir = $"{WobbleGame.WorkingDirectory}/Fonts";
            Directory.CreateDirectory(dir);

            // Copy over
            File.WriteAllBytes($"{dir}/lato-black.ttf", latoBlack);
        }
    }
}
