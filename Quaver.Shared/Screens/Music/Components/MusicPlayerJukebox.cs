using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Objects.Listening;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Music.Components
{
    public class MusicPlayerJukebox : Container, IJukebox
    {
        /// <summary>
        /// </summary>
        private Bindable<List<Mapset>> AvailableSongs { get; }

        /// <summary>
        ///     The list of tracks (maps) that were played during this jukebox section,
        ///     so we can go to the previous/next song.
        /// </summary>
        private List<Map> TrackListQueue { get; } = new List<Map>();

        /// <summary>
        /// </summary>
        public int TrackListQueuePosition { get; set; } = -1;

        /// <summary>
        /// </summary>
        public bool LoadingNextTrack { get; private set; }

        /// <summary>
        /// </summary>
        public int LoadFailures { get; set; }

        /// <summary>
        /// </summary>
        public MusicPlayerJukebox(Bindable<List<Mapset>> availableSongs)
        {
            AvailableSongs = availableSongs;
            MapManager.Selected.ValueChanged += OnMapChanged;

            if (OnlineManager.Client != null)
            {
                SelectListeningPartySong();

                OnlineManager.Client.OnListeningPartyStateUpdate += OnListeningPartyStateUpdate;
                OnlineManager.Client.OnListeningPartyFellowJoined += OnlisteningPartyFellowJoined;
                OnlineManager.Client.OnListeningPartyFellowLeft += OnListeningPartyFellowLeft;
            }

            // If a track is already playing, add it to the queue.
            if (MapManager.Selected == null || MapManager.Selected.Value == null)
                return;

            TrackListQueue.Add(MapManager.Selected.Value);
            TrackListQueuePosition++;

            SetRichPresence();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SelectNextTrackIfFinished();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnListeningPartyStateUpdate -= OnListeningPartyStateUpdate;
                OnlineManager.Client.OnListeningPartyFellowJoined -= OnlisteningPartyFellowJoined;
                OnlineManager.Client.OnListeningPartyFellowLeft -= OnListeningPartyFellowLeft;
            }

            base.Destroy();
        }

        /// <summary>
        ///     Selects tracks continously whenever the current one is complete.
        ///
        ///     Called in update.
        /// </summary>
        private void SelectNextTrackIfFinished()
        {
            if (!OnlineManager.IsListeningPartyHost)
                return;

            // Don't execute if we're in the middle of loading a new track.
            if (LoadingNextTrack)
                return;

            // Start selecting random tracks.
            if (AvailableSongs.Value.Count != 0 && AudioEngine.Track == null || AudioEngine.Track != null &&
                AudioEngine.Track.HasPlayed && AudioEngine.Track.IsStopped)
            {
                SelectNextTrack(Direction.Forward);
            }
        }

        /// <summary>
        ///     Selects a random map to be selected. (and for the track to play.)
        /// </summary>
        public void SelectNextTrack(Direction direction)
        {
            var game = (QuaverGame)GameBase.Game;

            if (AvailableSongs.Value.Count == 0 || game.CurrentScreen != null && game.CurrentScreen.Exiting)
                return;

            try
            {
                switch (direction)
                {
                    case Direction.Forward:
                        // We ran out of songs to play in the queue, so we need to pick a new one
                        if (TrackListQueuePosition == TrackListQueue.Count - 1)
                        {
                            var index = AvailableSongs.Value.FindIndex(x => x.Maps.Contains(MapManager.Selected.Value));

                            if (index != -1 && index + 1 < AvailableSongs.Value.Count)
                                MapManager.Selected.Value = AvailableSongs.Value[index + 1].Maps.First();
                        }
                        else if (TrackListQueuePosition + 1 < TrackListQueue.Count)
                        {
                            MapManager.Selected.Value = TrackListQueue[TrackListQueuePosition + 1];
                            TrackListQueuePosition++;
                        }
                        break;
                    case Direction.Backward:
                        // Don't allow backwards skipping if there arent any tracks before.
                        if (TrackListQueuePosition - 1 < 0)
                            return;

                        TrackListQueuePosition--;
                        MapManager.Selected.Value = TrackListQueue[TrackListQueuePosition];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }

                LoadTrack();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                LoadFailures++;

                // If we fail to load a track 3 times, then turn off the jukebox...
                if (LoadFailures == 3)
                {
                    LoadingNextTrack = true;
                    Logger.Error($"Failed to load track 3 times in a row. Stopping Jukebox", LogType.Runtime);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void LoadTrack()
        {
            if (LoadingNextTrack)
                return;

            LoadingNextTrack = true;

            ThreadScheduler.Run(() =>
            {
                try
                {
                    AudioEngine.LoadCurrentTrack();

                    lock (AudioEngine.Track)
                    {
                        if (!OnlineManager.IsListeningPartyHost)
                        {
                            // The host currently has the song paused, so just seek and wait
                            if (OnlineManager.ListeningParty.IsPaused)
                            {
                                AudioEngine.Track.Seek(OnlineManager.ListeningParty.SongTime);
                                AudioEngine.Track.Play();
                                AudioEngine.Track.Pause();
                            }
                            // Need to calculate the current song time from the listening party's last action time
                            else
                            {
                                var unix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                var seekTime = unix - OnlineManager.ListeningParty.LastActionTime + OnlineManager.ListeningParty.SongTime;

                                try
                                {
                                    AudioEngine.Track.Seek(seekTime);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(e, LogType.Runtime);
                                }

                                AudioEngine.Track.Play();
                            }
                        }
                        // We're the host, so just play it normally
                        else
                        {
                            AudioEngine.Track.Play();
                        }
                    }

                    OnlineManager.UpdateListeningPartyState(ListeningPartyAction.ChangeSong);
                }
                catch (Exception e)
                {
                    Logger.Error($"Track for map: could not be loaded.", LogType.Runtime);
                }
                finally
                {
                    LoadingNextTrack = false;
                    LoadFailures = 0;
                }

                if (MapManager.Selected.Value == null)
                {
                    if (!AudioEngine.Track.IsStopped)
                        AudioEngine.Track.Stop();
                }
                else
                {
                    Logger.Debug($"Selected new jukebox track ({TrackListQueuePosition}): " +
                                 $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title} " +
                                 $"[{MapManager.Selected.Value.DifficultyName}] ", LogType.Runtime);
                }
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            try
            {
                SetRichPresence();

                // Stop the music if it was changed to a map we don't have.
                if (e.Value == null)
                {
                    if (!AudioEngine.Track.IsStopped)
                    {
                        if (!AudioEngine.Track.IsPaused)
                            AudioEngine.Track.Pause();

                        AudioEngine.Track.Seek(0);
                    }
                }

                if (!TrackListQueue.Contains(e.Value))
                {
                    TrackListQueue.Add(e.Value);
                    TrackListQueuePosition = TrackListQueue.Count - 1;
                }

                LoadTrack();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyStateUpdate(object sender, ListeningPartyStateUpdateEventArgs e)
        {
            SetRichPresence();

            if (OnlineManager.IsListeningPartyHost)
                return;

            switch (e.Action)
            {
                case ListeningPartyAction.ChangeSong:
                    SelectListeningPartySong();
                    break;
                case ListeningPartyAction.Pause:
                    lock (AudioEngine.Track)
                    {
                        if (AudioEngine.Track.IsPlaying)
                            AudioEngine.Track.Pause();
                    }
                    break;
                case ListeningPartyAction.Play:
                    lock (AudioEngine.Track)
                    {
                        if (AudioEngine.Track.IsPaused)
                            AudioEngine.Track.Play();
                    }
                    break;
                case ListeningPartyAction.Seek:
                    lock (AudioEngine.Track)
                    {
                        try
                        {
                            AudioEngine.Track.Seek(e.SongTime);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, LogType.Runtime);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        private void SelectListeningPartySong()
        {
            if (OnlineManager.IsListeningPartyHost)
                return;

            MapManager.Selected.Value = MapManager.FindMapFromMd5(OnlineManager.ListeningParty.MapMd5);

            // User is missing the map, so inform the server of this.
            if (MapManager.Selected.Value == null)
                OnlineManager.Client?.InformMissingListeningPartySong();
            // Otherwise, make the server aware that we have the map
            else
                OnlineManager.Client?.InformListeningPartyHasMap();

            Logger.Important($"Host changed listening party map to: {OnlineManager.ListeningParty.MapMd5}", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        private void SetRichPresence()
        {
            if (OnlineManager.ListeningParty == null || OnlineManager.ListeningParty.Listeners.Count <= 1)
            {
                DiscordHelper.Presence.Details = $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}";
                DiscordHelper.Presence.State = "Listening";
                DiscordHelper.Presence.PartySize = 0;
                DiscordHelper.Presence.PartyMax = 0;
            }
            else
            {
                DiscordHelper.Presence.Details = $"{OnlineManager.ListeningParty.SongArtist} - {OnlineManager.ListeningParty.SongTitle}";
                DiscordHelper.Presence.State = "Listening Party";
                DiscordHelper.Presence.PartySize = OnlineManager.ListeningParty.Listeners.Count;
                DiscordHelper.Presence.PartyMax = 16;
            }

            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
            DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);
            DiscordHelper.UpdatePresence();

            SteamManager.SetRichPresence("State", DiscordHelper.Presence.State);
            SteamManager.SetRichPresence("Details", DiscordHelper.Presence.Details);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlisteningPartyFellowJoined(object sender, ListeningPartyFellowJoinedEventArgs e) => SetRichPresence();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListeningPartyFellowLeft(object sender, ListeningPartyFellowLeftEventArgs e) => SetRichPresence();
    }
}