/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Select.UI.Banner
{
    public class SelectMapBanner : Sprite
    {
        /// <summary>
        ///     Reference to the parent ScreenView.
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     The mask for the banner.
        /// </summary>
        private SpriteMaskContainer Mask { get; }

        /// <summary>
        ///     The sprite that displays the actual banner.
        /// </summary>
        private Sprite BannerSprite { get; }

        /// <summary>
        ///     A dark layer over the banner that provides some darkness.
        /// </summary>
        public Sprite Brightness { get; }

        /// <summary>
        ///     The name of the difficulty of themap.
        /// </summary>
        private SpriteTextBitmap MapDifficultyName { get; set; }

        /// <summary>
        ///     Displays the title of the song.
        /// </summary>
        private SpriteTextBitmap SongTitle { get; set; }

        /// <summary>
        ///     Displays the artist of the song.
        /// </summary>
        private SpriteTextBitmap SongArtist { get; set; }

        /// <summary>
        ///     Displays the creator of the map.
        /// </summary>
        private SpriteTextBitmap MapCreator { get; set; }

        /// <summary>
        ///     The flag that shows the ranked status of the map.
        /// </summary>
        public BannerRankedStatus RankedStatus { get; private set; }

        /// <summary>
        ///     Displays all the metadata for the map.
        /// </summary>
        private BannerMetadata Metadata { get; }

        /// <summary>
        ///     Displays the currently activated mods.
        /// </summary>
        private SpriteTextBitmap Mods { get; set; }

        /// <summary>
        ///     The container in which the metadata will be housed in.
        /// </summary>
        private ScrollContainer Container { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public SelectMapBanner(SelectScreenView view)
        {
            View = view;
            Tint = Color.Black;

            Size = new ScalableVector2(620, 234);
            AddBorder(ColorHelper.HexToColor("#69acc5"), 2);

            Mask = new SpriteMaskContainer()
            {
                Parent = this,
                Size = new ScalableVector2(Width - Border.Thickness * 2, Height - Border.Thickness * 2),
                Alignment = Alignment.MidCenter,
                Tint = Color.Black
            };

            BannerSprite = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(WindowManager.Width / 1.6f, WindowManager.Height / 1.6f),
                Alignment = Alignment.MidCenter,
                Y = -80
            };

            Mask.AddContainedSprite(BannerSprite);

            Brightness = new Sprite()
            {
                Parent = this,
                Size = Mask.Size,
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 1
            };

            Container = new ScrollContainer(Size, Size)
            {
                Parent = this,
                Alpha = 0
            };

            CreateMapDifficultyName();
            CreateSongTitle();
            CreateSongArtist();
            CreateMapCreator();
            CreateRankedStatus();
            CreateMods();
            Metadata = new BannerMetadata(this);

            MapManager.Selected.ValueChanged += OnMapChange;
            ModManager.ModsChanged += OnModsChanged;
            LoadBanner(null);
            UpdateText(MapManager.Selected.Value);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChange;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Loads the banner for the currently selected map.
        /// </summary>
        public void LoadBanner(Texture2D tex)
        {
            // Start the fadeout on the background.
            lock (BannerSprite.Animations)
            {
                BannerSprite.Animations.Clear();
                BannerSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, BannerSprite.Alpha, 0, 500));
            }

            lock (Brightness.Animations)
            {
                Brightness.Animations.Clear();
                Brightness.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Brightness.Alpha, 1, 300));
            }

            BannerSprite.Image = tex;

            // Fade in bg
            lock (BannerSprite.Animations)
            {
                BannerSprite.Animations.Clear();

                BannerSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint,
                    BannerSprite.Alpha, 1, 1000));
            }

            lock (Brightness.Animations)
            {
                Brightness.Animations.Clear();
                Brightness.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Brightness.Alpha, 0.45f, 300));
            }
        }

        /// <summary>
        ///     Creates the SpriteTextBitmap that displays the difficulty name of the map.
        /// </summary>
        private void CreateMapDifficultyName()
        {
            MapDifficultyName = new SpriteTextBitmap(FontsBitmap.GothamBold, " ")
            {
                Alignment = Alignment.TopLeft,
                X = 22,
                Y = 50,
                FontSize = 18
            };

            Container.AddContainedDrawable(MapDifficultyName);
        }

        /// <summary>
        ///    Creates the SpriteTextBitmap that displays the title of the song.
        /// </summary>
        private void CreateSongTitle()
        {
            SongTitle = new SpriteTextBitmap(FontsBitmap.GothamBold, " ")
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = MapDifficultyName.X,
                Y = MapDifficultyName.Y + MapDifficultyName.Height + 15,
                FontSize = 16,
            };

            Container.AddContainedDrawable(SongTitle);
        }

        /// <summary>
        ///     Creates the text that displays the artist of the song.
        /// </summary>
        private void CreateSongArtist()
        {
            SongArtist = new SpriteTextBitmap(FontsBitmap.GothamBold, " ")
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = SongTitle.X,
                Y = SongTitle.Y + SongTitle.Height + 15,
                FontSize = 16
            };

            Container.AddContainedDrawable(SongArtist);
        }

        /// <summary>
        ///    Creates the text that displays the creator of the map.
        /// </summary>
        private void CreateMapCreator()
        {
            MapCreator = new SpriteTextBitmap(FontsBitmap.GothamBold, " ")
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = SongTitle.X,
                Y = SongArtist.Y + SongArtist.Height + 15,
                FontSize = 16
            };

            Container.AddContainedDrawable(MapCreator);
        }

        /// <summary>
        ///     Creates the sprite that displays the ranked status of the map.
        /// </summary>
        private void CreateRankedStatus() => RankedStatus = new BannerRankedStatus()
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Y = 5,
            X = -8
        };

        /// <summary>
        ///     Called when the selected map is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e)
        {
            // LoadBanner(e.OldValue);
            UpdateText(e.Value);
        }

        /// <summary>
        ///     Creates the text that displays the mods.
        /// </summary>
        private void CreateMods() => Mods = new SpriteTextBitmap(FontsBitmap.GothamBold, "Mods: " + ModHelper.GetModsString(ModManager.Mods))
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = MapDifficultyName.X,
            Y = 15,
            FontSize = 16
        };

        /// <summary>
        ///     Updates the banner with a new map.
        /// </summary>
        /// <param name="map"></param>
        private void UpdateText(Map map)
        {
            if (map == null)
                return;

            MapDifficultyName.Text = $"\"{map.DifficultyName}\"";
            SongTitle.Text = map.Title;
            SongArtist.Text = map.Artist;
            MapCreator.Text = $"Created by: {map.Creator}";
            RankedStatus.UpdateMap(map);
            Metadata.UpdateAndAlignMetadata(map);
        }

        /// <summary>
        ///     Called when the activated mods change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => Mods.Text = $"Mods: {ModHelper.GetModsString(e.Mods)}";
    }
}
