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
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Skinning;
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
        ///     The individual map this background is for.
        /// </summary>
        public static Map Map { get; private set; }

        /// <summary>
        ///     Cancellation token to stop the existing background load tasks
        /// </summary>
        private static CancellationTokenSource Source { get; set; }

        /// <summary>
        ///     Mapset Banners to use throughout song select
        /// </summary>
        public static Dictionary<string, Texture2D> MapsetBanners { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        ///     Playlist banners to use throughout song select
        /// </summary>
        public static Dictionary<string, Texture2D> PlaylistBanners { get; } = new Dictionary<string, Texture2D>();

        /// <summary>
        ///     A list of banners that are queued to be loaded on the load thread
        /// </summary>
        private static List<Mapset> MapsetBannersToLoad { get; } = new List<Mapset>();

        /// <summary>
        ///     A list of playlist banners to load
        /// </summary>
        private static List<Playlist> PlaylistBannersToLoad { get; } = new List<Playlist>();

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
        ///     Event invoked when a new mapset banner has loaded
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
        public static void Draw(GameTime gameTime) => Background?.Draw(gameTime);

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
                        }
                    }
                }, 500);

                token.ThrowIfCancellationRequested();

                await Task.Delay(100, token);
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
                Thread.Sleep(100);

                if (MapsetBannersToLoad.Count == 0 && PlaylistBannersToLoad.Count == 0)
                    continue;

                LoadAllMapsetBanners();
                LoadAllPlaylistBanners();
            }
        }


        /// <summary>
        ///     Loads a background banner to use during song select
        /// </summary>
        /// <param name="mapset"></param>
        public static void LoadMapsetBanner(Mapset mapset)
        {
            MapsetBannersToLoad.Add(mapset);
        }

        /// <summary>
        ///     Loads a playlist banner into the game
        /// </summary>
        /// <param name="playlist"></param>
        public static void LoadPlaylistBanner(Playlist playlist)
        {
            PlaylistBannersToLoad.Add(playlist);
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

        /// <summary>
        ///     Responsible for making sure all mapset banners are loaded in
        /// </summary>
        private static void LoadAllMapsetBanners()
        {
            if (MapsetBannersToLoad.Count == 0)
                return;

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

                Texture2D mapTexture;

                try
                {
                    mapTexture = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : DefaultBanner;
                }
                catch (Exception e)
                {
                    mapTexture = DefaultBanner;
                    Logger.Error(e, LogType.Runtime);
                }

                // The banner is the default, so there's no need to cache it to a RenderTarget
                if (mapTexture == DefaultBanner || bannerExists)
                {
                    if (!MapsetBanners.ContainsKey(mapset.Directory))
                        MapsetBanners.Add(mapset.Directory, mapTexture);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, mapTexture));
                    bannersToRemove.Add(mapset);
                    continue;
                }

                CreateBanner(mapTexture, mapset);

                bannersToRemove.Add(mapset);
            }

            for (var i = 0; i < bannersToRemove.Count; i++)
                MapsetBannersToLoad.Remove(bannersToRemove[i]);
        }

        /// <summary>
        ///     Responsible for loading all playlist banners
        /// </summary>
        private static void LoadAllPlaylistBanners()
        {
            if (PlaylistBannersToLoad.Count == 0)
                return;

            var bannersToRemove = new List<Playlist>();

            for (var i = 0; i < PlaylistBannersToLoad.Count; i++)
            {
                var playlist = PlaylistBannersToLoad[i];

                // Give custom banners first priority
                var bannerExists = true;

                var extensions = new[] {".png", ".jpg", ".jpg"};

                var path = "";

                foreach (var ext in extensions)
                {
                    path = $"{ConfigManager.DataDirectory.Value}/playlists/{playlist.Id}{ext}";

                    if (File.Exists(path))
                        break;
                }

                // Give map backgrounds second priority
                if (playlist.PlaylistGame == MapGame.Etterna || !File.Exists(path))
                {
                    if (playlist.Maps.Count != 0)
                    {
                        path = MapManager.GetBackgroundPath(playlist.Maps.First()).Replace("//", "/");
                        bannerExists = false;
                    }
                }

                Texture2D mapTexture;

                try
                {
                    mapTexture = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : DefaultBanner;
                }
                catch (Exception e)
                {
                    mapTexture = DefaultBanner;
                    Logger.Error(e, LogType.Runtime);
                }

                // The banner is the default, so there's no need to cache it to a RenderTarget
                if (mapTexture == DefaultBanner || bannerExists)
                {
                    if (!PlaylistBanners.ContainsKey(playlist.Id.ToString()))
                        PlaylistBanners.Add(playlist.Id.ToString(), mapTexture);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(playlist, mapTexture));
                    bannersToRemove.Add(playlist);
                    continue;
                }

                CreateBanner(mapTexture, null, playlist);
                bannersToRemove.Add(playlist);
            }

            for (var i = 0; i < bannersToRemove.Count; i++)
                PlaylistBannersToLoad.Remove(bannersToRemove[i]);
        }

        /// <summary>
        ///     Masks a banner to use
        /// </summary>
        /// <param name="mapTexture"></param>
        /// <param name="mapset"></param>
        private static void CreateBanner(Texture2D mapTexture, Mapset mapset = null, Playlist playlist = null)
        {
            if (mapset == null && playlist == null || mapset != null && playlist != null)
                throw new InvalidOperationException();

            // Mask the image and draw it to a RenderTarget
            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                // Create a RenderTarget with mipmapping with the original texuture.
                var mipmapped = new RenderTarget2D(GameBase.Game.GraphicsDevice, mapTexture.Width, mapTexture.Height, true,
                    SurfaceFormat.Bgr565, DepthFormat.Depth24Stencil8);

                GameBase.Game.GraphicsDevice.SetRenderTarget(mipmapped);
                GameBase.Game.GraphicsDevice.Clear(Color.Transparent);

                var textureSprite = new Sprite
                {
                    Size = new ScalableVector2(mapTexture.Width, mapTexture.Height),
                    Image = mapTexture,
                    SpriteBatchOptions = new SpriteBatchOptions()
                    {
                        DoNotScale = true,
                        SamplerState = SamplerState.PointClamp
                    }
                };

                textureSprite.Draw(new GameTime());
                GameBase.Game.SpriteBatch.End();

                // Cache a smaller version of the texture to a RenderTarget
                var size = SkinManager.Skin?.SongSelect?.MapsetPanelBannerSize ?? new ScalableVector2(421, 82);
                var scrollContainer = new ScrollContainer(size, size);

                var maskedSprite = new Sprite
                {
                    Alignment = Alignment.MidCenter,
                    // Small 16:9 resolution size to make backgrounds look a bit better and zoomed out
                    Size = new ScalableVector2(448, 252),
                    Image = mipmapped
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

                textureSprite.Destroy();
                mipmapped?.Dispose();
                mapTexture.Dispose();

                if (mapset != null)
                {
                    if (!MapsetBanners.ContainsKey(mapset.Directory))
                        MapsetBanners.Add(mapset.Directory, renderTarget);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, renderTarget));
                }
                else if (playlist != null)
                {
                    if (!PlaylistBanners.ContainsKey(playlist.Id.ToString()))
                        PlaylistBanners.Add(playlist.Id.ToString(), renderTarget);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(playlist, renderTarget));
                }
            });
        }
    }
}
