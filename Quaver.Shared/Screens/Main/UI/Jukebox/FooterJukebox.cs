using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Music.Components;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class FooterJukebox : ImageButton, IJukebox
    {
        /// <summary>
        /// </summary>
        private FooterJukeboxMapBackground Background { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxPlayPauseButton PlayPauseButton { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxBackwardsButton BackwardsButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton NextButton { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxProgressBar ProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private FooterJukeboxSongInfo SongInfo { get; set; }

        /// <summary>
        /// </summary>
        private IconButton InfoButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton GoToJukeboxScreenButton { get; set; }

        /// <summary>
        ///     The list of tracks (maps) that were played during this jukebox section,
        ///     so we can go to the previous/next song.
        /// </summary>
        private List<Map> TrackListQueue { get; } = new List<Map>();

        /// <summary>
        /// </summary>
        public Qua Qua { get; private set; } = new Qua();

        /// <summary>
        ///     The current track in the queue we're currently on.
        ///     Basically the index of <see cref="TrackListQueue"/>
        ///
        ///     Started at -1 because there may not be any tracks to begin with,
        ///     meaning the user doesn't have any mapsets loaded.
        /// </summary>
        public int TrackListQueuePosition { get; set; } = -1;

        /// <summary>
        /// </summary>
        public bool LoadingNextTrack { get; private set; }

        /// <summary>
        /// </summary>
        public int LoadFailures { get; set; }

        /// <summary>
        ///     Selects new random maps to play.
        /// </summary>
        public Random RNG { get; } = new Random();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public FooterJukebox() : base(UserInterface.BlankBox)
        {
            Depth = 1;
            Size = new ScalableVector2(500, 54);
            Alpha = 0;

            // If a track is already playing, add it to the queue.
            if (MapManager.Selected != null && MapManager.Selected.Value != null)
            {
                TrackListQueue.Add(MapManager.Selected.Value);
                TrackListQueuePosition++;
            }

            CreateBackgrouund();
            CreatePlayPauseButton();
            CreatePreviousButton();
            CreateNextButton();
            CreateSongInfo();
            //CreateInfoIcon();
            CreateProgressBar();

            Clicked += (sender, args) => HandleClickSeeking();
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

        /// <summary>
        /// </summary>
        private void CreateBackgrouund() => Background = new FooterJukeboxMapBackground(this)
        {
            Parent = this,
            Alignment = Alignment.MidCenter
        };

        /// <summary>
        /// </summary>
        private void CreatePlayPauseButton()
        {
            PlayPauseButton = new JukeboxPlayPauseButton(this)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(34, 34),
                Y = -1,
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePreviousButton()
        {
            BackwardsButton = new JukeboxBackwardsButton(this)
            {
                Parent = PlayPauseButton,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(24, 22),
                X = -50
            };
        }

        /// <summary>
        /// </summary>
        private void CreateNextButton()
        {
            NextButton = new JukeboxForwardsButton(this)
            {
                Parent = PlayPauseButton,
                Alignment = Alignment.MidRight,
                Size = BackwardsButton.Size,
                Y = BackwardsButton.Y,
                X = -BackwardsButton.X,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSongInfo()
        {
            SongInfo = new FooterJukeboxSongInfo()
            {
                Parent = this,
                X = 22,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateInfoIcon()
        {
            InfoButton = new IconButton(UserInterface.JukeboxInfoButton, (sender, args) =>
            {
                if (SongInfo.Active)
                    SongInfo.Deactivate();
                else
                    SongInfo.Activate();
            })
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(22, 22),
                X = 20
            };

            InfoButton.ClickedOutside += (sender, args) => SongInfo.Deactivate();
        }

        /// <summary>
        /// </summary>
        private void CreateGoToJukeboxScreenButton()
        {
            GoToJukeboxScreenButton = new IconButton(UserInterface.JukeboxHamburgerIcon, (sender, args) =>
            {
                var game = (QuaverGame) GameBase.Game;
                game.CurrentScreen?.Exit(() => new MusicPlayerScreen());
            })
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(24, 20),
                X = -InfoButton.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateProgressBar()
        {
            ProgressBar = new JukeboxProgressBar(new Vector2(Width - 4, 2), 0, int.MaxValue,
                0, Color.Gray, SkinManager.Skin?.MainMenu?.JukeboxProgressBarColor ?? Colors.SecondaryAccent)
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
            };
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

            if (AudioEngine.Track != null && !AudioEngine.Track.HasPlayed)
                AudioEngine.Track.Play();
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
                        // We ran out of songs to play in the queue, so we need to pick a random one.
                        if (TrackListQueuePosition == TrackListQueue.Count - 1)
                        {
                            var randomSet = RNG.Next(0, MapManager.Mapsets.Count);
                            var randomMap = RNG.Next(0, MapManager.Mapsets[randomSet].Maps.Count);

                            MapManager.Selected.Value = MapManager.Mapsets[randomSet].Maps[randomMap];
                            TrackListQueue.Add(MapManager.Selected.Value);
                        }
                        else
                        {
                            MapManager.Selected.Value = TrackListQueue[TrackListQueuePosition + 1];
                        }

                        TrackListQueuePosition++;
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

                LoadingNextTrack = true;

                ThreadScheduler.Run(() =>
                {
                    try
                    {
                        AudioEngine.LoadCurrentTrack();

                        if (AudioEngine.Track != null)
                        {
                            lock (Qua)
                                Qua  = MapManager.Selected.Value.LoadQua();

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
        private void HandleClickSeeking()
        {
            if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                return;

            try
            {
                var percentage = (MouseManager.CurrentState.X - AbsolutePosition.X) / AbsoluteSize.X;

                lock (AudioEngine.Track)
                    AudioEngine.Track.Seek(percentage * AudioEngine.Track.Length);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);
            }
        }
    }
}