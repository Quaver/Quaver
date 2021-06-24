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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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

                var path = MapManager.GetMapsetBannerPath(mapset);

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

                CreateBanner(path, mapset);

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

                CreateBanner(path, null, playlist);
                bannersToRemove.Add(playlist);
            }

            for (var i = 0; i < bannersToRemove.Count; i++)
                PlaylistBannersToLoad.Remove(bannersToRemove[i]);
        }

        /// <summary>
        ///     Makes and creates a banner to use for song select/playlists
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mapset"></param>
        /// <param name="playlist"></param>
        private static void CreateBanner(string path, Mapset mapset = null, Playlist playlist = null)
        {
            if (mapset == null && playlist == null || mapset != null && playlist != null)
                throw new InvalidOperationException();

            using (var outStream = new MemoryStream())
            using (var image = Image.Load(File.OpenRead(path), out var format))
            {
                image.Mutate(i => i.Resize(448, 252).Crop(new SixLabors.ImageSharp.Rectangle(0, 20, 421, 82)));
                image.Save(outStream, format);

                var img = Texture2D.FromStream(GameBase.Game.GraphicsDevice, outStream);

                if (mapset != null)
                {
                    if (!MapsetBanners.ContainsKey(mapset.Directory))
                        MapsetBanners.Add(mapset.Directory, img);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, img));
                }
                else
                {
                    if (!PlaylistBanners.ContainsKey(playlist.Id.ToString()))
                        PlaylistBanners.Add(playlist.Id.ToString(), img);

                    BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(playlist, img));
                }
            }
        }
    }
}
