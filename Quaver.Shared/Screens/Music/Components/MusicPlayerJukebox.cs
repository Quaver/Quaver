using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
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
        public MusicPlayerJukebox()
        {
            MapManager.Selected.ValueChanged += OnMapChanged;

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

            base.Destroy();
        }

        /// <summary>
        ///     Selects tracks continously whenever the current one is complete.
        ///
        ///     Called in update.
        /// </summary>
        private void SelectNextTrackIfFinished()
        {
            // Don't execute if we're in the middle of loading a new track.
            if (LoadingNextTrack)
                return;

            // Start selecting random tracks.
            if (MapManager.Mapsets.Count != 0 && AudioEngine.Track == null || AudioEngine.Track != null &&
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
            var game = (QuaverGame) GameBase.Game;

            if (MapManager.Mapsets.Count == 0 || game.CurrentScreen != null && game.CurrentScreen.Exiting)
                return;

            try
            {
                switch (direction)
                {
                    case Direction.Forward:
                        // We ran out of songs to play in the queue, so we need to pick a new one
                        if (TrackListQueuePosition == TrackListQueue.Count - 1)
                        {
                            var index = MapManager.Mapsets.IndexOf(MapManager.Selected.Value.Mapset);

                            if (index != -1 && index + 1 < MapManager.Mapsets.Count)
                                MapManager.Selected.Value = MapManager.Mapsets[index + 1].Maps.First();
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

                    if (AudioEngine.Track != null)
                    {
                        lock (AudioEngine.Track)
                            AudioEngine.Track.Play();
                    }
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

                Logger.Debug($"Selected new jukebox track ({TrackListQueuePosition}): " +
                             $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title} " +
                             $"[{MapManager.Selected.Value.DifficultyName}] ", LogType.Runtime);
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
                if (!TrackListQueue.Contains(e.Value))
                {
                    TrackListQueue.Add(e.Value);
                    TrackListQueuePosition = TrackListQueue.Count - 1;
                }

                SetRichPresence();
                LoadTrack();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        private void SetRichPresence()
        {
            DiscordHelper.Presence.Details = $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}";
            DiscordHelper.Presence.State = "Listening";
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }
    }
}