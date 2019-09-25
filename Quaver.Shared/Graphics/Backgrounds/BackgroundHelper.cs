/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public static class BackgroundHelper
    {
        /// <summary>
        ///     The current background that we're working with.
        /// </summary>
        public static BackgroundImage Background { get; private set; }

        /// <summary>
        ///     The currently loaded raw background texture.
        /// </summary>
        public static Texture2D RawTexture { get; private set; }

        /// <summary>
        ///     A cached version of the blurred texture
        /// </summary>
        public static Texture2D BlurredTexture { get; private set; }

        /// <summary>
        ///     The individual map this background is for.
        /// </summary>
        public static Map Map { get; private set; }

        /// <summary>
        ///     Dictates if we have a cached blurred texture of the current background
        /// </summary>
        private static bool ShouldBlur { get; set; }

        /// <summary>
        ///     Cancellation token to stop the existing background load tasks
        /// </summary>
        private static CancellationTokenSource Source { get; set; }

        /// <summary>
        ///     Banners to use throughout song select
        /// </summary>
        public static Dictionary<string, Texture2D> Banners { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        ///     A list of banners that are queued to be loaded on the load thread
        /// </summary>
        private static List<Mapset> MapsetBannersToLoad { get; } = new List<Mapset>();

        /// <summary>
        /// </summary>
        private static Texture2D DefaultBanner => UserInterface.DefaultBanner;

        /// <summary>
        ///     Event invoked when a new background has been loaded
        /// </summary>
        public static event EventHandler<BackgroundLoadedEventArgs> Loaded;

        /// <summary>
        ///     Event invoked when a new background has been blurred
        /// </summary>
        public static event EventHandler<BackgroundBlurredEventArgs> Blurred;

        /// <summary>
        ///     Event invoked when a new banner has loaded
        /// </summary>
        public static event EventHandler<BannerLoadedEventArgs> BannerLoaded;

        /// <summary>
        ///     Initializes the background helper for the entire game.
        /// </summary>
        public static void Initialize()
        {
            Background = new BackgroundImage(UserInterface.MenuBackground, 0, false);
            Source = new CancellationTokenSource();

            ThreadScheduler.Run(LoadBanners);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            Background?.Update(gameTime);
        }

        /// <summary>
        ///     Set per screen.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Draw(GameTime gameTime)
        {
            if (ShouldBlur)
            {
                try
                {
                    GameBase.Game.SpriteBatch.End();
                }
                catch (Exception e)
                {
                    // ignored
                }

                var blur = new GaussianBlur(0.3f);
                BlurredTexture = blur.PerformGaussianBlur(blur.PerformGaussianBlur(blur.PerformGaussianBlur(RawTexture)));
                ShouldBlur = false;
                Blurred?.Invoke(typeof(BackgroundHelper), new BackgroundBlurredEventArgs(Map, BlurredTexture));
            }

            Background?.Draw(gameTime);
        }

        /// <summary>
        ///     Queues a load of the background for a map
        /// </summary>
        public static void Load(Map map) => ThreadScheduler.Run(async () =>
        {
            try
            {
                Source.Cancel();
                Source.Dispose();
                Source = new CancellationTokenSource();

                Map = map;
                var token = Source.Token;

                token.ThrowIfCancellationRequested();

                var oldRawTexture = RawTexture;
                var oldBlurredTexture = BlurredTexture;

                var path = MapManager.GetBackgroundPath(map);

                var tex = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : UserInterface.MenuBackgroundRaw;
                RawTexture = tex;

                ThreadScheduler.RunAfter(() =>
                {
                    lock (TextureManager.Textures)
                    {
                        if (oldRawTexture != null && oldRawTexture != UserInterface.MenuBackgroundRaw)
                        {
                            oldRawTexture?.Dispose();
                            oldBlurredTexture?.Dispose();
                        }
                    }
                }, 500);

                token.ThrowIfCancellationRequested();

                await Task.Delay(100, token);
                ShouldBlur = true;
                Loaded?.Invoke(typeof(BackgroundHelper), new BackgroundLoadedEventArgs(map, tex));
            }
            catch (OperationCanceledException e)
            {
                // ignored
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        });

        /// <summary>
        ///     Loads all banners that are currently queued
        /// </summary>
        private static void LoadBanners()
        {
            while (true)
            {
                if (MapsetBannersToLoad.Count == 0)
                    continue;

                var bannersToRemove = new List<Mapset>();

                for (var i = 0; i < MapsetBannersToLoad.Count; i++)
                {
                    var mapset = MapsetBannersToLoad[i];

                    // Give custom banners first priority
                    var bannerExists = true;

                    var path = MapManager.GetBannerPath(mapset);

                    // Give map backgrounds second priority
                    if (!File.Exists(path))
                    {
                        path = MapManager.GetBackgroundPath(mapset.Maps.First());
                        bannerExists = false;
                    }

                    // Give the default banner last priority
                    var mapTexture = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : DefaultBanner;

                    // The banner is the default, so there's no need to cache it to a RenderTarget
                    if (mapTexture == DefaultBanner || bannerExists)
                    {
                        if (!Banners.ContainsKey(mapset.Directory))
                            Banners.Add(mapset.Directory, mapTexture);

                        BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, mapTexture));
                        bannersToRemove.Add(mapset);
                        continue;
                    }

                    // Mask the image and draw it to a RenderTarget
                    GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
                    {
                        var size = new ScalableVector2(421, 82);
                        var scrollContainer = new ScrollContainer(size, size);

                        var maskedSprite = new Sprite
                        {
                            Alignment = Alignment.MidCenter,
                            // Small 16:9 resolution size to make backgrounds look a bit better and zoomed out
                            Size = new ScalableVector2(1024, 576),
                            // This y offset usually captures the best part of the image (such as faces or text)
                            Y = 100,
                            Image = mapTexture
                        };

                        scrollContainer.AddContainedDrawable(maskedSprite);

                        // Only create a new RT if needed
                        var (pixelWidth, pixelHeight) = scrollContainer.AbsoluteSize * WindowManager.ScreenScale;

                        var renderTarget = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int) pixelWidth,
                            (int) pixelHeight, false,
                            GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

                        GameBase.Game.GraphicsDevice.SetRenderTarget(renderTarget);
                        GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                        scrollContainer.Draw(new GameTime());
                        GameBase.Game.SpriteBatch.End();

                        GameBase.Game.GraphicsDevice.SetRenderTarget(null);
                        scrollContainer?.Destroy();
                        maskedSprite?.Destroy();

                        if (!Banners.ContainsKey(mapset.Directory))
                            Banners.Add(mapset.Directory, renderTarget);

                        BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, renderTarget));
                    });

                    bannersToRemove.Add(mapset);
                }

                for (var i = 0; i < bannersToRemove.Count; i++)
                    MapsetBannersToLoad.Remove(bannersToRemove[i]);

                Thread.Sleep(16);
            }
        }
        /// <summary>
        ///     Loads a background banner to use during song select
        /// </summary>
        /// <param name="mapset"></param>
        public static void LoadBanner(Mapset mapset)
        {
            MapsetBannersToLoad.Add(mapset);
        }

        /// <summary>
        ///     Fades the background brightness all the way to black
        /// </summary>
        public static void FadeToBlack()
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, 1, 250));
        }

        /// <summary>
        ///     Unfades the background
        /// </summary>
        public static void FadeIn()
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, 0.65f, 250));
        }

        /// <summary>
        ///     Unfades the background to a specific alpha
        /// </summary>
        /// <param name="alpha"></param>
        public static void FadeIn(float alpha)
        {
            Background.BrightnessSprite.ClearAnimations();
            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Background.BrightnessSprite.Alpha, alpha, 250));
        }
    }
}
