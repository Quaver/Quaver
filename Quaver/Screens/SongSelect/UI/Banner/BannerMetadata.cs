using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Quaver.API.Helpers;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Modifiers;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.SongSelect.UI.Banner
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

            Mode = new BannerMetadataItem(BitmapFonts.Exo2SemiBold, 13, "Mode", "4K") { Parent = this, Alignment = Alignment.MidLeft };
            Bpm = new BannerMetadataItem(BitmapFonts.Exo2SemiBold, 13, "BPM", "240") { Parent = this, Alignment = Alignment.MidLeft };
            Length = new BannerMetadataItem(BitmapFonts.Exo2SemiBold, 13, "Length", "0:00") { Parent = this, Alignment = Alignment.MidLeft };
            Difficulty = new BannerMetadataItem(BitmapFonts.Exo2SemiBold, 13, "Difficulty", "13.22") { Parent = this, Alignment = Alignment.MidLeft };

            Items = new List<BannerMetadataItem>
            {
                Mode,
                Bpm,
                Length,
                Difficulty
            };
        }

        /// <summary>
        ///     Updates the text of the metadata with a new map/mods
        ///     Realigns it so it's in the middle.
        /// </summary>
        /// <param name="map"></param>
        public void UpdateAndAlignMetadata(Map map)
        {
            Mode.UpdateValue(ModeHelper.ToShortHand(map.Mode));
            Bpm.UpdateValue((map.Bpm * ModHelper.GetRateFromMods(ModManager.Mods)).ToString(CultureInfo.InvariantCulture));
            Length.UpdateValue(TimeSpan.FromMilliseconds(map.SongLength * ModHelper.GetRateFromMods(ModManager.Mods)).ToString(@"mm\:ss"));
            Difficulty.UpdateValue("0.00");

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
    }
}