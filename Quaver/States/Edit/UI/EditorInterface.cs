using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons.Selection;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.UI;
using Quaver.Main;
using Quaver.States.Edit.UI.Components;
using Quaver.States.Menu;
using static Quaver.Main.GameBase;

namespace Quaver.States.Edit.UI
{
    internal class EditorInterface : IGameStateComponent
    {
        /// <summary>
        ///     Reference to the parent editor screen.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     Sprite container for all editor elements.
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     The current time in the map.
        /// </summary>
        private EditorSongTimeDisplay CurrentTime { get; set; }

        /// <summary>
        ///     The navbar for the editor.
        /// </summary>
        private Nav Navbar { get; set; }

        /// <summary>
        ///     The navbar button to play and pause the audio track.
        /// </summary>
        private NavbarButton PlayAndPauseButton { get; set; }

        /// <summary>
        ///     The bar that controls where in the song to seek to.
        /// </summary>
        private EditorSeekBar SeekBar { get; set; }

        /// <summary>
        ///     Allows the user to select the current beat snap.
        /// </summary>
        private HorizontalSelector BeatSnapSelector { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal EditorInterface(EditorScreen screen) => Screen = screen;

        /// <summary>
        ///
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new Container();
            CreateSongTimeDisplay();
            CreateNavbar();
            CreateSeekBar();
            CreateBeatSnapSelector();

            GameBase.AudioEngine.OnPlayed += OnAudioPlayed;
            GameBase.AudioEngine.OnPaused += OnAudioPausedOrStopped;
            GameBase.AudioEngine.OnStopped += OnAudioPausedOrStopped;
        }

        /// <summary>
        ///
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
            Navbar.UnloadContent();

            // ReSharper disable once DelegateSubtraction
            if (GameBase.AudioEngine.OnPlayed != null)
                GameBase.AudioEngine.OnPlayed -= OnAudioPlayed;

            if (GameBase.AudioEngine.OnPaused != null)
                // ReSharper disable once DelegateSubtraction
                GameBase.AudioEngine.OnPaused -= OnAudioPausedOrStopped;

            if (GameBase.AudioEngine.OnStopped != null)
                // ReSharper disable once DelegateSubtraction
                GameBase.AudioEngine.OnStopped -= OnAudioPausedOrStopped;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Hide the global nav.
            GameBase.Navbar.PerformHideAnimation(dt);

            Container.Update(dt);

            Navbar.Update(dt);
            Navbar.PerformShowAnimation(dt);
        }

        /// <summary>
        ///
        /// </summary>
        public void Draw()
        {
            SpriteBatch.Begin();
            BackgroundManager.Draw();
            SpriteBatch.End();

            // Draw editor in the middle, so it's behind all UI components but above the background.
            Screen.EditorGameMode.Draw();

            // Draw general UI componenets
            SpriteBatch.Begin();
            Container.Draw();
            Navbar.Draw();
            SpriteBatch.End();
        }

        /// <summary>
        ///     Creates the navbar for the editor.
        /// </summary>
        private void CreateNavbar()
        {
            Navbar = new Nav(() =>
            {
                // Go to main menu
                Navbar.CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Home, "Home", "Go to the main menu", (sender, e) =>
                {
                    GameBase.GameStateManager.ChangeState(new MainMenuScreen());
                });

                // Pause/Play
                PlayAndPauseButton = Navbar.CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Play, "Play", "Play the song.", (sender, e) =>
                {
                    // Resumable.
                    if (GameBase.AudioEngine.IsPlaying)
                    {
                        GameBase.AudioEngine.Pause();
                    }
                    else if (GameBase.AudioEngine.IsPaused)
                    {
                        GameBase.AudioEngine.Play();
                    }
                    else if (GameBase.AudioEngine.IsStopped)
                    {
                        GameBase.AudioEngine.ReloadStream();
                        GameBase.AudioEngine.Play();
                    }
                });

                // Stop Button
                Navbar.CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Stop, "Stop", "Stops the music", (sender, e) =>
                {
                    if (GameBase.AudioEngine.IsStopped)
                        return;

                    GameBase.AudioEngine.Stop();
                });

                // Change Rate Button
                Navbar.CreateNavbarButton(NavbarAlignment.Left, FontAwesome.Clock, "Change Audio Rate to 25%", "Change the playback rate", (sender, e) =>
                {
                    switch (GameBase.AudioEngine.PlaybackRate)
                    {
                        case 0.25f:
                            GameBase.AudioEngine.PlaybackRate = 0.50f;
                            break;
                        case 0.50f:
                            GameBase.AudioEngine.PlaybackRate = 0.75f;
                            break;
                        case 0.75f:
                            GameBase.AudioEngine.PlaybackRate = 1.0f;
                            break;
                        case 1.0f:
                            GameBase.AudioEngine.PlaybackRate = 0.25f;
                            break;
                        default:
                            GameBase.AudioEngine.PlaybackRate = 1.0f;
                            break;
                    }

                    GameBase.AudioEngine.SetPlaybackRate(false);

                    var speedButton = (NavbarButton) sender;

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    var nextRate = GameBase.AudioEngine.PlaybackRate == 1.0f ? 25 : (GameBase.AudioEngine.PlaybackRate + 0.25) * 100;
                    speedButton.TooltipName = $"Change Audio Rate to {nextRate}%";
                });

                // Open Mapset Folder
                Navbar.CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Folder, "Open Mapset Folder", "Open the mapset directory.", (sender, e) =>
                {
                    var map = SelectedMap;
                    Process.Start($"{ConfigManager.SongDirectory}/{map.Directory}/");
                });

                // Open .qua file
                Navbar.CreateNavbarButton(NavbarAlignment.Right, FontAwesome.File, "Open .qua File", "Opens the file in a text editor.", (sender, e) =>
                {
                    var map = SelectedMap;
                    Process.Start($"{ConfigManager.SongDirectory}/{map.Directory}/{map.Path}");
                });

                // Save
                Navbar.CreateNavbarButton(NavbarAlignment.Right, FontAwesome.Save, "Save", "Save the map to disk", (sender, e) => Screen.SaveMap());
            });

            Navbar.Initialize(Screen);
        }

        /// <summary>
        ///    Creates the bar to seek through the song's progress.
        /// </summary>
        private void CreateSeekBar()
        {
            SeekBar = new EditorSeekBar(SeekBarAxis.Horizontal, new Vector2(WindowRectangle.Width, 30))
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
            };
        }

        /// <summary>
        ///     Creates the
        /// </summary>
        private void CreateSongTimeDisplay()
        {
            CurrentTime = new EditorSongTimeDisplay(NumberDisplayType.SongTime, "00:00", new Vector2(2, 2))
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                PosY = -70,
                PosX = 0
            };
        }

        /// <summary>
        ///     Creates the element where users are able to select the current beat snap.
        /// </summary>
        private void CreateBeatSnapSelector()
        {
            var snaps = new List<string> { "1/1", "1/2", "1/3", "1/4", "1/6", "1/8", "1/12", "1/16", "1/32", "1/48" };

            BeatSnapSelector = new HorizontalSelector(snaps, new Vector2(200, 30), (item, index) =>
            {
                // Beat Snaps are in the format "1/x". This just splits it, and parses the snap.
                Screen.CurrentBeatSnap.Value = int.Parse(item.Split('/')[1]);
            }, 3)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosX = 200
            };
        }

        /// <summary>
        ///     Called when the audio track has started playing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAudioPlayed(object sender, EventArgs e)
        {
            // Change the navbar button to display a pause button.
            PlayAndPauseButton.Image = FontAwesome.Pause;
            PlayAndPauseButton.TooltipName = "Pause";
            PlayAndPauseButton.TooltipDescription = "Pauses the music.";
        }

        /// <summary>
        ///     Called when the audio track has paused or stopped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAudioPausedOrStopped(object sender, EventArgs e)
        {
            // Change the navbar button to display a play button.
            PlayAndPauseButton.Image = FontAwesome.Play;
            PlayAndPauseButton.TooltipName = "Play";
            PlayAndPauseButton.TooltipDescription = "Resumes the music.";
        }
    }
}