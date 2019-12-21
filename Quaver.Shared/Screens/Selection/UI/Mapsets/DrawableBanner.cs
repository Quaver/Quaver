using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Logging;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class DrawableBanner : Sprite
    {
        /// <summary>
        /// </summary>
        public DrawableBannerType Type { get; }

        /// <summary>
        /// </summary>
        private DrawableMapset Mapset { get; set; }

        /// <summary>
        /// </summary>
        private Playlist Playlist { get; set; }

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
        public DrawableBanner(DrawableMapset mapset)
        {
            Type = DrawableBannerType.Mapsets;
            Mapset = mapset;

            Alpha = 0;
            Image = DefaultBanner;

            BackgroundHelper.BannerLoaded += OnBannerLoaded;
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <summary>
        /// </summary>
        /// <param name="playlist"></param>
        public DrawableBanner(Playlist playlist)
        {
            Type = DrawableBannerType.Playlists;
            Playlist = playlist;

            Alpha = 0;
            Image = DefaultBanner;

            BackgroundHelper.BannerLoaded += OnBannerLoaded;
            PlaylistManager.Selected.ValueChanged += OnPlaylistChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceLoadRequested += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceLoadRequested >= 200 && !HasBannerLoaded)
            {
                Alpha = 0;

                switch (Type)
                {
                    case DrawableBannerType.Mapsets:
                        Logger.Debug($"Loading banner for mapset: {Mapset.Item.Artist} - {Mapset.Item.Title}", LogType.Runtime, false);
                        BackgroundHelper.LoadMapsetBanner(Mapset.Item);
                        break;
                    case DrawableBannerType.Playlists:
                        Logger.Debug($"Loading banner for playlist: {Playlist.Id} - {Playlist.Name}", LogType.Runtime, false);
                        BackgroundHelper.LoadPlaylistBanner(Playlist);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

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

            // The map is already loaded, so just use it.
            if (BackgroundHelper.MapsetBanners.ContainsKey(Mapset.Item.Directory))
            {
                var tex = BackgroundHelper.MapsetBanners[Mapset.Item.Directory];
                HandleFade(tex);
                return;
            }

            MakeInvisible();
        }

        /// <summary>
        ///     Updates content for the playlist
        /// </summary>
        /// <param name="playlist"></param>
        public void UpdateContent(Playlist playlist)
        {
            Playlist = playlist;

            // The map is already loaded, so just use it.
            if (BackgroundHelper.PlaylistBanners.ContainsKey(Playlist.Id.ToString()))
            {
                var tex = BackgroundHelper.PlaylistBanners[Playlist.Id.ToString()];
                HandleFade(tex);
                return;
            }

            MakeInvisible();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            switch (Type)
            {
                case DrawableBannerType.Mapsets:
                    // ReSharper disable once DelegateSubtraction
                    break;
                case DrawableBannerType.Playlists:
                    // ReSharper disable once DelegateSubtraction
                    PlaylistManager.Selected.ValueChanged -= OnPlaylistChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            BackgroundHelper.BannerLoaded -= OnBannerLoaded;

            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="tex"></param>
        private void HandleFade(Texture2D tex)
        {
            var selected = Type == DrawableBannerType.Mapsets ? Mapset.IsSelected : PlaylistManager.Selected.Value == Playlist;

            if (Image != tex)
            {
                Image = tex;
                FadeTo(selected ? 1 : DeselectedAlpha, Easing.OutQuint, 700);
            }
            else
            {
                Image = tex;

                ClearAnimations();
                FadeTo(selected ? 1 : DeselectedAlpha, Easing.OutQuint, 700);
            }

            HasBannerLoaded = true;
            TimeSinceLoadRequested = 1000;
        }

        /// <summary>
        ///     Makes the banner completely invisible
        /// </summary>
        private void MakeInvisible()
        {
            Alpha = 0;
            ClearAnimations();

            HasBannerLoaded = false;
            TimeSinceLoadRequested = 0;
        }

        /// <summary>
        ///     Called when a new banner has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBannerLoaded(object sender, BannerLoadedEventArgs e)
        {
            if (e.Mapset != null)
            {
                if (e.Mapset?.Directory != Mapset?.Item?.Directory)
                    return;

                UpdateContent(Mapset);
            }
            else if (e.Playlist != null)
            {
                if (e.Playlist != Playlist)
                    return;

                UpdateContent(Playlist);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
            => UpdateContent(Mapset);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlaylistChanged(object sender, BindableValueChangedEventArgs<Playlist> e)
            => UpdateContent(Playlist);
    }

    public enum DrawableBannerType
    {
        Mapsets,
        Playlists
    }
}