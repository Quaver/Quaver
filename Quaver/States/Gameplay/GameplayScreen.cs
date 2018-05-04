using System;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Gameplay
{
    internal class GameplayScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public State CurrentState { get; set; } = State.PlayScreen;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The specific audio timimg for this gameplay state.
        /// </summary>
        internal GameplayAudio AudioTiming { get; }

        /// <summary>
        ///     If the play session is finished.
        /// </summary>
        internal bool Finished { get; set; }

        /// <summary>
        ///     If the game is currently paused.
        /// </summary>
        internal bool Paused { get; set; }

        /// <summary>
        ///     The current parsed .qua file that is being played.
        /// </summary>
        internal Qua Map { get; }
        
        /// <summary>
        ///     The hash of the map that was played.
        /// </summary>
        private string MapHash { get; }

        /// <summary>
        ///     Keeps track of the previous start time in the delay.
        /// </summary>
        private long InitializationTime { get; set;  }

        /// <summary>
        ///     Dictates if we are currently resuming the game.
        /// </summary>
        private bool ResumeInProgress { get; set; }

        /// <summary>
        ///     The time the user resumed the game.
        /// </summary>
        private long ResumeTime { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal GameplayScreen(Qua map, string md5)
        {
            Map = map;
            MapHash = md5;
            
            AudioTiming = new GameplayAudio(this);
        
            // Hook onto the event of when a key is pressed so we can handle input.
            GameBase.GameWindow.TextInput += OnKeyPressed;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {           
            AudioTiming.Initialize(this);
            
            // Set the delay time last, so that we can begin to start the audio track.
            InitializationTime = GameBase.GameTime.ElapsedMilliseconds;

            // Change discord rich presence.
            DiscordController.ChangeDiscordPresenceGameplay(false);
            
            // Add gameplay loggers
            Logger.Add("Paused", $"Paused: {Paused}", Color.White);
            Logger.Add("Resume In Progress", $"Resume In Progress {ResumeInProgress}", Color.White);
            
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            AudioTiming.UnloadContent();
            GameBase.GameWindow.TextInput -= OnKeyPressed;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            AudioTiming.Update(dt);  
            HandleInput(dt);
            HandleResuming();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.BlanchedAlmond);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            
            // Update loggers.
            Logger.Update("Paused", $"Paused: {Paused}");
            Logger.Update("Resume In Progress", $"Resume In Progress {ResumeInProgress}");
            
            GameBase.SpriteBatch.End();
        }
        
        /// <summary>
        ///     Handles the input of the given game mode.
        /// </summary>
        /// <param name="dt"></param>
        internal void HandleInput(double dt)
        {
            switch (Map.Mode)
            {
                case GameModes.Keys4:
                    break;
                case GameModes.Keys7:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
        }

        /// <summary>
        ///     Pauses the game.
        /// </summary>
        private void HandlePausing()
        {            
            // Handle pause.
            if (!Paused)
            {
                Paused = true;
                ResumeInProgress = false;
                
                try
                {
                    GameBase.AudioEngine.Pause();
                    Console.WriteLine("Paused the audio");
                }
                catch (AudioEngineException e)
                {
                    // Don't need to handle.
                    Console.WriteLine(e);
                }

                return;
            }

            // Setting the resume time in this case allows us to give the user time to react 
            // with a delay before starting the audio track again.
            // When that resume time is past the specific set offset, it'll unpause the game.
            ResumeInProgress = true;
            ResumeTime = GameBase.GameTime.ElapsedMilliseconds;
        }

        /// <summary>
        ///     Handles resuming of the game.
        ///     Essentially gives a delay before starting the game back up.
        /// </summary>
        private void HandleResuming()
        {
            if (!Paused || !ResumeInProgress)
                return;

            // We don't want to resume if the time difference isn't at least or greter than the start delay.
            var timeDifference = GameBase.GameTime.ElapsedMilliseconds - ResumeTime;
            if (timeDifference < AudioTiming.StartDelay && timeDifference != 0) 
                return;
            
            // Unpause the game and reset the resume in progress.
            Paused = false;
            ResumeInProgress = false;
            
            // Resume the game audio stream.
            try
            {
                GameBase.AudioEngine.Resume();
            } 
            catch (AudioEngineException e) {}
        }
        
        /// <summary>
        ///     Event handler when a key is pressed. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyPressed(object sender, TextInputEventArgs e)
        {
            if (e.Key == ConfigManager.KeyPause.Value)
                HandlePausing();
        }
    }
}