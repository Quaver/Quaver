/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Skinning;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Menu.UI.Jukebox
{
    public class Jukebox : Sprite
    {
        /// <summary>
        ///     The background that contains the text that says "Jukebox"
        /// </summary>
        public Sprite TitleBackground { get; set; }

        /// <summary>
        ///     The text that says "Now Playing"
        /// </summary>
        public SpriteText NowPlayingText { get; set; }

        /// <summary>
        ///     Button to select the previous track.
        /// </summary>
        public JukeboxButton PreviousButton { get; set; }

        /// <summary>
        ///     The button to restart the track
        /// </summary>
        public JukeboxButton RestartButton { get; set; }

        /// <summary>
        ///     The button to pause/unpause the track.
        /// </summary>
        public JukeboxButton PauseResumeButton { get; set; }

        /// <summary>
        ///     The button to select the next track.
        /// </summary>
        public JukeboxButton NextButton { get; set; }

        /// <summary>
        ///     The container that holds the song title.
        /// </summary>
        public ScrollContainer SongTitleContainer { get; set; }

        /// <summary>
        ///     The text that displays the song title.
        /// </summary>
        public SpriteText SongTitleText { get; set; }

        /// <summary>
        ///     The song time progress bar.
        /// </summary>
        public ProgressBar SongTimeProgressBar { get; set; }

        /// <summary>
        ///     The list of tracks (maps) that were played during this jukebox section,
        ///     so we can go to the previous/next song.
        /// </summary>
        private List<Map> TrackListQueue { get; }

        /// <summary>
        ///     The current track in the queue we're currently on.
        ///     Basically the index of <see cref="TrackListQueue"/>
        ///
        ///     Started at -1 because there may not be any tracks to begin with.
        ///     Meaning... the user doesn't have any mapsets loaded.
        /// </summary>
        private int TrackListQueuePosition { get; set; } = -1;

        /// <summary>
        ///     Dictates if we're in the middle of loading the next track on a new thread.
        /// </summary>
        private bool LoadingNextTrack { get; set; }

        /// <summary>
        ///     Selects new random maps to play.
        /// </summary>
        public Random RNG { get; } = new Random();

        /// <summary>
        ///
        /// </summary>
        private int LoadFailures { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Jukebox()
        {
            TrackListQueue = new List<Map>();

            // If a track is already playing, add it to the queue.
            if (MapManager.Selected != null && MapManager.Selected.Value != null)
            {
                TrackListQueue.Add(MapManager.Selected.Value);
                TrackListQueuePosition++;
                ChangeDiscordPresenceToSongTitle();
            }

            Size = new ScalableVector2(614, 40);
            Tint = Color.Black;
            Alpha = 0.55f;

            CreateTitleBackground();
            CreateNowPlayingText();
            CreateSongTitleContainer();
            CreateSongTimeProgressBar();

            // IMPORTANT! Add the contained drawable afterwards, so that it appears on top of the progress bar.
            SongTitleContainer.AddContainedDrawable(SongTitleText);

            CreateControlButtons();
            AddBorder(Color.White, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnimateSongTitleText();
            SetSongTimeProgressBarStatus();
            SelectNextTrackIfFinished();
            HandleJukeboxInput();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Jukebox controls
        /// </summary>
        private void HandleJukeboxInput() {
            if (DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Z))
                PreviousButton.FireButtonClickEvent();

            if (KeyboardManager.IsUniqueKeyPress(Keys.X))
            {
                RestartButton.FireButtonClickEvent();
                NotificationManager.Show(NotificationLevel.Info, "Restarted track");
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.C))
            {
                var isPaused = AudioEngine.Track.IsPaused;
                PauseResumeButton.FireButtonClickEvent();
                NotificationManager.Show(NotificationLevel.Info, isPaused ? "Resumed track" : "Paused track");
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
                NextButton.FireButtonClickEvent();
        }

        /// <summary>
        ///     Selects a random map to be selected. (and for the track to play.)
        /// </summary>
        private void SelectNextTrack(Direction direction)
        {
            if (MapManager.Mapsets.Count == 0)
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
                    // Update the song title's text with the new one.
                    UpdateSongTitleText();

                    // Clear current song title animations.
                    lock (SongTitleText.Animations)
                        SongTitleText.Animations.Clear();

                    Logger.Debug($"Selected new jukebox track ({TrackListQueuePosition}): " +
                                 $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title} " +
                                 $"[{MapManager.Selected.Value.DifficultyName}] ", LogType.Runtime);

                    ChangeDiscordPresenceToSongTitle();
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

                ChangeDiscordPresenceToIdle();
            }
        }

        /// <summary>
        ///     Creates the sprite that serves as a background to the title text.
        /// </summary>
        private void CreateTitleBackground() => TitleBackground = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(130, Height),
            Tint = Color.Black,
            Alpha = 0.45f
        };

        /// <summary>
        ///     Creates the text that says "Now Playing"
        /// </summary>
        private void CreateNowPlayingText() => NowPlayingText = new SpriteText(Fonts.Exo2SemiBold, "Now Playing", 13)
        {
            Parent = TitleBackground,
            Alignment = Alignment.MidCenter,
            X = 2
        };

        /// <summary>
        ///     Creates the container that displays the song title.
        /// </summary>
        private void CreateSongTitleContainer()
        {
            SongTitleContainer = new ScrollContainer(new ScalableVector2(Width - TitleBackground.Width - 100 - Height * 0.50f - 10, Height),
                new ScalableVector2(Width - TitleBackground.Width - 100 - Height * 0.50f - 10, Height))
            {
                Parent = this,
                X = TitleBackground.Width,
                Alpha = 0
            };

            SongTitleText = new SpriteText(Fonts.Exo2SemiBold, " ", 13)
            {
                Y = 2,
                Alignment = Alignment.MidLeft
            };

            UpdateSongTitleText();
            SongTitleContainer.AddContainedDrawable(SongTitleText);
        }

        /// <summary>
        ///     Updates the song title text and realigns/resizes it. it.
        /// </summary>
        private void UpdateSongTitleText()
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (MapManager.Selected != null && MapManager.Selected.Value != null)
                SongTitleText.Text = $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}";
            else
                SongTitleText.Text = $"No tracks available to play";

            SongTitleText.X = SongTitleText.Width + 200;
        }

        /// <summary>
        ///     Animates the song title text from right to left.
        /// </summary>
        private void AnimateSongTitleText()
        {
            // Only reset the animation if the song title doesn't have an active Animation animation.
            if (SongTitleText.Animations.Count != 0)
                return;

            SongTitleText.X = SongTitleContainer.Width;

            SongTitleText.Animations.Add(new Animation(AnimationProperty.X, Easing.Linear,
                SongTitleText.X, -SongTitleContainer.Width -(SongTitleText.Width - SongTitleContainer.Width), 6000));
        }

        /// <summary>
        ///     Creates the jukebox progress bar.
        /// </summary>
        private void CreateSongTimeProgressBar()
        {
            SongTimeProgressBar = new ProgressBar(new Vector2(SongTitleContainer.Width, SongTitleContainer.Height - 4), 0,
                AudioEngine.Track != null && !AudioEngine.Track.IsDisposed ? AudioEngine.Track.Length : int.MaxValue, 0, Color.Transparent, Colors.MainAccent)
            {
                Alignment = Alignment.MidLeft,
                ActiveBar =
                {
                    Alpha = 0.45f
                }
            };

            // Create the invisible bar to seek through the audio.
            var seekBar = new ImageButton(UserInterface.BlankBox, (o, e) =>
            {
                if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                    return;

                // The percentage of how far the mouse is inside of the progress bar.
                try
                {
                    var percentage = (MouseManager.CurrentState.X - SongTimeProgressBar.AbsolutePosition.X) / SongTimeProgressBar.AbsoluteSize.X;

                    Logger.Debug($"Jukebox track seeked to: {(int)(percentage * AudioEngine.Track.Length)}ms ({(int)(percentage * 100)}%)", LogType.Runtime);

                    lock (AudioEngine.Track)
                        AudioEngine.Track.Seek(percentage * AudioEngine.Track.Length);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, LogType.Runtime);
                }
            })
            {
                Parent = SongTimeProgressBar,
                Size = SongTimeProgressBar.Size,
                Position = SongTimeProgressBar.Position,
                Alignment = SongTimeProgressBar.Alignment,
                Alpha = 0
            };

            SongTitleContainer.AddContainedDrawable(SongTimeProgressBar);
        }

         /// <summary>
        ///     Creates the jukebox buttons
        /// </summary>
        private void CreateControlButtons()
        {
            CreateNextSongButton();
            CreatePauseResumeButton();
            CreateRestartButton();
            CreatePreviousSongButton();
        }

        /// <summary>
        ///     Creates the button to allow the user to choose the next song.
        /// </summary>
        private void CreateNextSongButton()
        {
            NextButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_step_forward))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = -10
            };

            NextButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                SelectNextTrack(Direction.Forward);
            };
        }

        /// <summary>
        ///     Creates the pause/resume control button.
        /// </summary>
        private void CreatePauseResumeButton()
        {
            PauseResumeButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = NextButton.X - NextButton.Width - 10
            };

            PauseResumeButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                if (AudioEngine.Track == null || AudioEngine.Track.IsDisposed)
                    return;

                lock (AudioEngine.Track)
                {
                    if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsPaused)
                    {
                        AudioEngine.Track.Play();
                        PauseResumeButton.Image = FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol);
                        ChangeDiscordPresenceToSongTitle();
                    }
                    else
                    {
                        AudioEngine.Track.Pause();
                        PauseResumeButton.Image = FontAwesome.Get(FontAwesomeIcon.fa_play_button);
                        ChangeDiscordPresenceToIdle();
                    }
                }
            };
        }

        /// <summary>
        ///     Creates the restart button
        /// </summary>
        private void CreateRestartButton()
        {
            RestartButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_undo_arrow))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = PauseResumeButton.X - PauseResumeButton.Width - 10,
            };

            RestartButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                try
                {
                    AudioEngine.LoadCurrentTrack();

                    if (AudioEngine.Track != null)
                    {
                        lock (AudioEngine.Track)
                            AudioEngine.Track?.Play();
                    }

                    PauseResumeButton.Image = FontAwesome.Get(FontAwesomeIcon.fa_pause_symbol);
                }
                catch (Exception)
                {
                    Logger.Error($"Failed to load track", LogType.Runtime);
                }
            };
        }

        /// <summary>
        ///     Creates the previous song control button.
        /// </summary>
        private void CreatePreviousSongButton()
        {
            PreviousButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_step_backward))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.50f, Height * 0.50f),
                X = RestartButton.X - RestartButton.Width - 10
            };

            PreviousButton.Clicked += (o, e) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();
                SelectNextTrack(Direction.Backward);
            };
        }

        /// <summary>
        ///     Makes sure the song time progress bar is always up to date with the current time.
        /// </summary>
        private void SetSongTimeProgressBarStatus()
        {
            // Set progress bar time.
            try
            {
                if (AudioEngine.Track == null)
                    return;

                SongTimeProgressBar.Bindable.MaxValue = AudioEngine.Track.Length;
                SongTimeProgressBar.Bindable.MinValue = 0;
                SongTimeProgressBar.Bindable.Value = AudioEngine.Track.Position;
            }
            catch (Exception e)
            {
                // ignored
            }
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
        ///     Changes the discord presence to a listening state.
        /// </summary>
        private static void ChangeDiscordPresenceToSongTitle()
        {
            DiscordHelper.Presence.Details = $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}";
            DiscordHelper.Presence.State = "In the Menus - Listening";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <summary>
        ///     Changes the discord presence to an idle state
        /// </summary>
        private static void ChangeDiscordPresenceToIdle()
        {
            DiscordHelper.Presence.Details = $"Idle";
            DiscordHelper.Presence.State = "In the Menus";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }
    }
}
