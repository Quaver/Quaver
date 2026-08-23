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
using Wobble;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Assets
{
    /// <summary>
    ///     Contains the globally available icons in <see cref="UserInterface.IconsGrid"/>.
    ///     The coordinate-based names are temporary and can be renamed without changing their mappings.
    /// </summary>
    public enum GlobalIcon
    {
        Home,
        SinglePlayer,
        Multiplayer,
        Edit,
        Download,
        Workshop,
        Jukebox,
        Chat,
        Heart,
        Player,
        BurgerRedDot,
        Burger,
        Website,
        Discord,
        GitHub,
        
        Back,
        Play,
        Options,
        Volume,
        Recommend,
        Quit,
        Play2,
        Create,
        Quick,
        History,
        Ready,
        Music,
        CreatePlaylist,
        
        MoreOptions,
        LessOptions,
        Upload,
        ReverseSortAscending,
        ReverseSortDescending,
        ViewMap,
        OwnedMaps,
        QuestionMark,
        InProgress,
        Add,
        Reset,
        Remove,
        ViewProfile,
        LogIn,
        LogOut,
        Theatre,
        Clear,
        SelectAll,
        Random,
        Clan
    }

    /// <summary>
    ///     Loads and caches individual textures from the global icon grid.
    /// </summary>
    public static class GlobalIcons
    {
        public const int IconSize = 40;

        private const int ColumnStride = 80;

        private const int RowStride = 40;

        private static Texture2D? Sheet { get; set; }

        private static Dictionary<GlobalIcon, TextureRegion> Textures { get; } =
            new Dictionary<GlobalIcon, TextureRegion>();

        private static IReadOnlyDictionary<GlobalIcon, Point> Coordinates { get; } =
            new Dictionary<GlobalIcon, Point>
            {
                { GlobalIcon.Home, new Point(0, 0) },
                { GlobalIcon.SinglePlayer, new Point(0, 1) },
                { GlobalIcon.Multiplayer, new Point(0, 2) },
                { GlobalIcon.Edit, new Point(0, 3) },
                { GlobalIcon.Download, new Point(0, 4) },
                { GlobalIcon.Workshop, new Point(0, 5) },
                { GlobalIcon.Jukebox, new Point(0, 6) },
                { GlobalIcon.Chat, new Point(0, 7) },
                { GlobalIcon.Heart, new Point(0, 8) },
                { GlobalIcon.Player, new Point(0, 9) },
                { GlobalIcon.BurgerRedDot, new Point(0, 10) },
                { GlobalIcon.Burger, new Point(0, 11) },
                { GlobalIcon.Website, new Point(0, 12) },
                { GlobalIcon.Discord, new Point(0, 13) },
                { GlobalIcon.GitHub, new Point(0, 14) },
                
                { GlobalIcon.Back, new Point(1, 0) },
                { GlobalIcon.Play, new Point(1, 1) },
                { GlobalIcon.Options, new Point(1, 2) },
                { GlobalIcon.Volume, new Point(1, 3) },
                { GlobalIcon.Recommend, new Point(1, 4) },
                { GlobalIcon.Quit, new Point(1, 5) },
                { GlobalIcon.Play2, new Point(1, 6) },
                { GlobalIcon.Create, new Point(1, 7) },
                { GlobalIcon.Quick, new Point(1, 8) },
                { GlobalIcon.History, new Point(1, 9) },
                { GlobalIcon.Ready, new Point(1, 10) },
                { GlobalIcon.Music, new Point(1, 11) },
                { GlobalIcon.CreatePlaylist, new Point(1, 12) },
                
                { GlobalIcon.MoreOptions, new Point(2, 0) },
                { GlobalIcon.LessOptions, new Point(2, 1) },
                { GlobalIcon.Upload, new Point(2, 2) },
                { GlobalIcon.ReverseSortAscending, new Point(2, 3) },
                { GlobalIcon.ReverseSortDescending, new Point(2, 4) },
                { GlobalIcon.ViewMap, new Point(2, 5) },
                { GlobalIcon.OwnedMaps, new Point(2, 6) },
                { GlobalIcon.QuestionMark, new Point(2, 7) },
                { GlobalIcon.InProgress, new Point(2, 8) },
                { GlobalIcon.Add, new Point(2, 9) },
                { GlobalIcon.Reset, new Point(2, 10) },
                { GlobalIcon.Remove, new Point(2, 11) },
                { GlobalIcon.ViewProfile, new Point(2, 12) },
                { GlobalIcon.LogIn, new Point(2, 13) },
                { GlobalIcon.LogOut, new Point(2, 14) },
                { GlobalIcon.Theatre, new Point(2, 15) },
                { GlobalIcon.Clear, new Point(2, 16) },
                { GlobalIcon.SelectAll, new Point(2, 17) },
                { GlobalIcon.Random, new Point(2, 18) },
                { GlobalIcon.Clan, new Point(2, 19) }
            };

        /// <summary>
        ///     Gets a shared 40x40 atlas region for an icon in the global grid.
        /// </summary>
        /// <param name="icon">The icon to retrieve.</param>
        /// <returns>The globally owned icon atlas region.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The icon is not mapped.</exception>
        /// <exception cref="InvalidOperationException">The icon grid is unavailable or has invalid dimensions.</exception>
        public static TextureRegion Get(GlobalIcon icon)
        {
            if (!Coordinates.ContainsKey(icon))
                throw new ArgumentOutOfRangeException(nameof(icon), icon, "The global icon is not mapped.");

            var sheet = Sheet;

            if (sheet == null || sheet.IsDisposed)
                throw new InvalidOperationException("The global icon grid has not been loaded.");

            if (Textures.TryGetValue(icon, out var cachedTexture))
                return cachedTexture;

            throw new InvalidOperationException($"The global icon {icon} was not loaded.");
        }

        /// <summary>
        ///     Loads and validates the globally shared icon grid on the UI thread.
        /// </summary>
        internal static void Load()
        {
            if (Sheet != null && !Sheet.IsDisposed && Textures.Count == Coordinates.Count)
                return;

            var sheet = UserInterface.IconsGrid;

            if (sheet.Width % ColumnStride != 0 || sheet.Height % RowStride != 0)
            {
                throw new InvalidOperationException(
                    $"The global icon grid dimensions must be multiples of {ColumnStride}x{RowStride}, " +
                    $"but were {sheet.Width}x{sheet.Height}.");
            }

            try
            {
                foreach (var mapping in Coordinates)
                {
                    var sourceRectangle = CreateSourceRectangle(mapping.Value);
                    ValidateSourceRectangle(mapping.Key, sheet, sourceRectangle);

                    Textures.Add(mapping.Key, new TextureRegion(sheet, sourceRectangle));
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
        ///     Clears all icon atlas regions. The source sheet is owned by <c>TextureManager</c>.
        /// </summary>
        internal static void Dispose()
        {
            Textures.Clear();
            Sheet = null;
        }

        private static Rectangle CreateSourceRectangle(Point coordinate) => new Rectangle(
            coordinate.X * ColumnStride,
            coordinate.Y * RowStride,
            IconSize,
            IconSize);

        private static void ValidateSourceRectangle(GlobalIcon icon, Texture2D sheet, Rectangle sourceRectangle)
        {
            if (sourceRectangle.Left < 0 || sourceRectangle.Top < 0 ||
                sourceRectangle.Right > sheet.Width || sourceRectangle.Bottom > sheet.Height)
            {
                throw new InvalidOperationException(
                    $"The mapping for {icon} ({sourceRectangle}) is outside the " +
                    $"{sheet.Width}x{sheet.Height} global icon grid.");
            }
        }
    }
}
