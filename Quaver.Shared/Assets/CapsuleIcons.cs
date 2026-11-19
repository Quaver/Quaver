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
    public enum CapsuleIcon
    {
        BarChart,
        AreaChart,
        LineChart,
        Columns,
        Download,
        Music,
        Users,
        Refresh,
        Accuracy,
        GlobalRank,
        Disc,
        Minus,
        Speedometer,
        Trophy,
        DoubleUp,
        DoubleDown,
        Close,
        DownloadToTray,
        Sort
    }

    public static class CapsuleIcons
    {
        public const int IconHeight = 18;

        private const int SheetWidth = 486;

        private static Texture2D Sheet { get; set; }

        private static Dictionary<CapsuleIcon, TextureRegion> Textures { get; } = new Dictionary<CapsuleIcon, TextureRegion>();

        private static IReadOnlyDictionary<CapsuleIcon, Rectangle> Regions { get; } = new Dictionary<CapsuleIcon, Rectangle>
            {
                { CapsuleIcon.BarChart, new Rectangle(0, 0, 24, 18) },
                { CapsuleIcon.AreaChart, new Rectangle(30, 0, 24, 18) },
                { CapsuleIcon.LineChart, new Rectangle(60, 0, 24, 18) },
                { CapsuleIcon.Columns, new Rectangle(90, 0, 18, 18) },
                { CapsuleIcon.Download, new Rectangle(114, 0, 18, 18) },
                { CapsuleIcon.Music, new Rectangle(138, 0, 18, 18) },
                { CapsuleIcon.Users, new Rectangle(162, 0, 26, 18) },
                { CapsuleIcon.Refresh, new Rectangle(194, 0, 18, 18) },
                { CapsuleIcon.Accuracy, new Rectangle(218, 0, 18, 18) },
                { CapsuleIcon.GlobalRank, new Rectangle(242, 0, 23, 18) },
                { CapsuleIcon.Disc, new Rectangle(271, 0, 18, 18) },
                { CapsuleIcon.Minus, new Rectangle(295, 0, 20, 18) },
                { CapsuleIcon.Speedometer, new Rectangle(321, 0, 23, 18) },
                { CapsuleIcon.Trophy, new Rectangle(350, 0, 20, 18) },
                { CapsuleIcon.DoubleUp, new Rectangle(376, 0, 16, 18) },
                { CapsuleIcon.DoubleDown, new Rectangle(398, 0, 16, 18) },
                { CapsuleIcon.Close, new Rectangle(421, 0, 16, 18) },
                { CapsuleIcon.DownloadToTray, new Rectangle(444, 0, 18, 18) },
                { CapsuleIcon.Sort, new Rectangle(468, 0, 18, 18) }
            };

        public static TextureRegion Get(CapsuleIcon icon)
        {
            if (!Regions.ContainsKey(icon))
                throw new ArgumentOutOfRangeException(nameof(icon), icon, "The capsule icon is not mapped.");

            if (Sheet == null || Sheet.IsDisposed)
                throw new InvalidOperationException("The capsule icon grid has not been loaded.");

            if (Textures.TryGetValue(icon, out var texture))
                return texture;

            throw new InvalidOperationException($"The capsule icon {icon} was not loaded.");
        }

        public static Vector2 GetDisplaySize(CapsuleIcon icon, float height)
        {
            if (!Regions.TryGetValue(icon, out var region))
                throw new ArgumentOutOfRangeException(nameof(icon), icon, "The capsule icon is not mapped.");

            return new Vector2(region.Width * height / region.Height, height);
        }

        internal static void Load()
        {
            if (Sheet != null && !Sheet.IsDisposed && Textures.Count == Regions.Count)
                return;

            Textures.Clear();
            var sheet = UserInterface.CapsuleIconsGrid;
            if (sheet.Width != SheetWidth || sheet.Height != IconHeight)
            {
                throw new InvalidOperationException($"The capsule icon grid must be {SheetWidth}x{IconHeight}, but was {sheet.Width}x{sheet.Height}.");
            }

            try
            {
                foreach (var mapping in Regions)
                {
                    Textures.Add(mapping.Key, new TextureRegion(sheet, mapping.Value));
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

        internal static void Dispose()
        {
            Textures.Clear();
            Sheet = null;
        }
    }
}
