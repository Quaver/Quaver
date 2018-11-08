using System;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
using Quaver.Modifiers;
using Quaver.Resources;
using Quaver.Scheduling;
using Quaver.Screens.Select;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Banner
{
    public class SelectMapBanner : Sprite
    {
        /// <summary>
        ///     Reference to the parent ScreenView.
        /// </summary>
        public SongSelectScreenView View { get; }

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
        private Sprite Brightness { get; }

        /// <summary>
        ///     The name of the difficulty of themap.
        /// </summary>
        private SpriteText MapDifficultyName { get; set; }

        /// <summary>
        ///     Displays the title of the song.
        /// </summary>
        private SpriteText SongTitle { get; set; }

        /// <summary>
        ///     Displays the artist of the song.
        /// </summary>
        private SpriteText SongArtist { get; set; }

        /// <summary>
        ///     Displays the creator of the map.
        /// </summary>
        private SpriteText MapCreator { get; set; }

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
        private SpriteText Mods { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public SelectMapBanner(SongSelectScreenView view)
        {
            View = view;
            Tint = Color.Black;

            Size = new ScalableVector2(620, 234);
            AddBorder(Colors.MainAccent, 3);

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
                Tint = Color.Black
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
            //LoadBanner(null);
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
        private void LoadBanner(Map previousMap)
        {
            if (previousMap != null && MapManager.GetBackgroundPath(previousMap) == MapManager.GetBackgroundPath(MapManager.Selected.Value))
                return;

            Border.Tint = Colors.MainAccent;

            lock (Border.Animations)
            {
                Border.Animations.Clear();
                Border.FadeToColor(Color.White, Easing.Linear, 500);
            }

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

            var previousTex = BannerSprite.Image;

            // Reference the map before running the load thread.
            var map = MapManager.Selected.Value;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    // Get rid of the old texture
                    if (previousTex != null && previousTex != UserInterface.MenuBackground)
                        previousTex.Dispose();

                    var tex = AssetLoader.LoadTexture2DFromFile(MapManager.GetBackgroundPath(map));

                    // If the map is still the same, perform an animation.
                    if (map == MapManager.Selected.Value)
                    {
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
                    // Otherwise skip it and dispose of the texture, as its not needed anymore.
                    else
                    {
                        if (tex != UserInterface.MenuBackground)
                            tex.Dispose();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        /// <summary>
        ///     Creates the SpriteText that displays the difficulty name of the map.
        /// </summary>
        private void CreateMapDifficultyName() => MapDifficultyName = new SpriteText(BitmapFonts.Exo2Bold, " ", 13)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = 22,
            Y = 50
        };

        /// <summary>
        ///    Creates the SpriteText that displays the title of the song.
        /// </summary>
        private void CreateSongTitle() => SongTitle = new SpriteText(BitmapFonts.Exo2Bold, " ", 14)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = MapDifficultyName.X,
            Y = MapDifficultyName.Y + MapDifficultyName.Height + 8
        };

        /// <summary>
        ///     Creates the text that displays the artist of the song.
        /// </summary>
        private void CreateSongArtist() => SongArtist = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 14)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = SongTitle.X,
            Y = SongTitle.Y + SongTitle.Height + 8
        };

        /// <summary>
        ///    Creates the text that displays the creator of the map.
        /// </summary>
        private void CreateMapCreator() => MapCreator = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 13)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = SongTitle.X,
            Y = SongArtist.Y + SongArtist.Height + 8
        };

        /// <summary>
        ///     Creates the sprite that displays the ranked status of the map.
        /// </summary>
        private void CreateRankedStatus() => RankedStatus = new BannerRankedStatus()
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Y = Border.Thickness,
            X = -Border.Thickness
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
        private void CreateMods() => Mods = new SpriteText(BitmapFonts.Exo2Bold, "Mods: " + ModHelper.GetModsString(ModManager.Mods), 12)
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            X = MapDifficultyName.X,
            Y = 10
        };

        /// <summary>
        ///     Updates the banner with a new map.
        /// </summary>
        /// <param name="map"></param>
        private void UpdateText(Map map)
        {
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