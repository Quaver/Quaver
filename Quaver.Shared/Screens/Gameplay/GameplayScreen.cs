/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Gameplay.Replays;
using Quaver.Shared.Screens.Gameplay.Rulesets;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio;
using Wobble.Audio.Tracks;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Screens;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay
{
    public class GameplayScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Gameplay;

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
        ///     If test playing, a new .qua is made with only objects from that section of the map
        ///     It is set to <see cref="Map"/>. This keeps the original map, so we can use it
        ///     to go back to the editor after testing.
        /// </summary>
        public Qua OriginalEditorMap { get; }

        /// <summary>
        ///     The list of scores displayed on the leaderboard.
        /// </summary>
        public List<Score> LocalScores { get; }

        /// <summary>
        ///     The MD5 hash of the map being played.
        /// </summary>
        public string MapHash { get; }

        /// <summary>
        ///     The current replay being watched (if there is one.)
        /// </summary>
        public Replay LoadedReplay { get; private set; }

        /// <summary>
        ///     If we are currently viewing a replay.
        /// </summary>
        public bool InReplayMode { get; set; }

        /// <summary>
        ///     If we're currently in play test mode.
        /// </summary>
        public bool IsPlayTesting { get; }

        /// <summary>
        ///     The time in the audio the play test began.
        ///     Used for retries
        /// </summary>
        public double PlayTestAudioTime { get; }

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
        public bool Failed => !IsPlayTesting && (!ModManager.IsActivated(ModIdentifier.NoFail) && Ruleset.ScoreProcessor.Health <= 0) || ForceFail;

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
        ///     If the user is eligible to skip to the next object.
        /// </summary>
        public bool EligibleToSkip => Map.HitObjects.First().StartTime - Ruleset.Screen.Timing.Time >= GameplayAudioTiming.StartDelay + 5000;

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
        /// <param name="isPlayTesting"></param>
        /// <param name="playTestTime"></param>
        public GameplayScreen(Qua map, string md5, List<Score> scores, Replay replay = null, bool isPlayTesting = false, double playTestTime = 0)
        {
            TimePlayed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (isPlayTesting)
            {
                var testingQua = ObjectHelper.DeepClone(map);
                testingQua.HitObjects.RemoveAll(x => x.StartTime < playTestTime);

                Map = testingQua;
                OriginalEditorMap = map;
            }
            else
                Map = map;

            LocalScores = scores;
            MapHash = md5;
            LoadedReplay = replay;
            IsPlayTesting = isPlayTesting;
            PlayTestAudioTime = playTestTime;

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
            if (Exiting)
                return;

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

                if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyQuickExit.Value))
                    HandleQuickExit();

                // Go back to editor at the same time
                if (IsPlayTesting && KeyboardManager.IsUniqueKeyPress(Keys.F2))
                {
                    if (AudioEngine.Track.IsPlaying)
                        AudioEngine.Track.Pause();

                    Exit(() => new EditorScreen(OriginalEditorMap));
                }

                // Handle play test autoplay input.
                if (IsPlayTesting && KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                {
                    var inputManager = (KeysInputManager) Ruleset.InputManager;

                    if (LoadedReplay == null)
                    {
                        LoadedReplay = ReplayHelper.GeneratePerfectReplay(Map, MapHash);
                        inputManager.ReplayInputManager = new ReplayInputManagerKeys(this);
                        inputManager.ReplayInputManager.HandleSkip();
                        inputManager.ReplayInputManager.CurrentFrame++;
                    }

                    InReplayMode = !InReplayMode;
                    inputManager.ReplayInputManager.HandleSkip();
                    inputManager.ReplayInputManager.CurrentFrame++;

                    if (!InReplayMode)
                    {
                        for (var i = 0; i < Map.GetKeyCount(); i++)
                        {
                            inputManager.ReplayInputManager.UniquePresses[i] = false;
                            inputManager.ReplayInputManager.UniqueReleases[i] = true;
                            inputManager.BindingStore[i].Pressed = false;

                            var playfield = (GameplayPlayfieldKeys) Ruleset.Playfield;
                            playfield.Stage.HitLightingObjects[i].StopHolding();
                            playfield.Stage.SetReceptorAndLightingActivity(i, inputManager.BindingStore[i].Pressed);
                        }

                        inputManager.HandleInput(gameTime.ElapsedGameTime.TotalMilliseconds);
                    }

                    NotificationManager.Show(NotificationLevel.Info, $"Autoplay has been turned {(InReplayMode ? "on" : "off")}");
                }

                // Only allow offset changes if the map hasn't started or if we're on a break
                if (Ruleset.Screen.Timing.Time <= 5000 || Ruleset.Screen.EligibleToSkip)
                {
                    // Handle offset +
                    if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyIncreaseMapOffset.Value))
                    {
                        MapManager.Selected.Value.LocalOffset += 5;
                        NotificationManager.Show(NotificationLevel.Success, $"Local map offset is now: {MapManager.Selected.Value.LocalOffset}ms");
                        MapDatabaseCache.UpdateMap(MapManager.Selected.Value);
                    }

                    // Handle offset -
                    if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyDecreaseMapOffset.Value))
                    {
                        MapManager.Selected.Value.LocalOffset -= 5;
                        NotificationManager.Show(NotificationLevel.Success, $"Local map offset is now: {MapManager.Selected.Value.LocalOffset}ms");
                        MapDatabaseCache.UpdateMap(MapManager.Selected.Value);
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
        ///     Handles the input for all pause input.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandlePauseInput(GameTime gameTime)
        {
            // Go back to editor if we're currently play testing.
            if (IsPlayTesting && (KeyboardManager.IsUniqueKeyPress(Keys.Escape) || KeyboardManager.CurrentState.IsKeyDown(ConfigManager.KeyPause.Value)))
            {
                if (AudioEngine.Track.IsPlaying)
                {
                    AudioEngine.Track.Pause();
                    AudioEngine.Track.Seek(PlayTestAudioTime);
                }

                Exit(() => new EditorScreen(OriginalEditorMap));
            }

            if (!IsPaused && (KeyboardManager.CurrentState.IsKeyDown(ConfigManager.KeyPause.Value) || KeyboardManager.CurrentState.IsKeyDown(Keys.Escape)))
                Pause(gameTime);
            // The user wants to resume their play.
            else if (IsPaused && (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyPause.Value) || KeyboardManager.IsUniqueKeyPress(Keys.Escape)))
            {
                if (ChatManager.IsActive)
                {
                    ChatManager.ToggleChatOverlay();
                    return;
                }

                Pause();
                TimePauseKeyHeld = 0;
                GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
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
                    Logger.Error(log, LogType.Runtime);
                    throw new InvalidOperationException(log);
                }

                // Increase the time the pause key has been held.
                TimePauseKeyHeld += gameTime.ElapsedGameTime.TotalMilliseconds;

                screenView.Transitioner.Alpha = MathHelper.Lerp(screenView.Transitioner.Alpha, 1,
                    (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / TimeToHoldPause, 1));

                // Make the user hold the pause key down before pausing if tap to pause is disabled.
                if (!ConfigManager.TapToPause.Value && TimePauseKeyHeld < TimeToHoldPause)
                    return;

                IsPaused = true;
                IsResumeInProgress = false;
                PauseCount++;
                GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

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
                catch (Exception)
                {
                    // ignored
                }

                DiscordHelper.Presence.State = $"Paused for the {StringHelper.AddOrdinal(PauseCount)} time";
                DiscordHelper.Presence.EndTimestamp = 0;
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

                OnlineManager.Client?.UpdateClientStatus(GetClientStatus());

                // Fade in the transitioner.
                screenView.Transitioner.Animations.Clear();
                screenView.Transitioner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, screenView.Transitioner.Alpha, 0.75f, 400));

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
            screenView.Transitioner.Animations.Clear();
            var alphaTransformation = new Animation(AnimationProperty.Alpha, Easing.Linear, 0.75f, 0, 400);
            screenView.Transitioner.Animations.Add(alphaTransformation);

            // Deactivate pause screen.
            screenView.PauseScreen.Deactivate();
            SetRichPresence();
            OnlineManager.Client?.UpdateClientStatus(GetClientStatus());
        }

        /// <summary>
        ///     Handles exiting the screen if the user has no pause on.
        /// </summary>
        private void HandleQuickExit()
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
                    view.Transitioner.Animations.Clear();
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
                catch (Exception)
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
            catch (Exception)
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

                    screenView.Transitioner.Animations.Clear();
                    screenView.Transitioner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear,
                        screenView.Transitioner.Alpha, 1, 100));
                }

                // Restart the map if the user has held it down for
                if (RestartKeyHoldTime >= 200)
                {
                    SkinManager.Skin.SoundRetry.CreateChannel().Play();
                    Retry();
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

                screenView.Transitioner.Animations.Clear();
                screenView.Transitioner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 1, 0, 200));
            }
        }

        /// <summary>
        ///     Will restart the screen appropriately.
        /// </summary>
        public void Retry()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
            SkinManager.Skin.SoundRetry.CreateChannel().Play();

            // Use ChangeScreen here to give instant feedback. Can't be threaded
            if (IsPlayTesting)
                QuaverScreenManager.ChangeScreen(new GameplayScreen(OriginalEditorMap, MapHash, LocalScores, null, true, PlayTestAudioTime));
            else if (InReplayMode)
                QuaverScreenManager.ChangeScreen(new GameplayScreen(Map, MapHash, LocalScores, LoadedReplay));
            else
                QuaverScreenManager.ChangeScreen(new GameplayScreen(Map, MapHash, LocalScores));
        }

        /// <summary>
        ///     Skips the song to the next object if on a break.
        /// </summary>
        private void SkipToNextObject()
        {
            if (!EligibleToSkip || IsPaused || IsResumeInProgress)
                return;

            // Get the skip time of the next object.
            var nextObject = Ruleset.HitObjectManager.NextHitObject.StartTime;
            var skipTime = nextObject - GameplayAudioTiming.StartDelay * ModHelper.GetRateFromMods(ModManager.Mods);

            try
            {
                // Skip to the time if the audio already played once. If it hasn't, then play it.
                AudioEngine.Track?.Seek(skipTime);
                Timing.Time = AudioEngine.Track.Time;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                Logger.Warning("Trying to skip with no audio file loaded. Still continuing..", LogType.Runtime);
                Timing.Time = skipTime;
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
            DiscordHelper.Presence.Details = Map.ToString();

            if (IsPlayTesting)
                DiscordHelper.Presence.State = "Play Testing";
            else if (InReplayMode)
                DiscordHelper.Presence.State = $"Watching {LoadedReplay.PlayerName}";
            else
                DiscordHelper.Presence.State = $"Playing {(ModManager.Mods > 0 ? "+ " + ModHelper.GetModsString(ModManager.Mods) : "")}";

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = Convert.ToInt64((DateTime.UtcNow.AddMilliseconds((Map.Length - Timing.Time) / AudioEngine.Track.Rate) - epoch).TotalSeconds);

            DiscordHelper.Presence.EndTimestamp = time;
            DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(Ruleset.Mode);
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus()
        {
            ClientStatus status;

            string content;

            if (IsPlayTesting)
            {
                status = ClientStatus.Playing;
                content = Map.ToString();
            }
            else if (InReplayMode)
            {
                status = ClientStatus.Watching;
                content = LoadedReplay.PlayerName;
            }
            else if (IsResumeInProgress)
            {
                status = ClientStatus.Playing;
                content = Map.ToString();
            }
            else if (IsPaused)
            {
                status = ClientStatus.Paused;
                content = "";
            }
            else
            {
                status = ClientStatus.Playing;
                content = Map.ToString();
            }

            return new UserClientStatus(status, Map.MapId, MapHash, (byte) Ruleset.Mode, content, (long) ModManager.Mods);
        }
    }
}
