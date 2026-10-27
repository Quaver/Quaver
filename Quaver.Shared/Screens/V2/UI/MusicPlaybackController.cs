using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects.Listening;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Music.Components;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class MusicPlaybackController
    {
        private Bindable<string> EmptySearchQuery { get; } = new Bindable<string>(string.Empty);

        internal Bindable<List<Mapset>> AvailableSongs { get; } = new Bindable<List<Mapset>>(new List<Mapset>());

        private MusicPlayerJukebox Jukebox { get; }

        private Random Random { get; } = new Random();

        private bool IsDestroyed { get; set; }

        private bool DefaultTrackRequested { get; set; }

        public MusicPlaybackController(Func<bool> isActive)
        {
            RefreshAvailableSongs();
            Jukebox = new MusicPlayerJukebox(AvailableSongs, isActive);

            ConfigManager.MusicPlayerOrderMapsBy.ValueChanged += OnOrderMapsByChanged;
            PlaylistManager.Selected.ValueChanged += OnPlaylistChanged;

            var track = AudioEngine.Track;
            if (track != null && !track.IsDisposed && track.IsPaused)
            {
                track.Play();
                track.Fade(100, 300);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsDestroyed)
                Jukebox.Update(gameTime);
        }

        public void TogglePlayback()
        {
            var track = AudioEngine.Track;
            if (track == null || track.IsDisposed || !CanControlListeningParty())
                return;

            lock (track)
            {
                if (track.IsPlaying)
                {
                    track.Pause();
                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.Pause);
                }
                else
                {
                    track.Play();
                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.Play);
                }
            }
        }

        public void Previous()
        {
            var track = AudioEngine.Track;
            if (track == null || track.IsDisposed || !CanControlListeningParty())
                return;

            lock (track)
            {
                if (track.Position >= 2000 || Jukebox.TrackListQueuePosition <= 0)
                {
                    AudioEngine.LoadCurrentTrack();
                    AudioEngine.Track?.Play();
                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.ChangeSong);
                    return;
                }

                Jukebox.SelectNextTrack(Direction.Backward);
            }
        }

        public void Next()
        {
            if (CanControlListeningParty())
                Jukebox.SelectNextTrack(Direction.Forward);
        }

        public void Select(Mapset mapset)
        {
            if (mapset?.Maps == null || mapset.Maps.Count == 0 || !CanControlListeningParty())
                return;

            var map = mapset.Maps.First();
            if (!ReferenceEquals(MapManager.Selected?.Value, map))
                MapManager.Selected.Value = map;
        }

        public List<Mapset> GetOrderedMapsets() => AvailableSongs.Value.ToList();

        public List<Mapset> GetQueueMapsets(int previousCount, int nextCount)
        {
            var songs = AvailableSongs.Value;
            var selected = MapManager.Selected?.Value;
            if (songs == null || songs.Count == 0)
                return new List<Mapset>();

            previousCount = Math.Max(0, previousCount);
            nextCount = Math.Max(0, nextCount);
            var count = Math.Min(songs.Count, previousCount + nextCount + 1);
            var currentIndex = selected == null ? -1 : songs.FindIndex(mapset => mapset.Maps.Contains(selected));
            if (currentIndex < 0)
                return songs.Take(count).ToList();

            var startIndex = Math.Max(0, currentIndex - previousCount);
            startIndex = Math.Min(startIndex, songs.Count - count);
            return songs.GetRange(startIndex, count);
        }

        public void EnsureDefaultTrack()
        {
            if (DefaultTrackRequested || AudioEngine.Track != null || MapManager.Mapsets.Count == 0)
                return;

            var mapset = MapManager.Mapsets[Random.Next(0, MapManager.Mapsets.Count)];
            if (mapset.Maps.Count == 0)
                return;

            DefaultTrackRequested = true;
            MapManager.Selected.Value = mapset.Maps[Random.Next(0, mapset.Maps.Count)];
        }

        public void Seek(double normalizedPosition)
        {
            var track = AudioEngine.Track;
            if (track == null || track.IsDisposed || !CanControlListeningParty())
                return;

            lock (track)
            {
                track.Seek(Math.Max(0, Math.Min(1, normalizedPosition)) * track.Length);
                OnlineManager.UpdateListeningPartyState(ListeningPartyAction.Seek);
            }
        }

        public void Destroy()
        {
            IsDestroyed = true;
            ConfigManager.MusicPlayerOrderMapsBy.ValueChanged -= OnOrderMapsByChanged;
            PlaylistManager.Selected.ValueChanged -= OnPlaylistChanged;
            Jukebox.Destroy();
        }

        private void RefreshAvailableSongs() => AvailableSongs.Value = MapsetHelper.FilterMapsets(EmptySearchQuery, true);

        private void OnOrderMapsByChanged(object sender, BindableValueChangedEventArgs<OrderMapsetsBy> args) =>
            RefreshAvailableSongs();

        private void OnPlaylistChanged(object sender, BindableValueChangedEventArgs<Playlist> args) =>
            RefreshAvailableSongs();

        private static bool CanControlListeningParty()
        {
            if (OnlineManager.IsListeningPartyHost)
                return true;

            NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Music_ListeningPartyHostRequired"));
            return false;
        }
    }
}
