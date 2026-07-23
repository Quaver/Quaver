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
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Assets
{
    public static class Fonts
    {
        private const int NotoCjkWeight = FontWeight.SemiBold;

        // Temporary adjustment for testing lighter font rendering across Quaver.
        private const int FontWeightAdjustment = -100;
        private const int InterImplicitFontSizeReduction = 0;
        private const int InterDefaultSize = 20;
        private const string Folder = "Quaver.Resources/Fonts";
        private const string Emoji = "Emoji";
        private const string Cjk = "CJK";
        private const string Inter = "Inter";

        #region NEW_FONTS

        public static string InterRegular { get; } = "Inter-Regular";
        public static string InterMedium { get; } = "Inter-Medium";
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
                weight: AdjustFontWeight(NotoCjkWeight));

            var interFont = GameBase.Game.Resources.Get($"{Folder}/Inter/{Inter}.ttf");

            Dictionary<string, WobbleFontFace> CreateFallbacks() =>
                new Dictionary<string, WobbleFontFace>
                {
                    { Emoji, new WobbleFontFace(emojiFont) },
                    { Cjk, notoCjkFont }
                };

            void CacheFont(string name, WobbleFontStore font)
            {
                if (FontManager.WobbleFonts.ContainsKey(name))
                    return;

                FontManager.CacheWobbleFont(name, font);
            }

            void CacheInterFont(string name, int weight)
            {
                CacheFont(name, new WobbleFontStore(InterDefaultSize,
                    new WobbleFontFace(interFont, weight: AdjustFontWeight(weight),
                        enableTabularNumbers: true),
                    implicitFontSizeReduction: InterImplicitFontSizeReduction,
                    addedFonts: CreateFallbacks()));
            }

            CacheInterFont(InterRegular, FontWeight.Regular);
            CacheInterFont(InterMedium, FontWeight.Medium);
            CacheInterFont(InterSemiBold, FontWeight.SemiBold);
            CacheInterFont(InterBold, FontWeight.Bold);
            // CacheInterFont(InterLight, FontWeight.Light);
            // CacheInterFont(InterHeavy, FontWeight.ExtraBold);
            // CacheInterFont(InterBlack, FontWeight.Black);

            TooltipManager.Theme.Fonts = new Dictionary<int, WobbleFontStore>
            {
                { FontWeight.Regular, FontManager.GetWobbleFont(InterRegular) },
                // { FontWeight.Light, FontManager.GetWobbleFont(InterLight) },
                { FontWeight.Medium, FontManager.GetWobbleFont(InterMedium) },
                { FontWeight.SemiBold, FontManager.GetWobbleFont(InterSemiBold) },
                { FontWeight.Bold, FontManager.GetWobbleFont(InterBold) },
                // { FontWeight.ExtraBold, FontManager.GetWobbleFont(InterHeavy) },
                // { FontWeight.Black, FontManager.GetWobbleFont(InterBlack) }
            };

            var dir = $"{WobbleGame.WorkingDirectory}/Fonts";
            Directory.CreateDirectory(dir);

            // Copy over
            File.WriteAllBytes($"{dir}/inter.ttf", interFont);
            File.WriteAllBytes($"{dir}/noto-sans-cjk.ttc",
                GameBase.Game.Resources.Get($@"{Folder}/NotoCJK/NotoSansCJK-VF.ttf.ttc"));
            File.WriteAllBytes($"{dir}/lato-black.ttf", interFont);
        }

        public static void ReloadCjkFontFace(string cultureName)
        {
            var interFont = GameBase.Game.Resources.Get($"{Folder}/Inter/{Inter}.ttf");
            var fallbacks = CreateFallbacks(cultureName);

            ReloadInterFont(InterRegular, FontWeight.Regular, interFont, fallbacks);
            ReloadInterFont(InterMedium, FontWeight.Medium, interFont, fallbacks);
            ReloadInterFont(InterSemiBold, FontWeight.SemiBold, interFont, fallbacks);
            ReloadInterFont(InterBold, FontWeight.Bold, interFont, fallbacks);
            // ReloadInterFont(InterLight, FontWeight.Light, interFont, fallbacks);
            // ReloadInterFont(InterHeavy, FontWeight.ExtraBold, interFont, fallbacks);
            // ReloadInterFont(InterBlack, FontWeight.Black, interFont, fallbacks);
        }

        private static void ReloadInterFont(string name, int weight, byte[] interFont,
            Dictionary<string, WobbleFontFace> fallbacks)
        {
            if (!FontManager.WobbleFonts.TryGetValue(name, out var font))
                return;

            font.Reload(new WobbleFontFace(interFont, weight: AdjustFontWeight(weight)),
                InterImplicitFontSizeReduction,
                fallbacks);
        }

        private static Dictionary<string, WobbleFontFace> CreateFallbacks(string cultureName)
        {
            var emojiFont = GameBase.Game.Resources.Get($@"{Folder}/NotoColorEmoji/NotoColorEmoji.ttf");
            var notoCjkFont = new WobbleFontFace(
                GameBase.Game.Resources.Get($@"{Folder}/NotoCJK/NotoSansCJK-VF.ttf.ttc"),
                index: QuaverLocalization.GetNotoCjkFaceIndex(cultureName),
                weight: AdjustFontWeight(NotoCjkWeight));

            return new Dictionary<string, WobbleFontFace>
            {
                { Emoji, new WobbleFontFace(emojiFont) },
                { Cjk, notoCjkFont }
            };
        }

        private static int AdjustFontWeight(int weight) =>
            System.Math.Max(FontWeight.Thin, weight + FontWeightAdjustment);
    }
}
