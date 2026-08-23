/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Wobble;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Assets
{
    /// <summary>
    ///     The built-in judgement-window presets represented by the global judgement spritesheet.
    /// </summary>
    public enum GlobalJudgementWindow
    {
        Peaceful,
        Lenient,
        Chill,
        Standard,
        Strict,
        Tough,
        Extreme,
        Impossible,
        Custom
    }

    /// <summary>
    ///     Additional modifier badges that are not gameplay modifiers.
    /// </summary>
    public enum GlobalModBadge
    {
        More
    }

    /// <summary>
    ///     Loads and caches the shared judgement, modifier, and rate spritesheets.
    /// </summary>
    public static class GlobalGameplayAssets
    {
        private const int SpriteWidth = 60;
        private const int SpriteHeight = 24;
        private const int SpriteSpacing = 5;

        private const int RowStride = SpriteHeight + SpriteSpacing;

        private const int JudgementSpriteWidth = 66;
        private const int ModSpriteWidth = SpriteWidth;
        private const int ModColumnStride = SpriteWidth + SpriteSpacing;
        private const int RateSpriteWidth = SpriteWidth;

        private const int LogicalJudgementsSheetWidth = JudgementSpriteWidth;
        private const int LogicalJudgementsSheetHeight = 256;
        private const int LogicalModsSheetWidth = ModColumnStride + ModSpriteWidth;
        private const int LogicalModsSheetHeight = 459;
        private const int LogicalRatesSheetWidth = RateSpriteWidth;
        private const int LogicalRatesSheetHeight = 894;

        private static Texture2D? JudgementsSheet { get; set; }
        private static Texture2D? ModsSheet { get; set; }
        private static Texture2D? RatesSheet { get; set; }

        private static Dictionary<GlobalJudgementWindow, TextureRegion> JudgementTextures { get; } =
            new Dictionary<GlobalJudgementWindow, TextureRegion>();

        private static Dictionary<(ModIdentifier Mod, bool Inactive), TextureRegion> ModTextures { get; } =
            new Dictionary<(ModIdentifier Mod, bool Inactive), TextureRegion>();

        private static Dictionary<(GlobalModBadge Badge, bool Inactive), TextureRegion> ModBadgeTextures { get; } =
            new Dictionary<(GlobalModBadge Badge, bool Inactive), TextureRegion>();

        private static Dictionary<int, TextureRegion> RateTextures { get; } = new Dictionary<int, TextureRegion>();

        public static Point JudgementDisplaySize => new Point(JudgementSpriteWidth, SpriteHeight);

        public static Point ModDisplaySize => new Point(ModSpriteWidth, SpriteHeight);

        public static Point RateDisplaySize => new Point(RateSpriteWidth, SpriteHeight);

        private static IReadOnlyDictionary<GlobalJudgementWindow, Point> JudgementCoordinates { get; } =
            new Dictionary<GlobalJudgementWindow, Point>
            {
                { GlobalJudgementWindow.Peaceful, new Point(0, 0) },
                { GlobalJudgementWindow.Lenient, new Point(0, 1) },
                { GlobalJudgementWindow.Chill, new Point(0, 2) },
                { GlobalJudgementWindow.Standard, new Point(0, 3) },
                { GlobalJudgementWindow.Strict, new Point(0, 4) },
                { GlobalJudgementWindow.Tough, new Point(0, 5) },
                { GlobalJudgementWindow.Extreme, new Point(0, 6) },
                { GlobalJudgementWindow.Impossible, new Point(0, 7) },
                { GlobalJudgementWindow.Custom, new Point(0, 8) }
            };

        private static IReadOnlyDictionary<ModIdentifier, Point> ModCoordinates { get; } =
            new Dictionary<ModIdentifier, Point>
            {
                { ModIdentifier.Mirror, new Point(0, 0) },
                { ModIdentifier.Autoplay, new Point(0, 1) },
                { ModIdentifier.Coop, new Point(0, 2) },
                { ModIdentifier.NoFail, new Point(0, 3) },
                { ModIdentifier.NoSliderVelocity, new Point(0, 4) },
                { ModIdentifier.NoMiss, new Point(0, 5) },
                { ModIdentifier.NoMines, new Point(0, 6) },
                { ModIdentifier.NoLongNotes, new Point(0, 7) },
                { ModIdentifier.FullLN, new Point(0, 8) },
                { ModIdentifier.Inverse, new Point(0, 9) },
                { ModIdentifier.Randomize, new Point(0, 10) },
                { ModIdentifier.HeatlthAdjust, new Point(0, 11) },
                { ModIdentifier.NoPause, new Point(0, 12) },
                { ModIdentifier.Paused, new Point(0, 13) },
                { ModIdentifier.None, new Point(0, 14) }
            };

        private static IReadOnlyDictionary<GlobalModBadge, Point> ModBadgeCoordinates { get; } =
            new Dictionary<GlobalModBadge, Point>
            {
                { GlobalModBadge.More, new Point(0, 15) }
            };

        /// <summary>
        ///     Gets a shared judgement-window sprite.
        /// </summary>
        /// <param name="judgement">The preset to retrieve.</param>
        /// <returns>The globally owned judgement-window atlas region.</returns>
        public static TextureRegion GetJudgement(GlobalJudgementWindow judgement) =>
            GetTexture(JudgementTextures, JudgementsSheet, JudgementCoordinates, judgement,
                "global judgement-window");

        /// <summary>
        ///     Gets a shared modifier sprite.
        /// </summary>
        /// <param name="mod">The modifier to retrieve.</param>
        /// <param name="inactive">Whether to retrieve the inactive variant.</param>
        /// <returns>The globally owned modifier atlas region.</returns>
        public static TextureRegion GetMod(ModIdentifier mod, bool inactive = false)
        {
            if (!ModCoordinates.ContainsKey(mod))
                throw new ArgumentOutOfRangeException(nameof(mod), mod, "The global modifier is not mapped.");

            var key = (mod, inactive);
            if (ModTextures.TryGetValue(key, out var cachedTexture))
                return cachedTexture;

            var sheet = RequireSheet(ModsSheet, "global modifier");
            var coordinate = ModCoordinates[mod];
            var sourceRectangle = CreateSourceRectangle(coordinate, inactive ? ModColumnStride : 0, ModSpriteWidth);
            ValidateSourceRectangle(mod, sheet, sourceRectangle, "global modifier");

            throw new InvalidOperationException($"The global modifier {mod} was not loaded.");
        }

        /// <summary>
        ///     Gets a shared non-gameplay modifier badge.
        /// </summary>
        /// <param name="badge">The badge to retrieve.</param>
        /// <param name="inactive">Whether to retrieve the inactive variant.</param>
        /// <returns>The globally owned modifier badge atlas region.</returns>
        public static TextureRegion GetModBadge(GlobalModBadge badge, bool inactive = false)
        {
            if (!ModBadgeCoordinates.ContainsKey(badge))
                throw new ArgumentOutOfRangeException(nameof(badge), badge, "The global modifier badge is not mapped.");

            var key = (badge, inactive);
            if (ModBadgeTextures.TryGetValue(key, out var cachedTexture))
                return cachedTexture;

            var sheet = RequireSheet(ModsSheet, "global modifier badge");
            var sourceRectangle = CreateSourceRectangle(ModBadgeCoordinates[badge],
                inactive ? ModColumnStride : 0, ModSpriteWidth);
            ValidateSourceRectangle(badge, sheet, sourceRectangle, "global modifier badge");

            throw new InvalidOperationException($"The global modifier badge {badge} was not loaded.");
        }

        /// <summary>
        ///     Gets a shared rate sprite. Rates are supported from 0.50x through 2.00x in 0.05x steps.
        /// </summary>
        /// <param name="rate">The playback rate to retrieve.</param>
        /// <returns>The globally owned rate atlas region.</returns>
        public static TextureRegion GetRate(float rate)
        {
            var rateHundredths = (int)Math.Round(rate * 100, MidpointRounding.AwayFromZero);
            if (rateHundredths < 50 || rateHundredths > 200 || (rateHundredths - 50) % 5 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rate), rate,
                    "The global rate must be between 0.50x and 2.00x in 0.05x steps.");
            }

            if (RateTextures.TryGetValue(rateHundredths, out var cachedTexture))
                return cachedTexture;

            var sheet = RequireSheet(RatesSheet, "global rate");
            var coordinate = new Point(0, (rateHundredths - 50) / 5);
            var sourceRectangle = CreateSourceRectangle(coordinate, 0, RateSpriteWidth);
            ValidateSourceRectangle(rate, sheet, sourceRectangle, "global rate");

            throw new InvalidOperationException($"The global rate {rate:0.00}x was not loaded.");
        }

        /// <summary>
        ///     Loads and validates all shared gameplay spritesheets on the UI thread.
        /// </summary>
        internal static void Load()
        {
            if (JudgementsSheet != null && !JudgementsSheet.IsDisposed &&
                ModsSheet != null && !ModsSheet.IsDisposed &&
                RatesSheet != null && !RatesSheet.IsDisposed &&
                JudgementTextures.Count == JudgementCoordinates.Count &&
                ModTextures.Count == ModCoordinates.Count * 2 &&
                ModBadgeTextures.Count == ModBadgeCoordinates.Count * 2 &&
                RateTextures.Count == 31)
            {
                return;
            }

            var judgementsSheet = UserInterface.Judgements;
            var modsSheet = UserInterface.Mods;
            var ratesSheet = UserInterface.Rates;
            var judgementScale = GetSourceScale(judgementsSheet, LogicalJudgementsSheetWidth,
                LogicalJudgementsSheetHeight, "global judgement-window");
            var modScale = GetSourceScale(modsSheet, LogicalModsSheetWidth, LogicalModsSheetHeight,
                "global modifier");
            var rateScale = GetSourceScale(ratesSheet, LogicalRatesSheetWidth, LogicalRatesSheetHeight,
                "global rate");

            try
            {
                foreach (var mapping in JudgementCoordinates)
                {
                    var sourceRectangle = CreateSourceRectangle(mapping.Value, 0, JudgementSpriteWidth, judgementScale);
                    ValidateSourceRectangle(mapping.Key, judgementsSheet, sourceRectangle, "global judgement-window");
                    JudgementTextures.Add(mapping.Key, new TextureRegion(judgementsSheet, sourceRectangle));
                }

                foreach (var mapping in ModCoordinates)
                {
                    foreach (var inactive in new[] { false, true })
                    {
                        var sourceRectangle = CreateSourceRectangle(mapping.Value,
                            inactive ? ModColumnStride : 0, ModSpriteWidth, modScale);
                        ValidateSourceRectangle(mapping.Key, modsSheet, sourceRectangle, "global modifier");
                        ModTextures.Add((mapping.Key, inactive), new TextureRegion(modsSheet, sourceRectangle));
                    }
                }

                foreach (var mapping in ModBadgeCoordinates)
                {
                    foreach (var inactive in new[] { false, true })
                    {
                        var sourceRectangle = CreateSourceRectangle(mapping.Value,
                            inactive ? ModColumnStride : 0, ModSpriteWidth, modScale);
                        ValidateSourceRectangle(mapping.Key, modsSheet, sourceRectangle, "global modifier badge");
                        ModBadgeTextures.Add((mapping.Key, inactive), new TextureRegion(modsSheet, sourceRectangle));
                    }
                }

                for (var rateIndex = 0; rateIndex < 31; rateIndex++)
                {
                    var sourceRectangle = CreateSourceRectangle(new Point(0, rateIndex), 0, RateSpriteWidth, rateScale);
                    ValidateSourceRectangle(rateIndex, ratesSheet, sourceRectangle, "global rate");
                    RateTextures.Add(50 + rateIndex * 5, new TextureRegion(ratesSheet, sourceRectangle));
                }

                JudgementsSheet = judgementsSheet;
                ModsSheet = modsSheet;
                RatesSheet = ratesSheet;
            }
            catch
            {
                JudgementTextures.Clear();
                ModTextures.Clear();
                ModBadgeTextures.Clear();
                RateTextures.Clear();
                JudgementsSheet = null;
                ModsSheet = null;
                RatesSheet = null;
                throw;
            }
        }

        /// <summary>
        ///     Clears all gameplay atlas regions. The source sheets are owned by <c>TextureManager</c>.
        /// </summary>
        internal static void Dispose()
        {
            JudgementTextures.Clear();
            ModTextures.Clear();
            ModBadgeTextures.Clear();
            RateTextures.Clear();
            JudgementsSheet = null;
            ModsSheet = null;
            RatesSheet = null;
        }

        private static TextureRegion GetTexture<T>(
            IReadOnlyDictionary<T, TextureRegion> textures,
            Texture2D? sheet,
            IReadOnlyDictionary<T, Point> coordinates,
            T value,
            string sheetName) where T : notnull
        {
            if (!coordinates.ContainsKey(value))
                throw new ArgumentOutOfRangeException(nameof(value), value, $"The {sheetName} sprite is not mapped.");

            if (textures.TryGetValue(value, out var cachedTexture))
                return cachedTexture;

            RequireSheet(sheet, sheetName);
            throw new InvalidOperationException($"The {sheetName} sprite {value} was not loaded.");
        }

        private static Rectangle CreateSourceRectangle(Point coordinate, int xOffset, int width, int scale = 1) =>
            new Rectangle(
                (xOffset + coordinate.X * (width + SpriteSpacing)) * scale,
                coordinate.Y * RowStride * scale,
                width * scale,
                SpriteHeight * scale);

        private static int GetSourceScale(Texture2D sheet, int logicalWidth, int logicalHeight, string sheetName)
        {
            if (sheet.Width % logicalWidth != 0 || sheet.Height % logicalHeight != 0 ||
                sheet.Width / logicalWidth != sheet.Height / logicalHeight)
            {
                throw new InvalidOperationException(
                    $"The {sheetName} spritesheet must be a uniform integer scale of " +
                    $"{logicalWidth}x{logicalHeight}, but was {sheet.Width}x{sheet.Height}.");
            }

            return sheet.Width / logicalWidth;
        }

        private static Texture2D RequireSheet(Texture2D? sheet, string sheetName)
        {
            if (sheet == null || sheet.IsDisposed)
                throw new InvalidOperationException($"The {sheetName} spritesheet has not been loaded.");

            return sheet;
        }

        private static void ValidateSourceRectangle<T>(T value, Texture2D sheet, Rectangle sourceRectangle,
            string sheetName)
        {
            if (sourceRectangle.Left < 0 || sourceRectangle.Top < 0 ||
                sourceRectangle.Right > sheet.Width || sourceRectangle.Bottom > sheet.Height)
            {
                throw new InvalidOperationException(
                    $"The mapping for {value} ({sourceRectangle}) is outside the {sheet.Width}x{sheet.Height} " +
                    $"{sheetName} spritesheet.");
            }
        }

    }
}
