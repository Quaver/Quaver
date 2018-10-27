using System;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Backgrounds;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public SelectMapBanner(SongSelectScreenView view)
        {
            View = view;
            Tint = Color.Black;

            Size = new ScalableVector2(518, 234);
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

            MapManager.Selected.ValueChanged += OnMapChange;
            LoadBanner(null);
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

            var previousTex = BannerSprite.Image;

            // Reference the map before running the load thread.
            var map = MapManager.Selected.Value;

            Scheduler.RunThread(() =>
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
        ///     Called when the selected map is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e) => LoadBanner(e.OldValue);
    }
}