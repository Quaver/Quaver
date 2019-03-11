/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Select.UI.Banner
{
    public class BannerMetadata : Sprite
    {
        /// <summary>
        ///     Referenece to the parent map banner
        /// </summary>
        private SelectMapBanner Banner { get; }

        /// <summary>
        ///     All the metadata displayed in the container.
        /// </summary>
        private List<BannerMetadataItem> Items { get; }

        /// <summary>
        ///     Displays the game mode.
        /// </summary>
        private BannerMetadataItem Mode { get; }

        /// <summary>
        ///     Displays the BPM of the map.
        /// </summary>
        private BannerMetadataItem Bpm { get; }

        /// <summary>
        ///     The length of the map.
        /// </summary>
        private BannerMetadataItem Length { get; }

        /// <summary>
        ///     The difficulty of the map.
        /// </summary>
        private BannerMetadataItem Difficulty { get; }

        /// <summary>
        ///     The percentage of long notes among the map's hit objects.
        /// </summary>
        private BannerMetadataItem LNPercentage { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="banner"></param>
        public BannerMetadata(SelectMapBanner banner)
        {
            Banner = banner;
            Parent = Banner;
            Size = new ScalableVector2(Banner.Width - Banner.Border.Thickness * 2, 34);
            Alignment = Alignment.BotCenter;
            Tint = Color.Black;
            Y = -Banner.Border.Thickness;
            Alpha = 0.45f;

            Mode = new BannerMetadataItem(Fonts.Exo2SemiBold, 13, "Mode", "4K") { Parent = this, Alignment = Alignment.MidLeft };
            Bpm = new BannerMetadataItem(Fonts.Exo2SemiBold, 13, "BPM", "240") { Parent = this, Alignment = Alignment.MidLeft };
            Length = new BannerMetadataItem(Fonts.Exo2SemiBold, 13, "Length", "0:00") { Parent = this, Alignment = Alignment.MidLeft };
            Difficulty = new BannerMetadataItem(Fonts.Exo2SemiBold, 13, "Difficulty", "13.22") { Parent = this, Alignment = Alignment.MidLeft };
            LNPercentage = new BannerMetadataItem(Fonts.Exo2SemiBold, 13, "LNs", "10%") { Parent = this, Alignment = Alignment.MidLeft };

            Items = new List<BannerMetadataItem>
            {
                Mode,
                Bpm,
                Length,
                Difficulty,
                LNPercentage
            };

            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Updates the text of the metadata with a new map/mods
        ///     Realigns it so it's in the middle.
        /// </summary>
        /// <param name="map"></param>
        public void UpdateAndAlignMetadata(Map map)
        {
            var length = TimeSpan.FromMilliseconds(map.SongLength / ModHelper.GetRateFromMods(ModManager.Mods));

            Mode.UpdateValue(ModeHelper.ToShortHand(map.Mode));
            Bpm.UpdateValue(((int)(map.Bpm * ModHelper.GetRateFromMods(ModManager.Mods))).ToString(CultureInfo.InvariantCulture));
            Length.UpdateValue(length.Hours > 0 ? length.ToString(@"hh\:mm\:ss") : length.ToString(@"mm\:ss"));
            Difficulty.UpdateValue(StringHelper.AccuracyToString((float) map.DifficultyFromMods(ModManager.Mods)).Replace("%", ""));
            LNPercentage.UpdateValue(((int) map.LNPercentage).ToString(CultureInfo.InvariantCulture) + "%");

            for (var i = 0; i < Items.Count; i++)
            {
                var metadata = Items[i];

                if (i == 0)
                {
                    metadata.X = 5;
                    continue;
                }

                var previous = Items[i - 1];
                metadata.X = previous.X + previous.Width + 5 + 5;
            }

            Items.ForEach(x => x.X += (Banner.Width - Items.Last().X) / Items.Count / 2);
        }

        /// <summary>
        ///     Called when game modifiers have changed.
        /// </summary>
        private void OnModsChanged(object o, ModsChangedEventArgs e) => UpdateAndAlignMetadata(MapManager.Selected.Value);
    }
}
