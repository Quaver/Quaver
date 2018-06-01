using System;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.States.Gameplay
{
    internal class GameplayTiming : IGameStateComponent
    {
        /// <summary>
        ///     The current audio time.
        /// </summary>
        private double _currentTime;
        internal double CurrentTime
        {
            get
            {
                if (_currentTime <= 0)
                    return _currentTime;
                
                return _currentTime + AudioEngine.BassDelayOffset * GameBase.AudioEngine.PlaybackRate;
            }
            set => _currentTime = value;
        }

        /// <summary>
        ///     The amount of time it takes before the gameplay/song actually starts.
        /// </summary>
        internal int StartDelay { get; } = 3000;
        
        /// <summary>
        ///     Reference to the gameplay screen.
        /// </summary>
        private GameplayScreen GameplayScreen { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal GameplayTiming(GameplayScreen game)
        {
            GameplayScreen = game;
            
            // Reload the audio stream.
            try
            {
                GameBase.AudioEngine.ReloadStream();
            }
            catch (AudioEngineException e)
            {
                Logger.LogError(e, LogType.Runtime);
            }

            
            // Set the current time & the end time of the map.
            CurrentTime = -StartDelay * GameBase.AudioEngine.PlaybackRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {            
            Logger.Add("GameplayAudio", $"Audio Time: 0", Color.White);
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnloadContent()
        {
            Logger.Remove("GameplayAudio");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {        
            UpdateSongTime(dt);
            
            // Update the audio's time on the logger.
            Logger.Update("GameplayAudio", $"Audio Time: {CurrentTime}");
        }

         /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
        }

        /// <summary>
        ///     Updates the current song time.
        /// </summary>
        /// <param name="dt"></param>
        private void UpdateSongTime(double dt)
        {
            if (GameplayScreen.IsPaused || GameplayScreen.Failed || GameplayScreen.IsPlayComplete)
                return;
                        
            // If the audio didn't begin yet, 
            if (CurrentTime < 0)
            {
                CurrentTime += dt * GameBase.AudioEngine.PlaybackRate;
                return;
            }
                      
            // Play the audio stream if the current time is past the start delay. 
            if (!GameplayScreen.HasStarted)
            {
                try
                {
                    GameplayScreen.HasStarted = true;
                    GameBase.AudioEngine.Play();
                }
                catch (AudioEngineException e) {}
            }
            
            // If the audio is playing, use the song itself for song pos.
            if (GameBase.AudioEngine.IsPlaying)
                CurrentTime = (GameBase.AudioEngine.Position + (CurrentTime + dt * GameBase.AudioEngine.PlaybackRate)) / 2;
            // Otherwise use deltatime.
            else
                CurrentTime += dt * GameBase.AudioEngine.PlaybackRate;
        }
    }
}