using System;
using System.ComponentModel;
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
using Quaver.States.Gameplay.GameModes.Keys;
using Quaver.States.Gameplay.UI;

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
        ///     The curent game mode object.
        /// </summary>
        internal GameModeRuleset GameModeComponent { get; }

        /// <summary>
        ///     The general gameplay UI.
        /// </summary>
        private GameplayInterface UI { get; }

        /// <summary>
        ///     If the game is currently paused.
        /// </summary>
        internal bool Paused { get; set; }

        /// <summary>
        ///     If the game session has already been started.
        /// </summary>
        internal bool Started { get; set; }

        /// <summary>
        ///     The current parsed .qua file that is being played.
        /// </summary>
        internal Qua Map { get; }
        
        /// <summary>
        ///     The hash of the map that was played.
        /// </summary>
        private string MapHash { get; }

        /// <summary>
        ///     Dictates if we are currently resuming the game.
        /// </summary>
        private bool ResumeInProgress { get; set; }

        /// <summary>
        ///     The time the user resumed the game.
        /// </summary>
        private long ResumeTime { get; set; }

        /// <summary>
        ///     The last recorded combo. We use this value for combo breaking.
        /// </summary>
        private int LastRecordedCombo { get; set; }

        /// <summary>
        ///     Dictates if the intro of the song is currently skippable.
        /// </summary>
        private bool IntroSkippable
        {
            get => GameBase.SelectedMap.Qua.HitObjects[0].StartTime - AudioTiming.CurrentTime >= AudioTiming.StartDelay + 2000;            
        }

         /// <summary>
        ///     Ctor - 
        /// </summary>
        internal GameplayScreen(Qua map, string md5)
        {
            Map = map;
            MapHash = md5;
            
            AudioTiming = new GameplayAudio(this);
            UI = new GameplayInterface(this);
            
            // Set the game mode component.
            switch (map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    GameModeComponent = new GameModeKeys(this, map.Mode, map);
                    break;
                default:
                    throw new InvalidEnumArgumentException("Game mode must be a valid!");
            }
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Initialize()
        {           
            AudioTiming.Initialize(this);
            UI.Initialize(this);
            
            // Change discord rich presence.
            DiscordController.ChangeDiscordPresenceGameplay(false);
            
            // Initialize the game mode.
            GameModeComponent.Initialize();
            
            // Add gameplay loggers
            /*Logger.Add("Paused", $"Paused: {Paused}", Color.White);
            Logger.Add("Resume In Progress", $"Resume In Progress {ResumeInProgress}", Color.White);
            Logger.Add("Intro Skippable", $"Intro Skippable: {IntroSkippable}", Color.White);
            Logger.Add("Score", $"Score: {GameModeComponent.ScoreProcessor.Score}", Color.White);
            Logger.Add($"Accuracy", $"Accuracy: {GameModeComponent.ScoreProcessor.Accuracy}", Color.White);
            Logger.Add($"Combo", $"Combo: {GameModeComponent.ScoreProcessor.Combo}", Color.White);
            Logger.Add($"Max Combo", $"Max Combo: {GameModeComponent.ScoreProcessor.MaxCombo}", Color.White);
            Logger.Add($"Objects Left", $"Objects Left {GameModeComponent.HitObjectManager.ObjectsLeft}", Color.White);
            Logger.Add($"Finished", $"Finished: {GameModeComponent.HitObjectManager.IsComplete}", Color.White);*/
               
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            AudioTiming.UnloadContent();
            UI.UnloadContent();
            GameModeComponent.Destroy();
            Logger.Clear();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            AudioTiming.Update(dt); 
            UI.Update(dt);
            HandleInput(dt);
            HandleResuming();
            PauseIfWindowInactive();
            PlayComboBreakSound();
            GameModeComponent.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.BlanchedAlmond);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            GameModeComponent.Draw();
            UI.Draw();
            
            // Update loggers.
            /*Logger.Update("Paused", $"Paused: {Paused}");
            Logger.Update("Resume In Progress", $"Resume In Progress {ResumeInProgress}");
            Logger.Update("Intro Skippable", $"Intro Skippable: {IntroSkippable}");
            Logger.Update("Score", $"Score: {GameModeComponent.ScoreProcessor.Score}");
            Logger.Update($"Accuracy", $"Accuracy: {GameModeComponent.ScoreProcessor.Accuracy}");
            Logger.Update($"Combo", $"Combo: {GameModeComponent.ScoreProcessor.Combo}");
            Logger.Update($"Max Combo", $"Max Combo: {GameModeComponent.ScoreProcessor.MaxCombo}");
            Logger.Update($"Objects Left", $"Objects Left {GameModeComponent.HitObjectManager.ObjectsLeft}");
            Logger.Update($"Finished", $"Finished: {GameModeComponent.HitObjectManager.IsComplete}");*/
            
            GameBase.SpriteBatch.End();
        }
        
#region INPUT               
        /// <summary>
        ///     Handles the input of the game + individual game modes.
        /// </summary>
        /// <param name="dt"></param>
        private void HandleInput(double dt)
        {
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeyPause.Value))
                Pause();
            
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeySkipIntro.Value))
                SkipSongIntro();
            
            // Restart map.
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeyRestartMap.Value))
                GameBase.GameStateManager.ChangeState(new GameplayScreen(Map, MapHash));
            
            // Don't handle actually gameplay specific input if the game is paused.
            if (Paused)
                return;
            
            // Handle input per game mode.
            GameModeComponent.HandleInput(dt);
        }

        /// <summary>
        ///     Pauses the game.
        /// </summary>
        private void Pause()
        {            
            // Handle pause.
            if (!Paused || ResumeInProgress)
            {
                Paused = true;
                ResumeInProgress = false;
                
                try
                {
                    GameBase.AudioEngine.Pause();
                }
                catch (AudioEngineException e) {}

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
            if (GameBase.GameTime.ElapsedMilliseconds - ResumeTime > AudioTiming.StartDelay)
            {
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
        }

        /// <summary>
       ///     Skips the song intro to 3 seconds before the first note.
       /// </summary>
        private void SkipSongIntro()
        {
            if (!IntroSkippable || Paused || ResumeInProgress)
                return;

            var skipTime = GameBase.SelectedMap.Qua.HitObjects[0].StartTime - AudioTiming.StartDelay + AudioEngine.BassDelayOffset;

            try
            {
                // Skip to the time if the audio already played once. If it hasn't, then play it.
                if (GameBase.AudioEngine.HasPlayed)
                    GameBase.AudioEngine.ChangeSongPosition(skipTime);
                else
                    GameBase.AudioEngine.Play(skipTime);

                // Set the actual song time to the position in the audio if it was successful.
                AudioTiming.CurrentTime = GameBase.AudioEngine.Position;
            }
            catch (AudioEngineException ex)
            {
                Logger.LogWarning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);

                // If there is no audio file, make sure the actual song time is set to the skip time.
                const int actualSongTimeOffset = 10000; // The offset between the actual song time and audio position (?)
                AudioTiming.CurrentTime = skipTime + actualSongTimeOffset;
            }
            finally
            {
                // Skip to 3 seconds before the notes start
                DiscordController.ChangeDiscordPresenceGameplay(true);
            }
        }   
 #endregion
        /// <summary>
        ///     Checks if the window is currently active and pauses the game if it isn't.
        /// </summary>
        private void PauseIfWindowInactive()
        {
            if (Paused)
                return;
            
            // Pause the game
            if (!QuaverGame.Game.IsActive)
                Pause();
        }

        /// <summary>
        ///     Plays a combo break sound if we've 
        /// </summary>
        private void PlayComboBreakSound()
        {
            if (LastRecordedCombo >= 20 && GameModeComponent.ScoreProcessor.Combo == 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundComboBreak);

            LastRecordedCombo = GameModeComponent.ScoreProcessor.Combo;
        }
    }
}