using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableMapsetBanner : Sprite
    {
        /// <summary>
        /// </summary>
        private DrawableMapset Mapset { get; set; }

        /// <summary>
        /// </summary>
        private static Texture2D DefaultBanner => UserInterface.DefaultBanner;

        /// <summary>
        ///     The amount of time since a new banner load was requested
        /// </summary>
        private double TimeSinceLoadRequested { get; set; }

        /// <summary>
        /// </summary>
        public bool HasBannerLoaded { get; private set; }

        /// <summary>
        /// </summary>
        public static float DeselectedAlpha { get; } = 0.75f;

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public DrawableMapsetBanner(DrawableMapset mapset)
        {
            Mapset = mapset;

            Image = DefaultBanner;
            BackgroundHelper.BannerLoaded += OnBannerLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceLoadRequested += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLoadRequested >= 250 && !HasBannerLoaded)
            {
                Alpha = 0;
                BackgroundHelper.LoadBanner(Mapset.Item);
                HasBannerLoaded = true;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Updates the banner contents
        /// </summary>
        /// <param name="mapset"></param>
        public void UpdateContent(DrawableMapset mapset)
        {
            Mapset = mapset;
            Alpha = 0;
            ClearAnimations();

            if (BackgroundHelper.Banners.ContainsKey(Mapset.Item.Directory))
            {
                Image = BackgroundHelper.Banners[Mapset.Item.Directory];
                Alpha = Mapset.IsSelected ? 1 : DeselectedAlpha;

                HasBannerLoaded = true;
                return;
            }

            HasBannerLoaded = false;
            TimeSinceLoadRequested = 0;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            BackgroundHelper.BannerLoaded -= OnBannerLoaded;

            base.Destroy();
        }

        /// <summary>
        ///     Fades in the image from 0 alpha
        /// </summary>
        private void FadeIn()
        {
            Alpha = 0;
            ClearAnimations();

            var alpha = Mapset.IsSelected ? 1 : DeselectedAlpha;
            FadeTo(alpha, Easing.OutQuint, 700);
        }

        /// <summary>
        ///     Called when a new banner has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBannerLoaded(object sender, BannerLoadedEventArgs e)
        {
            if (e.Mapset.Directory != Mapset.Item.Directory)
                return;

            Alpha = 0;
            Image = e.Banner;
            FadeIn();
            HasBannerLoaded = true;
        }
    }
}