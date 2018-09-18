using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Screens.Gameplay.Replays;
using Quaver.Screens.Gameplay.Rulesets;
using Quaver.Screens.Gameplay.Rulesets.Input;
using Quaver.Screens.Gameplay.Rulesets.Keys;
using Quaver.Skinning;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Discord;
using Wobble.Discord.RPC;
using Wobble.Graphics.Transformations;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Screens.Gameplay
{
    public class GameplayScreen : Screen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///     The audio timing for the map.
        /// </summary>
        public GameplayAudioTiming Timing { get; }

        /// <summary>
        ///     The ruleset for the game mode of the map.
        /// </summary>
        public GameplayRuleset Ruleset { get; private set; }

        /// <summary>
        ///     The current map being played.
        /// </summary>
        public Qua Map { get; }

        /// <summary>
        ///     The list of scores displayed on the leaderboard.
        /// </summary>
        public List<LocalScore> LocalScores { get; }

        /// <summary>
        ///     The MD5 hash of the map being played.
        /// </summary>
        public string MapHash { get; }

        /// <summary>
        ///     The current replay being watched (if there is one.)
        /// </summary>
        public Replay LoadedReplay { get; }

        /// <summary>
        ///     If we are currently viewing a replay.
        /// </summary>
        public bool InReplayMode { get; }

        /// <summary>
        ///     Determines if the gameplay has actually started.
        /// </summary>
        public bool HasStarted { get; set; }

        /// <summary>
        ///     If the game is paused.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        ///     The amount of times the user requested to quit.
        /// </summary>
        private int TimesRequestedToPause { get; set; }

        /// <summary>
        ///     The amount of time the pause key has been held.
        /// </summary>
        public double TimePauseKeyHeld { get; private set; }

        /// <summary>
        ///     The amount of time it takes to hold the pause button in order to pause.
        /// </summary>
        public int TimeToHoldPause { get; } = 500;

        /// <summary>
        ///     The time the user resumed the game.
        /// </summary>
        private long ResumeTime { get; set; }

        /// <summary>
        ///     Dictates if we are currently resuming the game.
        /// </summary>
        public bool IsResumeInProgress { get; private set; }

        /// <summary>
        ///     If the play was failed (0 health)
        /// </summary>
        public bool Failed => Ruleset.ScoreProcessor.Health <= 0 || ForceFail;

        /// <summary>
        ///     If we're force failing the user.
        /// </summary>
        public bool ForceFail { get; set; }

        /// <summary>
        ///     Flag that makes sure the failure sound only gets played once.
        /// </summary>
        private bool FailureHandled { get; set; }

        /// <summary>
        ///     If the play is finished.
        /// </summary>
        public bool IsPlayComplete => Ruleset.HitObjectManager.IsComplete;

        /// <summary>
        ///     If the user quit the game themselves.
        /// </summary>
        public bool HasQuit { get; set; }

        /// <summary>
        ///     Flag that dictates if the user is currently restarting the play.
        /// </summary>
        public bool IsRestartingPlay { get; private set; }

        /// <summary>
        ///     The amount of time the restart key has been held down for.
        /// </summary>
        private double RestartKeyHoldTime { get; set; }

        /// <summary>
        ///     When the play is either failed or completed, this is a counter that
        ///     will increase and dictates when to go to the results screen.
        /// </summary>
        public double TimeSincePlayEnded { get; set; }

        /// <summary>
        ///     If the user is currently on a break in the song.
        /// </summary>
        private bool _onBreak;
        public bool OnBreak
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
                if (nextObject.TrueStartTime - Timing.Time >= GameplayAudioTiming.StartDelay + 5000)
                    _onBreak = true;
                // If the user is already on a break, then we need to turn the break off if the next object is at the start delay.
                else if (_onBreak && nextObject.TrueStartTime - Timing.Time <= GameplayAudioTiming.StartDelay)
                    _onBreak = false;

                return _onBreak;
            }
        }

        /// <summary>
        ///     The amount of times the user has paused.
        /// </summary>
        public int PauseCount { get; private set; }

        /// <summary>
        ///     The last recorded combo. We use this value for combo breaking.
        /// </summary>
        public int LastRecordedCombo { get; private set; }

        /// <summary>
        ///     The current replay for this gameplay session.
        /// </summary>
        public ReplayCapturer ReplayCapturer { get; }

        /// <summary>
        ///     The time the score began.
        /// </summary>
        public long TimePlayed { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="map"></param>
        /// <param name="md5"></param>
        /// <param name="scores"></param>
        /// <param name="replay"></param>
        public GameplayScreen(Qua map, string md5, List<LocalScore> scores, Replay replay = null)
        {
            TimePlayed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            Map = map;
            LocalScores = scores;
            MapHash = md5;
            LoadedReplay = replay;

            Timing = new GameplayAudioTiming(this);

            // Remove paused modifier if enabled.
            if (ModManager.IsActivated(ModIdentifier.Paused))
                ModManager.RemoveMod(ModIdentifier.Paused);

            // Handle autoplay replays.
            if (ModManager.IsActivated(ModIdentifier.Autoplay))
                LoadedReplay = ReplayHelper.GeneratePerfectReplay(map, MapHash);

            // Determine if we're in replay mode.
            if (LoadedReplay != null)
            {
                InReplayMode = true;
                AddModsFromReplay();
            }

            // Create the current replay that will be captured.
            ReplayCapturer = new ReplayCapturer(this);

            SetRuleset();
            SetRichPresence();

            AudioTrack.AllowPlayback = true;
            View = new GameplayScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Timing.Update(gameTime);


            if (!Failed && !IsPlayComplete)
            {
                HandleResuming();
                PlayComboBreakSound();
            }

            HandleInput(gameTime);
            HandleFailure();
            ReplayCapturer.Capture(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles the input of the game + individual game modes.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Handle pausing
            if (!Failed && !IsPlayComplete)
                HandlePauseInput(gameTime);

            // Show/hide scoreboard.
            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyScoreboardVisible.Value))
                ConfigManager.ScoreboardVisible.Value = !ConfigManager.ScoreboardVisible.Value;

            // Everything after this point is applicable to gameplay ONLY.
            if (IsPaused || Failed)
                return;

            if (!IsPlayComplete)
            {
                // Handle the restarting of the map.
                HandlePlayRestart(dt);

                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeySkipIntro.Value))
                    SkipToNextObject();

                // Only allow offset changes if the map hasn't started or if we're on a break
                if (Ruleset.Screen.Timing.Time <= 5000 || Ruleset.Screen.OnBreak)
                {
                    // Handle offset +
                    if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus))
                    {
                        MapManager.Selected.Value.LocalOffset += 5;
                        NotificationManager.Show(NotificationLevel.Success, $"Local map offset is now: {MapManager.Selected.Value.LocalOffset}ms");
                    }

                    // Handle offset -
                    if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus))
                    {
                        MapManager.Selected.Value.LocalOffset -= 5;
                        NotificationManager.Show(NotificationLevel.Success, $"Local map offset is now: {MapManager.Selected.Value.LocalOffset}ms");
                    }
                }
            }

            // Handle input per game mode.
            Ruleset.HandleInput(gameTime);
        }

        /// <summary>
        ///     Sets the ruleset for this current game mode.
        /// </summary>
        private void SetRuleset()
        {
            switch (Map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Ruleset = new GameplayRulesetKeys(this, Map);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        /// <summary>
        ///     Adds all modifiers that were present in the loaded replay (if there is one.)
        /// </summary>
        private void AddModsFromReplay()
        {
            // Add the correct mods on if we're in replay mode.
            if (!InReplayMode)
                return;

            // Remove all the current mods that we have on.
            ModManager.RemoveAllMods();

            // Put on the mods from the replay.);
            for (var i = 0; i <= Math.Log((int)LoadedReplay.Mods, 2); i++)
            {
                var mod = (ModIdentifier)Math.Pow(2, i);

                if (!LoadedReplay.Mods.HasFlag(mod))
                    continue;

                ModManager.AddMod(mod);
            }
        }

        /// <summary>
        ///     Handles the input for all pause input.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandlePauseInput(GameTime gameTime)
        {
            // User has the `No Pause` mod on, and they're requesting to exit.
            // OR
            // they have pressed the QuickExit key.
            if (ModManager.IsActivated(ModIdentifier.NoPause) &&
                (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyPause.Value) || KeyboardManager.IsUniqueKeyPress(Keys.Escape)) ||
                KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyQuickExit.Value))
            {
                HandleNoPauseExit();
            }
            // `No Pause` isn't activated, so handle pausing normally.
            else if (!IsPaused && !ModManager.IsActivated(ModIdentifier.NoPause) &&
                     (KeyboardManager.CurrentState.IsKeyDown(ConfigManager.KeyPause.Value) || KeyboardManager.CurrentState.IsKeyDown(Keys.Escape)))
            {
                Pause(gameTime);
            }
            // The user wants to resume their play.
            else if (IsPaused && !ModManager.IsActivated(ModIdentifier.NoPause) &&
                     (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyPause.Value) || KeyboardManager.IsUniqueKeyPress(Keys.Escape)))
            {
                Pause();
                TimePauseKeyHeld = 0;
            }
            else
            {
                TimePauseKeyHeld = 0;

                var screenView = (GameplayScreenView) View;

                if (Failed || IsPlayComplete || IsPaused)
                    return;

                // Properly fade in now.
                if (!screenView.FadingOnRestartKeyPress)
                {
                    screenView.Transitioner.Alpha = MathHelper.Lerp(screenView.Transitioner.Alpha, 0,
                        (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 120, 1));
                }
            }
        }

        /// <summary>
        ///     Pauses the game.
        /// </summary>
        internal void Pause(GameTime gameTime = null)
        {
            // Don't allow pausing if the play is already finished.
            if (IsPlayComplete)
                return;

            // Grab the casted version of the screenview.
            var screenView = (GameplayScreenView)View;

            // Handle pause.
            if (!IsPaused)
            {
                // Handle cases where someone (a developer) calls pause but there is not GameTime.
                // shouldn't ever happen though.
                if (gameTime == null)
                {
                    const string log = "Cannot pause if GameTime is null";
                    Logger.LogError(log, LogType.Runtime);

                    throw new InvalidOperationException(log);
                }

                // Increase the time the pause key has been held.
                TimePauseKeyHeld += gameTime.ElapsedGameTime.TotalMilliseconds;

                screenView.Transitioner.Alpha = MathHelper.Lerp(screenView.Transitioner.Alpha, 1,
                    (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / TimeToHoldPause, 1));

                // Make the user hold the pause key down before pausing.
                if (TimePauseKeyHeld < TimeToHoldPause)
                    return;

                IsPaused = true;
                IsResumeInProgress = false;
                PauseCount++;

                if (!InReplayMode)
                {
                    // Show notification to the user that their score is invalid.
                    NotificationManager.Show(NotificationLevel.Warning, "WARNING! Your score will not be submitted due to pausing during gameplay!");

                    // Add the pause mod to their score.
                    if (!ModManager.IsActivated(ModIdentifier.Paused))
                    {
                        ModManager.AddMod(ModIdentifier.Paused);
                        ReplayCapturer.Replay.Mods |= ModIdentifier.Paused;
                        Ruleset.ScoreProcessor.Mods |= ModIdentifier.Paused;
                    }
                }

                try
                {
                    AudioEngine.Track.Pause();
                }
                catch (AudioEngineException)
                {
                    // ignored
                }

                DiscordManager.Client.CurrentPresence.State = $"Paused for the {StringHelper.AddOrdinal(PauseCount)} time.";
                DiscordManager.Client.CurrentPresence.Timestamps = null;
                DiscordManager.Client.SetPresence(DiscordManager.Client.CurrentPresence);

                // Fade in the transitioner.
                screenView.Transitioner.Transformations.Clear();
                screenView.Transitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, screenView.Transitioner.Alpha, 0.75f, 400));

                // Activate pause menu
                screenView.PauseScreen.Activate();
                return;
            }


            if (IsResumeInProgress)
                return;

            // Setting the resume time in this case allows us to give the user time to react
            // with a delay before starting the audio track again.
            // When that resume time is past the specific set offset, it'll unpause the game.
            IsResumeInProgress = true;
            ResumeTime = GameBase.Game.TimeRunning;

            // Fade screen transitioner
            screenView.Transitioner.Transformations.Clear();
            var alphaTransformation = new Transformation(TransformationProperty.Alpha, Easing.Linear, 0.75f, 0, 400);
            screenView.Transitioner.Transformations.Add(alphaTransformation);

            // Deactivate pause screen.
            screenView.PauseScreen.Deactivate();
            SetRichPresence();
        }

        /// <summary>
        ///     Handles exiting the screen if the user has no pause on.
        /// </summary>
        private void HandleNoPauseExit()
        {
            if (InReplayMode && !Failed && !IsPlayComplete)
                return;

            TimesRequestedToPause++;

            // Force fail the user if they request to quit more than once.
            switch (TimesRequestedToPause)
            {
                case 1:
                    NotificationManager.Show(NotificationLevel.Warning, "Press the exit button once more to quit.");
                    break;
                default:
                    ForceFail = true;
                    HasQuit = true;

                    var view = (GameplayScreenView) View;
                    view.Transitioner.Transformations.Clear();
                    break;
            }
        }

        /// <summary>
        ///     Plays a combo break sound if we've
        /// </summary>
        private void PlayComboBreakSound()
        {
            if (LastRecordedCombo >= 20 && Ruleset.ScoreProcessor.Combo == 0)
                SkinManager.Skin.SoundComboBreak.CreateChannel().Play();

            LastRecordedCombo = Ruleset.ScoreProcessor.Combo;
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
            if (GameBase.Game.TimeRunning - ResumeTime > 800)
            {
                // Unpause the game and reset the resume in progress.
                IsPaused = false;
                IsResumeInProgress = false;

                // Resume the game audio stream.
                try
                {
                    // If the track already played, then we'll want to continue it.
                    // this check is necessary if the user paused before the track started.
                    if (HasStarted)
                        AudioEngine.Track.Play();
                }
                catch (AudioEngineException)
                {
                    // ignored
                }
            }

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
                AudioEngine.Track.Pause();
            }
            // No need to handle this exception.
            catch (AudioEngineException)
            {
                // ignored
            }

            // Play failure sound.
            SkinManager.Skin.SoundFailure.CreateChannel().Play();

            FailureHandled = true;
        }

        /// <summary>
        ///     Restarts the game if the user is holding down the key for a specified amount of time
        ///
        /// </summary>
        private void HandlePlayRestart(double dt)
        {
            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyRestartMap.Value))
                IsRestartingPlay = true;

            // Grab a reference to the ScreenView.
            var screenView = (GameplayScreenView)View;

            if (KeyboardManager.CurrentState.IsKeyDown(ConfigManager.KeyRestartMap.Value) && IsRestartingPlay)
            {
                RestartKeyHoldTime += dt;

                // Fade in the transitioner.
                if (!screenView.FadingOnRestartKeyPress)
                {
                    screenView.FadingOnRestartKeyPress = true;
                    screenView.FadingOnRestartKeyRelease = false;

                    screenView.Transitioner.Transformations.Clear();
                    screenView.Transitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear,
                        screenView.Transitioner.Alpha, 1, 100));
                }

                // Restart the map if the user has held it down for
                if (RestartKeyHoldTime >= 200)
                {
                    SkinManager.Skin.SoundRetry.CreateChannel().Play();

                    if (InReplayMode)
                        ScreenManager.ChangeScreen(new GameplayScreen(Map, MapHash, LocalScores, LoadedReplay));
                    else
                        ScreenManager.ChangeScreen(new GameplayScreen(Map, MapHash, LocalScores));
                }

                return;
            }

            RestartKeyHoldTime = 0;
            IsRestartingPlay = false;

            // Set it so that it's not fading in on restart anymore.
            if (!screenView.FadingOnRestartKeyRelease && screenView.FadingOnRestartKeyPress)
            {
                screenView.FadingOnRestartKeyPress = false;
                screenView.FadingOnRestartKeyRelease = true;

                screenView.Transitioner.Transformations.Clear();
                screenView.Transitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 200));
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
            var skipTime = Ruleset.HitObjectManager.ObjectPool.First().TrueStartTime - GameplayAudioTiming.StartDelay;

            try
            {
                // Skip to the time if the audio already played once. If it hasn't, then play it.
                AudioEngine.Track.Seek(skipTime);
                AudioEngine.Track.Play();

                // Set the actual song time to the position in the audio if it was successful.
                Timing.Time = AudioEngine.Track.Time;
            }
            catch (AudioEngineException)
            {
                Logger.LogWarning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);

                // If there is no audio file, make sure the actual song time is set to the skip time.
                const int actualSongTimeOffset = 10000; // The offset between the actual song time and audio position (?)
                Timing.Time = skipTime + actualSongTimeOffset;
            }
            finally
            {
                if (InReplayMode)
                {
                    var inputManager = (KeysInputManager)Ruleset.InputManager;
                    inputManager.ReplayInputManager.HandleSkip();
                }
            }
        }

        /// <summary>
        ///     Sets rich presence based on which activity we're doing in gameplay.
        /// </summary>
        private void SetRichPresence()
        {
            var presence = DiscordManager.Client.CurrentPresence;

            presence.Details = Map.ToString();

            if (InReplayMode)
                presence.State = $"Watching {LoadedReplay.PlayerName}";
            else
                presence.State = $"Playing {(ModManager.Mods > 0 ? "+ " + ModHelper.GetModsString(ModManager.Mods) : "")}";

            presence.Timestamps = new Timestamps
            {
                End = DateTime.UtcNow.AddMilliseconds((Map.Length - Timing.Time) / AudioEngine.Track.Rate)
            };

            DiscordManager.Client.SetPresence(presence);
        }
    }
}
