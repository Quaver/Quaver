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
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Assets
{
    /// <summary>
    ///     User-group badges available in the global user-group spritesheet.
    /// </summary>
    public enum GlobalUserGroup
    {
        Swan,
        Bot,
        AiAe,
        Developer,
        GraphicDesigner,
        Administrator,
        RankingSupervisor,
        TrialRankingSupervisor,
        HeadRankingSupervisor,
        Contributor,
        Moderator,
        Donator
    }

    /// <summary>
    ///     Loads and caches the small and full badges from the global user-group spritesheet.
    /// </summary>
    public static class UserGroupAssets
    {
        public const int SmallBadgeWidth = 35;

        public const int BadgeHeight = 22;

        private const int FullBadgeX = 40;

        private const int RowStride = 27;

        private const int LogicalSheetWidth = 237;

        private const int LogicalSheetHeight = 319;

        private static Texture2D? Sheet { get; set; }

        private static Dictionary<(GlobalUserGroup Group, bool Full), TextureRegion> Textures { get; } =
            new Dictionary<(GlobalUserGroup Group, bool Full), TextureRegion>();

        private static IReadOnlyDictionary<GlobalUserGroup, (int Row, int FullWidth)> Mappings { get; } =
            new Dictionary<GlobalUserGroup, (int Row, int FullWidth)>
            {
                { GlobalUserGroup.Swan, (0, 70) },
                { GlobalUserGroup.Bot, (1, 61) },
                { GlobalUserGroup.AiAe, (2, 72) },
                { GlobalUserGroup.Developer, (3, 104) },
                { GlobalUserGroup.GraphicDesigner, (4, 147) },
                { GlobalUserGroup.Administrator, (5, 129) },
                { GlobalUserGroup.RankingSupervisor, (6, 161) },
                { GlobalUserGroup.TrialRankingSupervisor, (7, 192) },
                { GlobalUserGroup.HeadRankingSupervisor, (8, 197) },
                { GlobalUserGroup.Contributor, (9, 110) },
                { GlobalUserGroup.Moderator, (10, 106) },
                { GlobalUserGroup.Donator, (11, 88) }
            };

        /// <summary>
        ///     Gets a shared user-group badge from the global atlas.
        /// </summary>
        /// <param name="group">The user group to retrieve.</param>
        /// <param name="full">Whether to retrieve the full badge. The small badge is returned by default.</param>
        /// <returns>The globally owned user-group atlas region.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The user group is not mapped.</exception>
        /// <exception cref="InvalidOperationException">The user-group spritesheet has not been loaded.</exception>
        public static TextureRegion Get(GlobalUserGroup group, bool full = false)
        {
            if (!Mappings.ContainsKey(group))
                throw new ArgumentOutOfRangeException(nameof(group), group, "The global user group is not mapped.");

            if (Textures.TryGetValue((group, full), out var texture))
                return texture;

            var sheet = Sheet;

            if (sheet == null || sheet.IsDisposed)
                throw new InvalidOperationException("The global user-group spritesheet has not been loaded.");

            throw new InvalidOperationException($"The global user-group badge {group} was not loaded.");
        }

        /// <summary>
        ///     Gets the logical on-screen size of a user-group badge.
        /// </summary>
        public static Point GetDisplaySize(GlobalUserGroup group, bool full = false)
        {
            if (!Mappings.TryGetValue(group, out var mapping))
                throw new ArgumentOutOfRangeException(nameof(group), group, "The global user group is not mapped.");

            return new Point(full ? mapping.FullWidth : SmallBadgeWidth, BadgeHeight);
        }

        /// <summary>
        ///     Loads and validates every user-group badge on the UI thread.
        /// </summary>
        internal static void Load()
        {
            if (Sheet != null && !Sheet.IsDisposed && Textures.Count == Mappings.Count * 2)
                return;

            Textures.Clear();
            Sheet = null;

            var sheet = UserInterface.UserGroups;
            var scale = GetSourceScale(sheet);

            try
            {
                foreach (var mapping in Mappings)
                {
                    foreach (var full in new[] { false, true })
                    {
                        var sourceRectangle = CreateSourceRectangle(mapping.Value, full, scale);
                        ValidateSourceRectangle(mapping.Key, full, sheet, sourceRectangle);
                        Textures.Add((mapping.Key, full), new TextureRegion(sheet, sourceRectangle));
                    }
                }

                Sheet = sheet;
            }
            catch
            {
                Textures.Clear();
                Sheet = null;
                throw;
            }
        }

        /// <summary>
        ///     Clears all user-group atlas regions. The source sheet is owned by <c>TextureManager</c>.
        /// </summary>
        internal static void Dispose()
        {
            Textures.Clear();
            Sheet = null;
        }

        private static Rectangle CreateSourceRectangle((int Row, int FullWidth) mapping, bool full, int scale) =>
            new Rectangle(
                (full ? FullBadgeX : 0) * scale,
                mapping.Row * RowStride * scale,
                (full ? mapping.FullWidth : SmallBadgeWidth) * scale,
                BadgeHeight * scale);

        private static int GetSourceScale(Texture2D sheet)
        {
            if (sheet.Width % LogicalSheetWidth != 0 || sheet.Height % LogicalSheetHeight != 0 ||
                sheet.Width / LogicalSheetWidth != sheet.Height / LogicalSheetHeight)
            {
                throw new InvalidOperationException(
                    $"The global user-group spritesheet must be a uniform integer scale of " +
                    $"{LogicalSheetWidth}x{LogicalSheetHeight}, but was {sheet.Width}x{sheet.Height}.");
            }

            return sheet.Width / LogicalSheetWidth;
        }

        private static void ValidateSourceRectangle(GlobalUserGroup group, bool full, Texture2D sheet,
            Rectangle sourceRectangle)
        {
            if (sourceRectangle.Left < 0 || sourceRectangle.Top < 0 ||
                sourceRectangle.Right > sheet.Width || sourceRectangle.Bottom > sheet.Height)
            {
                var variant = full ? "full" : "small";
                throw new InvalidOperationException(
                    $"The mapping for the {variant} {group} badge ({sourceRectangle}) is outside the " +
                    $"{sheet.Width}x{sheet.Height} global user-group spritesheet.");
            }
        }
    }
}
