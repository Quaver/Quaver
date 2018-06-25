using System;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.UserInterface;
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
                    GameBase.GameStateManager.ChangeState(new MainMenuState());
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
            });
            
            Navbar.Initialize(Screen);
        }
    
        /// <summary>
        ///     Creates the 
        /// </summary>
        private void CreateSongTimeDisplay()
        {
            CurrentTime = new EditorSongTimeDisplay(NumberDisplayType.SongTime, "00:00", new Vector2(2, 2))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,     
                PosY = -30,
                PosX = 0
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