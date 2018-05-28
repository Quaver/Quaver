using System;
using System.ComponentModel;
using System.Linq;
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
using Quaver.Modifiers;
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
        public State CurrentState { get; set; } = State.Gameplay;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     The specific audio timimg for this gameplay state.
        /// </summary>
        internal GameplayTiming Timing { get; }

        /// <summary>
        ///     The curent game mode ruleset
        /// </summary>
        internal GameModeRuleset Ruleset { get; }

        /// <summary>
        ///     The general gameplay UI.
        /// </summary>
        private GameplayInterface UI { get; }

        /// <summary>
        ///     If the game is currently paused.
        /// </summary>
        internal bool IsPaused { get; private set; }

        /// <summary>
        ///     The amount of times the user has paused.
        /// </summary>
        private int PauseCounter { get; set; }

        /// <summary>
        ///     If the game session has already been started.
        /// </summary>
        internal bool HasStarted { get; set; }

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
        internal bool IsResumeInProgress { get; private set; }

        /// <summary>
        ///     The time the user resumed the game.
        /// </summary>
        private long ResumeTime { get; set; }

        /// <summary>
        ///     The last recorded combo. We use this value for combo breaking.
        /// </summary>
        private int LastRecordedCombo { get; set; }

        /// <summary>
        ///     If the user is currently on a break in the song.
        /// </summary>
        private bool _onBreak;
        internal bool OnBreak
        {
            get
            {
                // By default if there aren't any objects left we aren't on a break.
                if (Ruleset.HitObjectManager.ObjectPool.Count <= 0) 
                    return false;

                // Grab the next object in the object pool.
                var nextObject = Ruleset.HitObjectManager.ObjectPool.First();
                
                // If the player is currently not on a break, then we want to detect if it's on a break
                // by checking if the next object is 10 seconds away.
                if (nextObject.TrueStartTime - Timing.CurrentTime >= Timing.StartDelay + 10000)
                    _onBreak = true;                 
                // If the user is already on a break, then we need to turn the break off if the next object is at the start delay.
                else if (_onBreak && nextObject.TrueStartTime - Timing.CurrentTime <= Timing.StartDelay)
                    _onBreak = false;

                return _onBreak;
            }
        }

        /// <summary>
        ///     If the play is finished.
        /// </summary>
        internal bool IsPlayComplete => Ruleset.HitObjectManager.IsComplete;

        /// <summary>
        ///     If the play was failed (0 health)
        /// </summary>
        internal bool Failed => Ruleset.ScoreProcessor.Health <= 0;

        /// <summary>
        ///     Flag that makes sure the failure sound only gets played once.
        /// </summary>
        private bool FailureHandled { get; set; }

        /// <summary>
        ///     The amount of time the restart key has been held down for.
        /// </summary>
        private double RestartKeyHoldTime { get; set; }

        /// <summary>
        ///     Flag that dictates if the user is currently restarting the play.
        /// </summary>
        internal bool IsRestartingPlay { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal GameplayScreen(Qua map, string md5)
        {
            Map = map;
            MapHash = md5;
            
            Timing = new GameplayTiming(this);
            UI = new GameplayInterface(this);
            
            // Set the game mode component.
            switch (map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Ruleset = new GameModeRulesetKeys(this, map.Mode, map);
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
            Timing.Initialize(this);
            UI.Initialize(this);
            
            // Change discord rich presence.
            DiscordController.ChangeDiscordPresenceGameplay(false);
            
            // Initialize the game mode.
            Ruleset.Initialize();
            
            // Add gameplay loggers
            Logger.Add("Paused", $"Paused: {IsPaused}", Color.White);
            Logger.Add("Resume In Progress", $"Resume In Progress {IsResumeInProgress}", Color.White);
            Logger.Add($"Max Combo", $"Max Combo: {Ruleset.ScoreProcessor.MaxCombo}", Color.White);
            Logger.Add($"Objects Left", $"Objects Left {Ruleset.HitObjectManager.ObjectsLeft}", Color.White);
            Logger.Add($"Finished", $"Finished: {IsPlayComplete}", Color.White);
            Logger.Add($"On Break", $"On Break: {OnBreak}", Color.White);
            Logger.Add($"Failed", $"Failed: {Failed}", Color.White);
               
            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UnloadContent()
        {
            Timing.UnloadContent();
            UI.UnloadContent();
            Ruleset.Destroy();
            Logger.Clear();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            UI.Update(dt);

            if (!Failed && !IsPlayComplete)
            {
                Timing.Update(dt);
                HandleResuming();
                PauseIfWindowInactive();
                PlayComboBreakSound();
            }

            HandleInput(dt);
            HandleFailure();
            Ruleset.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Ruleset.Draw();
            UI.Draw();
            
            // Update loggers.
            Logger.Update("Paused", $"Paused: {IsPaused}");
            Logger.Update("Resume In Progress", $"Resume In Progress {IsResumeInProgress}");
            Logger.Update($"Max Combo", $"Max Combo: {Ruleset.ScoreProcessor.MaxCombo}");
            Logger.Update($"Objects Left", $"Objects Left {Ruleset.HitObjectManager.ObjectsLeft}");
            Logger.Update($"Finished", $"Finished: {IsPlayComplete}");
            Logger.Update($"On Break", $"On Break: {OnBreak}");
            Logger.Update($"Failed", $"Failed: {Failed}");
            
            GameBase.SpriteBatch.End();
        }
        
#region INPUT               
        /// <summary>
        ///     Handles the input of the game + individual game modes.
        /// </summary>
        /// <param name="dt"></param>
        private void HandleInput(double dt)
        {
            if (!Failed && !IsPlayComplete && InputHelper.IsUniqueKeyPress(ConfigManager.KeyPause.Value))
                Pause();
                        
            // Don't handle actually gameplay specific input if the game is paused.
            if (IsPaused || Failed || IsPlayComplete)
                return;
            
            // Handle the restarting of the map.
            HandlePlayRestart(dt);
            
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeySkipIntro.Value))
                SkipToNextObject();
            
            // Handle input per game mode.
            Ruleset.HandleInput(dt);
        }

        /// <summary>
        ///     Pauses the game.
        /// </summary>
        private void Pause()
        {
            // Don't allow any sort of pausing if the play is already finished.
            if (IsPlayComplete)
                return;

            if (ModManager.IsActivated(ModIdentifier.NoPause))
            {
                Logger.LogImportant($"You cannot pause while the no-pause mod is activated!", LogType.Runtime);
                return;
            }
            
            // Handle pause.
            if (!IsPaused || IsResumeInProgress)
            {
                IsPaused = true;
                IsResumeInProgress = false;
                PauseCounter++;
                
                DiscordController.ChangeDiscordPresence($"{Map.Artist} - {Map.Title} [{Map.DifficultyName}]", $"Paused for the {StringHelper.AddOrdinal(PauseCounter)} time");
                
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
            IsResumeInProgress = true;
            ResumeTime = GameBase.GameTime.ElapsedMilliseconds;
            DiscordController.ChangeDiscordPresenceGameplay(true);
        }

        /// <summary>
        ///     Handles resuming of the game.
        ///     Essentially gives a delay before starting the game back up.
        /// </summary>
        private void HandleResuming()
        {
            if (!IsPaused || !IsResumeInProgress)
                return;

            // We don't want to resume if the time difference isn't at least or greter than the start delay.
            if (GameBase.GameTime.ElapsedMilliseconds - ResumeTime > 800)
            {
                // Unpause the game and reset the resume in progress.
                IsPaused = false;
                IsResumeInProgress = false;
            
                // Resume the game audio stream.
                try
                {
                    GameBase.AudioEngine.Resume();
                } 
                catch (AudioEngineException e) {}
            }
        }

        /// <summary>
       ///     Skips the song to the next object if on a break.
       /// </summary>
        private void SkipToNextObject()
        {
            if (!OnBreak || IsPaused || IsResumeInProgress)
                return;

            // Get the skip time of the next object.           
            var skipTime = Ruleset.HitObjectManager.ObjectPool.First().TrueStartTime - Timing.StartDelay + AudioEngine.BassDelayOffset;

            try
            {
                // Skip to the time if the audio already played once. If it hasn't, then play it.
                if (GameBase.AudioEngine.HasPlayed)
                    GameBase.AudioEngine.ChangeSongPosition(skipTime);
                else
                    GameBase.AudioEngine.Play((int)skipTime);

                // Set the actual song time to the position in the audio if it was successful.
                Timing.CurrentTime = GameBase.AudioEngine.Position;
            }
            catch (AudioEngineException ex)
            {
                Logger.LogWarning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);

                // If there is no audio file, make sure the actual song time is set to the skip time.
                const int actualSongTimeOffset = 10000; // The offset between the actual song time and audio position (?)
                Timing.CurrentTime = skipTime + actualSongTimeOffset;
            }
            finally
            {
                // Skip to 3 seconds before the notes start
                DiscordController.ChangeDiscordPresenceGameplay(true);
            }
        }

        /// <summary>
        ///     Restarts the game if the user is holding down the key for a specified amount of time
        ///     
        /// </summary>
        private void HandlePlayRestart(double dt)
        {
            if (InputHelper.IsUniqueKeyPress(ConfigManager.KeyRestartMap.Value))
                IsRestartingPlay = true;
            
            if (GameBase.KeyboardState.IsKeyDown(ConfigManager.KeyRestartMap.Value) && IsRestartingPlay)
            {                
                RestartKeyHoldTime += dt;
                UI.ScreenTransitioner.FadeIn(dt, 60);
                
                // Restart the map if the user has held it down for 
                if (RestartKeyHoldTime >= 350)
                {
                    GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundScreenshot);
                    GameBase.GameStateManager.ChangeState(new GameplayScreen(Map, MapHash));
                }

                return;
            }

            RestartKeyHoldTime = 0;
            IsRestartingPlay = false;
        }
 #endregion
        
        /// <summary>
        ///     Checks if the window is currently active and pauses the game if it isn't.
        /// </summary>
        private void PauseIfWindowInactive()
        {
            if (IsPaused)
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
            if (LastRecordedCombo >= 20 && Ruleset.ScoreProcessor.Combo == 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundComboBreak);

            LastRecordedCombo = Ruleset.ScoreProcessor.Combo;
        }

        /// <summary>
        ///     Stops the music and begins the failure process.
        /// </summary>
        private void HandleFailure()
        {
            if (!Failed || FailureHandled)
                return;

            try
            {
                // Pause the audio if applicable.
                GameBase.AudioEngine.Pause();
            }
            // No need to handle this exception.
            catch (AudioEngineException e) {}
            
            // Play failure sound.
            GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundComboBreak);

            FailureHandled = true;
        }
    }
}