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
        private const int InterImplicitFontSizeReduction = 4;
        private const int InterDefaultSize = 20;
        private const string Folder = "Quaver.Resources/Fonts";
        private const string Emoji = "Emoji";
        private const string Cjk = "CJK";
        private const string Inter = "Inter";

        public static string Exo2Bold { get; } = "exo2-bold";
        public static string Exo2BoldItalic { get; } = "exo2-bolditalic";
        public static string Exo2Medium { get; } = "exo2-medium";
        public static string Exo2Regular { get; } = "exo2-regular";
        public static string Exo2SemiBold { get; } = "exo2-semibold";

        #region NEW_FONTS

        public static string InterRegular { get; } = "Inter-Regular";
        public static string InterSemiBold { get; } = "Inter-Semibold";
        public static string InterBold { get; } = "Inter-Bold";
        public static string InterLight { get; } = "Inter-Light";
        public static string InterHeavy { get; } = "Inter-Heavy";
        public static string InterBlack { get; } = "Inter-Black";

        #endregion

        /// <summary>
        /// </summary>
        public static void LoadWobbleFonts()
        {
            // Load fallback fonts or fonts that are used across multiple WobbleFonts
            var emojiFont =
                GameBase.Game.Resources.Get($@"{Folder}/NotoColorEmoji/NotoColorEmoji.ttf");

            var notoCjkFont = new WobbleFontFace(
                GameBase.Game.Resources.Get($@"{Folder}/NotoCJK/NotoSansCJK-VF.ttf.ttc"),
                index: QuaverLocalization.GetNotoCjkFaceIndex(ConfigManager.Language.Value),
                weight: NotoCjkWeight);

            var interFont = GameBase.Game.Resources.Get($"{Folder}/Inter/{Inter}.ttf");

            Dictionary<string, WobbleFontFace> CreateFallbacks() =>
                new Dictionary<string, WobbleFontFace>
                {
                    { Emoji, new WobbleFontFace(emojiFont) },
                    { Cjk, notoCjkFont }
                };

            void CacheInterFont(string name, int weight)
            {
                FontManager.CacheWobbleFont(name, new WobbleFontStore(InterDefaultSize,
                    new WobbleFontFace(interFont, weight: weight),
                    implicitFontSizeReduction: InterImplicitFontSizeReduction,
                    addedFonts: CreateFallbacks()));
            }

            CacheInterFont(InterRegular, FontWeight.Regular);
            CacheInterFont(InterSemiBold, FontWeight.SemiBold);
            CacheInterFont(InterBold, FontWeight.Bold);
            CacheInterFont(InterLight, FontWeight.Light);
            CacheInterFont(InterHeavy, FontWeight.ExtraBold);
            CacheInterFont(InterBlack, FontWeight.Black);

            var dir = $"{WobbleGame.WorkingDirectory}/Fonts";
            Directory.CreateDirectory(dir);

            // Copy over
            File.WriteAllBytes($"{dir}/inter.ttf", interFont);
            File.WriteAllBytes($"{dir}/lato-black.ttf", interFont);
        }

        public static void ReloadCjkFontFace(string cultureName)
        {
            var interFont = GameBase.Game.Resources.Get($"{Folder}/Inter/{Inter}.ttf");
            var fallbacks = CreateFallbacks(cultureName);

            ReloadInterFont(InterRegular, FontWeight.Regular, interFont, fallbacks);
            ReloadInterFont(InterSemiBold, FontWeight.SemiBold, interFont, fallbacks);
            ReloadInterFont(InterBold, FontWeight.Bold, interFont, fallbacks);
            ReloadInterFont(InterLight, FontWeight.Light, interFont, fallbacks);
            ReloadInterFont(InterHeavy, FontWeight.ExtraBold, interFont, fallbacks);
            ReloadInterFont(InterBlack, FontWeight.Black, interFont, fallbacks);
        }

        private static void ReloadInterFont(string name, int weight, byte[] interFont,
            Dictionary<string, WobbleFontFace> fallbacks)
        {
            if (!FontManager.WobbleFonts.TryGetValue(name, out var font))
                return;

            font.Reload(new WobbleFontFace(interFont, weight: weight),
                InterImplicitFontSizeReduction,
                fallbacks);
        }

        private static Dictionary<string, WobbleFontFace> CreateFallbacks(string cultureName)
        {
            var emojiFont = GameBase.Game.Resources.Get($@"{Folder}/NotoColorEmoji/NotoColorEmoji.ttf");
            var notoCjkFont = new WobbleFontFace(
                GameBase.Game.Resources.Get($@"{Folder}/NotoCJK/NotoSansCJK-VF.ttf.ttc"),
                index: QuaverLocalization.GetNotoCjkFaceIndex(cultureName),
                weight: NotoCjkWeight);

            return new Dictionary<string, WobbleFontFace>
            {
                { Emoji, new WobbleFontFace(emojiFont) },
                { Cjk, notoCjkFont }
            };
        }
    }
}
