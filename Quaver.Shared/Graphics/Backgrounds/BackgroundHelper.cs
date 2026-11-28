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
using SkiaSharp;
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
        ///     The file path used to load <see cref="RawTexture"/>.
        /// </summary>
        private static string RawTexturePath { get; set; }

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
        private static List<MapsetBannerRequest> MapsetBannersToLoad { get; } = new List<MapsetBannerRequest>();

        /// <summary>
        ///     A list of playlist banners to load
        /// </summary>
        private static List<Playlist> PlaylistBannersToLoad { get; } = new List<Playlist>();

        /// <summary>
        /// </summary>
        private static Texture2D DefaultBanner => UserInterface.DefaultBanner;

        private const int BannerWidth = 421;

        private const int BannerHeight = 82;

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
                var path = GetLoadPath(map);

                if (RawTexture != null && RawTexturePath == path && !RawTexture.IsDisposed)
                {
                    Loaded?.Invoke(typeof(BackgroundHelper), new BackgroundLoadedEventArgs(map, RawTexture));
                    return;
                }

                var tex = File.Exists(path) ? AssetLoader.LoadTexture2DFromFile(path) : UserInterface.MenuBackgroundRaw;
                RawTexture = tex;
                RawTexturePath = path;

                token.ThrowIfCancellationRequested();

                Loaded?.Invoke(typeof(BackgroundHelper), new BackgroundLoadedEventArgs(map, tex));

                lock (TextureManager.Textures)
                {
                    if (oldRawTexture != null && oldRawTexture != UserInterface.MenuBackgroundRaw)
                    {
                        oldRawTexture?.Dispose();
                    }
                }
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
        ///     Gets the background path that <see cref="Load"/> will use for a map.
        /// </summary>
        public static string GetLoadPath(Map map)
        {
            if (map == null)
                return "";

            if (SkinManager.Skin != null && SkinManager.Skin.UseSkinBackgrounds && SkinManager.Skin.BackgroundPaths.Count != 0)
            {
                // string.GetHashCode() is not consistent across builds
                var hash = map.Md5Checksum.Aggregate(0, (hash, c) => (hash << 5) - hash + c);

                return SkinManager.Skin.BackgroundPaths[Math.Abs(hash) % SkinManager.Skin.BackgroundPaths.Count];
            }

            return MapManager.GetBackgroundPath(map);
        }

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
            LoadMapsetBanner(mapset, BannerWidth, BannerHeight, AddBanner);
        }

        /// <summary>
        ///     Loads a mapset banner using the requested dimensions
        /// </summary>
        public static void LoadMapsetBanner(Mapset mapset, int width, int height, Action<Texture2D, Mapset> loaded)
        {
            if (mapset == null)
                throw new ArgumentNullException(nameof(mapset));

            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));

            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            
            if (loaded == null)
                throw new ArgumentNullException(nameof(loaded));

            MapsetBannersToLoad.Add(new MapsetBannerRequest(mapset, width, height, loaded));
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

            var bannersToRemove = new List<MapsetBannerRequest>();

            for (var i = 0; i < MapsetBannersToLoad.Count; i++)
            {
                var request = MapsetBannersToLoad[i];
                var mapset = request.Mapset;

                var path = MapManager.GetMapsetBannerPath(mapset);

                // Give map backgrounds second priority
                if (!File.Exists(path))
                    path = mapset.Maps.Count > 0 ? MapManager.GetBackgroundPath(mapset.Maps.First()) : "";

                if (!File.Exists(path))
                {
                    request.Loaded(DefaultBanner, mapset);
                    bannersToRemove.Add(request);
                    continue;
                }

                CreateBanner(path, request.Width, request.Height, banner => request.Loaded(banner, mapset));
                bannersToRemove.Add(request);
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

                var extensions = new[] { ".png", ".jpg", ".jpg" };

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

                if (!File.Exists(path))
                {
                    AddBanner(DefaultBanner, playlist);
                    bannersToRemove.Add(playlist);
                    continue;
                }

                // Custom banners already have the final banner dimensions, so they can be used directly.
                if (bannerExists)
                {
                    AddBanner(LoadBannerTexture(path), playlist);
                    bannersToRemove.Add(playlist);
                    continue;
                }

                CreateBanner(path, playlist);
                bannersToRemove.Add(playlist);
            }

            for (var i = 0; i < bannersToRemove.Count; i++)
                PlaylistBannersToLoad.Remove(bannersToRemove[i]);
        }

        /// <summary>
        ///     Makes and creates a banner to use for song select/playlists
        /// </summary>
        /// <param name="path"></param>
        /// <param name="playlist"></param>
        private static void CreateBanner(string path, Playlist playlist) => 
            CreateBanner(path, BannerWidth, BannerHeight, banner => AddBanner(banner, playlist));

        private static void CreateBanner(string path, int width, int height, Action<Texture2D> addBanner)
        {
            var bannerData = CreateBannerData(path, width, height);

            if (bannerData == null)
            {
                addBanner(DefaultBanner);
                return;
            }

            GameBase.Game.ScheduleRenderTargetDraw(() =>
            {
                try
                {
                    addBanner(AssetLoader.LoadTexture2D(bannerData));
                }
                catch (Exception e)
                {
                    addBanner(DefaultBanner);
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }

        private static Texture2D LoadBannerTexture(string path)
        {
            try
            {
                return AssetLoader.LoadTexture2DFromFile(path);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return DefaultBanner;
            }
        }

        private static byte[] CreateBannerData(string path, int width, int height)
        {
            try
            {
                using (var source = SKImage.FromEncodedData(path))
                using (var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)))
                using (var paint = new SKPaint { IsAntialias = true })
                {
                    if (source == null || surface == null)
                        return null;

                    // Resize source banner to requested ratio if found
                    var targetRatio = width / (float) height;
                    var sourceRatio = source.Width / (float) source.Height;
                    SKRect sourceRectangle;

                    if (sourceRatio > targetRatio)
                    {
                        var cropWidth = source.Height * targetRatio;
                        var left = (source.Width - cropWidth) / 2f;
                        sourceRectangle = new SKRect(left, 0, left + cropWidth, source.Height);
                    }
                    else
                    {
                        var cropHeight = source.Width / targetRatio;
                        var top = (source.Height - cropHeight) / 2f;
                        sourceRectangle = new SKRect(0, top, source.Width, top + cropHeight);
                    }

                    surface.Canvas.Clear(SKColors.Transparent);
                    surface.Canvas.DrawImage(source, sourceRectangle, new SKRect(0, 0, width, height), new SKSamplingOptions(SKFilterMode.Linear), paint);
                    surface.Canvas.Flush();

                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        return data?.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return null;
            }
        }

        private static void AddBanner(Texture2D banner, Mapset mapset)
        {
            if (MapsetBanners.TryGetValue(mapset.Directory, out var existingBanner))
            {
                DisposeDuplicateBanner(banner, existingBanner);
                banner = existingBanner;
            }
            else
            {
                MapsetBanners.Add(mapset.Directory, banner);
            }

            BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(mapset, banner));
        }

        private static void AddBanner(Texture2D banner, Playlist playlist)
        {
            var playlistKey = playlist.Id.ToString();

            if (PlaylistBanners.TryGetValue(playlistKey, out var existingPlaylistBanner))
            {
                DisposeDuplicateBanner(banner, existingPlaylistBanner);
                banner = existingPlaylistBanner;
            }
            else
            {
                PlaylistBanners.Add(playlistKey, banner);
            }

            BannerLoaded?.Invoke(typeof(BackgroundHelper), new BannerLoadedEventArgs(playlist, banner));
        }

        private static void DisposeDuplicateBanner(Texture2D banner, Texture2D existingBanner)
        {
            if (banner != existingBanner && banner != DefaultBanner && !banner.IsDisposed)
                banner.Dispose();
        }

        /// <summary>
        ///     %apset banner request informations handled by the banner loader
        /// </summary>
        private sealed class MapsetBannerRequest
        {
            public Mapset Mapset { get; }
            public int Width { get; }
            public int Height { get; }
            public Action<Texture2D, Mapset> Loaded { get; }

            public MapsetBannerRequest(Mapset mapset, int width, int height, Action<Texture2D, Mapset> loaded)
            {
                Mapset = mapset;
                Width = width;
                Height = height;
                Loaded = loaded;
            }
        }

    }
}
