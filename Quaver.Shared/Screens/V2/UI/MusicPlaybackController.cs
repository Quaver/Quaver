using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Server.Client.Objects.Listening;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Bindables;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class MusicPlaybackController
    {
        private List<Map> TrackQueue { get; } = new List<Map>();

        private Random Random { get; } = new Random();

        private int QueuePosition { get; set; } = -1;

        private bool ChangingSelection { get; set; }

        private bool LoadingTrack { get; set; }

        private bool IsDestroyed { get; set; }

        public MusicPlaybackController()
        {
            AddSelectedMapToQueue(MapManager.Selected?.Value);
            MapManager.Selected.ValueChanged += OnSelectedMapChanged;
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

            if (track.Position >= 2000 || QueuePosition <= 0)
            {
                LoadSelectedTrack();
                return;
            }

            QueuePosition--;
            SetSelectedMap(TrackQueue[QueuePosition]);
            LoadSelectedTrack();
        }

        public void Next()
        {
            if (!CanControlListeningParty() || LoadingTrack || IsCurrentScreenExiting())
                return;

            if (QueuePosition < TrackQueue.Count - 1)
            {
                QueuePosition++;
                SetSelectedMap(TrackQueue[QueuePosition]);
                LoadSelectedTrack();
                return;
            }

            var mapsets = MapManager.Mapsets.Where(mapset => mapset.Maps.Count != 0).ToList();
            if (mapsets.Count == 0)
                return;

            var mapset = mapsets[Random.Next(mapsets.Count)];
            var map = mapset.Maps[Random.Next(mapset.Maps.Count)];
            TrackQueue.Add(map);
            QueuePosition = TrackQueue.Count - 1;
            SetSelectedMap(map);
            LoadSelectedTrack();
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
            MapManager.Selected.ValueChanged -= OnSelectedMapChanged;
        }

        private void LoadSelectedTrack()
        {
            if (LoadingTrack || IsDestroyed || MapManager.Selected?.Value == null || IsCurrentScreenExiting())
                return;

            LoadingTrack = true;
            var map = MapManager.Selected.Value;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    if (IsDestroyed)
                        return;

                    AudioEngine.LoadCurrentTrack();
                    if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed)
                    {
                        AudioEngine.Track.Play();
                        OnlineManager.UpdateListeningPartyState(ListeningPartyAction.ChangeSong);
                    }

                    Logger.Debug($"Selected V2 footer track: {map.Artist} - {map.Title} " + $"[{map.DifficultyName}]", LogType.Runtime);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, LogType.Runtime);
                    Logger.Error("Failed to load a track from the V2 footer music player.", LogType.Runtime);
                }
                finally
                {
                    LoadingTrack = false;
                }
            });
        }

        private void OnSelectedMapChanged(object sender, BindableValueChangedEventArgs<Map> args)
        {
            if (!ChangingSelection)
                AddSelectedMapToQueue(args.Value);
        }

        private void AddSelectedMapToQueue(Map map)
        {
            if (map == null || QueuePosition >= 0 && ReferenceEquals(TrackQueue[QueuePosition], map))
                return;

            if (QueuePosition < TrackQueue.Count - 1)
                TrackQueue.RemoveRange(QueuePosition + 1, TrackQueue.Count - QueuePosition - 1);

            TrackQueue.Add(map);
            QueuePosition = TrackQueue.Count - 1;
        }

        private void SetSelectedMap(Map map)
        {
            ChangingSelection = true;
            MapManager.Selected.Value = map;
            ChangingSelection = false;
        }

        private static bool CanControlListeningParty()
        {
            if (OnlineManager.IsListeningPartyHost)
                return true;

            NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Music_ListeningPartyHostRequired"));
            return false;
        }

        private static bool IsCurrentScreenExiting() => GameBase.Game is QuaverGame game && game.CurrentScreen?.Exiting == true;
    }
}
