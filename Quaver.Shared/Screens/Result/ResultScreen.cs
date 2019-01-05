/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Modifiers.Mods;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Result.UI;
using Quaver.Shared.Screens.Select;
using Wobble;
using Wobble.Discord;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Result
{
    public class ResultScreen : QuaverScreen
    {
        /// <summary>
        ///     The type of results screen that this is for.
        /// </summary>
        public ResultScreenType ResultsType { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Results;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        ///    Reference to the gameplay screen, if the user was previous playing
        /// </summary>
        public GameplayScreen Gameplay { get; }

        /// <summary>
        ///     Reference to the score if the user is coming from a scoreboard score.
        /// </summary>
        public Score Score { get; }

        /// <summary>
        ///     Reference to the replay, if the user is coming from a replay file.
        /// </summary>
        public Replay Replay { get; private set; }

        /// <summary>
        ///     The ScoreProcesor that is used as a container for all scoring values
        /// </summary>
        public ScoreProcessor ScoreProcessor { get; private set; }

        /// <summary>
        ///     Reference to the map that was played for this score.
        /// </summary>
        public static Map Map => MapManager.Selected.Value;

        /// <summary>
        ///     Reference to the user's set scroll speed.
        /// </summary>
        private static int ScrollSpeed => Map.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;

        /// <summary>
        ///     The last time the replay was exported
        /// </summary>
        private long LastReplayExportTime { get; set; }

        /// <summary>
        ///     Song title + Difficulty name.
        /// </summary>
        public string SongTitle => $"{Map.Artist} - {Map.Title} [{Map.DifficultyName}]";

        /// <summary>
        ///     Dictates if we're in the midst of fetching an online replay for watching.
        /// </summary>
        public bool IsFetchingOnlineReplay { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="gameplay"></param>
        public ResultScreen(GameplayScreen gameplay)
        {
            Gameplay = gameplay;
            ResultsType = ResultScreenType.Gameplay;
            ScoreProcessor = Gameplay.Ruleset.ScoreProcessor;

            InitializeIfGameplayType();
            ChangeDiscordPresence();
            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public ResultScreen(Score score)
        {
            Score = score;
            ResultsType = ResultScreenType.Score;

            Replay = score.ToReplay();
            ScoreProcessor = new ScoreProcessorKeys(Replay);

            InitializeIfScoreType();
            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="replay"></param>
        public ResultScreen(Replay replay)
        {
            Replay = replay;
            ResultsType = ResultScreenType.Replay;
            ScoreProcessor = new ScoreProcessorKeys(replay);

            InitializeIfReplayType();
            ChangeDiscordPresence();

            View = new ResultScreenView(this);
        }

        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            MakeCursorVisible();

            if (ResultsType == ResultScreenType.Replay)
                AudioEngine.PlaySelectedTrackAtPreview();

            BackgroundHelper.Background.BrightnessSprite.ClearAnimations();
            BackgroundHelper.Background.BrightnessSprite.Alpha = 1;

            var view = View as ResultScreenView;
            view?.HandleBackgroundChange();

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles input for the entire screen.
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0 || Exiting)
                return;

            var view = View as ResultScreenView;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ExitToMenu();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                view?.ButtonContainer.ChangeSelectedButton(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                view?.ButtonContainer.ChangeSelectedButton(Direction.Forward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                view?.ButtonContainer.SelectedButton.FireButtonClickEvent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) GameMode.Keys4, "", 0);

        /// <summary>
        ///     Makes sure the cursor is visible, as it is usually hidden in gameplay)
        /// </summary>
        private void MakeCursorVisible()
        {
            var game = GameBase.Game as QuaverGame;
            var cursor = game?.GlobalUserInterface.Cursor;
            cursor.Alpha = 1;
        }

        /// <summary>
        ///     If we're coming from gameplay, the screen gets intiialized this way.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void InitializeIfGameplayType()
        {
            if (ResultsType != ResultScreenType.Gameplay)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Gameplay");

            // Stop `capturing` the replay. Not sure if this is necessary since update isn't even called and
            // it wouldn't be capturing anyway.
            Gameplay.ReplayCapturer.ShouldCapture = false;

            // Keep the same replay and score processor if the user was watching a replay before,
            if (Gameplay.InReplayMode)
            {
                Replay = Gameplay.LoadedReplay;
                ScoreProcessor = Replay.Mods.HasFlag(ModIdentifier.Autoplay) ? Gameplay.Ruleset.ScoreProcessor : new ScoreProcessorKeys(Replay);
                ScoreProcessor.Stats = Gameplay.Ruleset.ScoreProcessor.Stats;

                // Remove all modifiers that was played during the replay, so the user doesn't still have them
                // activated when they go back to select.
                ModManager.RemoveAllMods();
                return;
            }

            // User has played the map, and wasn't watching a replay, so we can proceed normally.

            // Set the replay that the user has generated
            Replay = Gameplay.ReplayCapturer.Replay;
            Replay.PauseCount = Gameplay.PauseCount;
            Replay.RandomizeModifierSeed = Gameplay.Map.RandomizeModifierSeed;
            ScoreProcessor = Gameplay.Ruleset.ScoreProcessor;
            Replay.FromScoreProcessor(ScoreProcessor);

            // Remove paused modifier if enabled.
            if (ModManager.IsActivated(ModIdentifier.Paused))
                ModManager.RemoveMod(ModIdentifier.Paused);

            SubmitScore();
        }

        /// <summary>
        ///     If we're coming from a `Score`, then the screen should be initialized with this method
        /// </summary>
        private void InitializeIfScoreType()
        {
            if (ResultsType != ResultScreenType.Score)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Score");
        }

        /// <summary>
        ///     If we're coming from a `replay`, then the screen should be initialized with this method
        /// </summary>
        private void InitializeIfReplayType()
        {
            if (ResultsType != ResultScreenType.Replay)
                throw new InvalidOperationException("Cannot call this if ResultsType is not Replay");
        }

        /// <summary>
        ///     Goes through the score submission process.
        ///     saves score to local database.
        /// </summary>
        private void SubmitScore()
        {
            // Don't save scores if the user quit themself.
            if (Gameplay.HasQuit || Gameplay.InReplayMode)
                return;

            // Don't submit scores at all if the user has ALL misses for their judgements.
            // They basically haven't actually played the map.
            if (ScoreProcessor.CurrentJudgements[Judgement.Miss] == ScoreProcessor.TotalJudgementCount)
                return;

            ThreadScheduler.Run(SaveLocalScore);

            // Don't submit scores if disconnected from the server completely.
            if (OnlineManager.Status.Value == ConnectionStatus.Disconnected)
                return;

            ThreadScheduler.Run(() =>
            {
                Logger.Important($"Beginning to submit score on map: {Gameplay.MapHash}", LogType.Network);

                OnlineManager.Client?.Submit(new OnlineScore(Gameplay.MapHash, Gameplay.ReplayCapturer.Replay,
                    ScoreProcessor, ScrollSpeed, ModHelper.GetRateFromMods(ModManager.Mods), TimeHelper.GetUnixTimestampMilliseconds(),
                    SteamManager.PTicket));
            });
        }

        /// <summary>
        ///     Saves a local score to the database.
        /// </summary>
        private void SaveLocalScore()
        {
            var scoreId = 0;

            try
            {
                var localScore = Score.FromScoreProcessor(ScoreProcessor, Gameplay.MapHash, ConfigManager.Username.Value, ScrollSpeed,
                    Gameplay.PauseCount, Gameplay.Map.RandomizeModifierSeed);

                localScore.RatingProcessorVersion = RatingProcessorKeys.Version;

                if (ScoreProcessor.Failed)
                    localScore.PerformanceRating = 0;
                else
                    localScore.PerformanceRating = new RatingProcessorKeys(Map.DifficultyFromMods(ScoreProcessor.Mods)).CalculateRating(ScoreProcessor.Accuracy);

                scoreId = ScoreDatabaseCache.InsertScoreIntoDatabase(localScore);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an error saving your score. Check Runtime.log for more details.");
                Logger.Error(e, LogType.Runtime);
            }

            try
            {
                Replay.Write($"{ConfigManager.DataDirectory}/r/{scoreId}.qr");
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an error when saving your replay. Check Runtime.log for more details.");
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Exits the screen back to the main menu
        /// </summary>
        public void ExitToMenu()
        {
            if (IsFetchingOnlineReplay)
                return;

            Exit(() => new SelectScreen());
        }

        /// <summary>
        ///     Exports the currently loaded replay to a file.
        /// </summary>
        public void ExportReplay()
        {
            if (IsFetchingOnlineReplay)
                return;

            Replay replay;

            try
            {
                switch (ResultsType)
                {
                    case ResultScreenType.Gameplay:
                    case ResultScreenType.Replay:
                        replay = Replay;
                        break;
                    case ResultScreenType.Score:
                        if (Score.IsOnline)
                        {
                            NotificationManager.Show(NotificationLevel.Warning, "You must watch the online replay before exporting it.");
                            return;
                        }

                        replay = new Replay($"{ConfigManager.DataDirectory.Value}/r/{Score.Id}.qr");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return;
            }

            // Run a check for if the replay has data
            if (!replay.HasData)
            {
                NotificationManager.Show(NotificationLevel.Error, "This replay has no data!");
                return;
            }

            // Don't allow autoplay replays to be exported
            if (replay.Mods.HasFlag(ModIdentifier.Autoplay))
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot export replays that have the Autoplay mod");
                return;
            }

            // Make sure the user isn't spamming the export button and proceed to export
            if (GameBase.Game.TimeRunning - LastReplayExportTime >= 5000 || LastReplayExportTime == 0)
            {
                NotificationManager.Show(NotificationLevel.Info, "One moment. The replay is getting exported.");
                LastReplayExportTime = GameBase.Game.TimeRunning;

                ThreadScheduler.Run(() =>
                {
                    var path = $@"{ConfigManager.ReplayDirectory.Value}/{Replay.PlayerName} - {StringHelper.FileNameSafeString(SongTitle)} - {DateTime.Now:yyyyddMMhhmmss}{GameBase.Game.TimeRunning}.qr";
                    Replay.Write(path);

                    // Open containing folder
                    Utils.NativeUtils.HighlightInFileManager(path);
                    NotificationManager.Show(NotificationLevel.Success, "The replay has been successfully exported!");
                });
            }
            else
            {
                NotificationManager.Show(NotificationLevel.Error, "Please wait a few moments before exporting again!");
                return;
            }
        }

        /// <summary>
        ///     Exits the screen to watch a replay
        /// </summary>
        public void ExitToWatchReplay()
        {
            if (IsFetchingOnlineReplay)
                return;

            Replay replay;

            try
            {
                switch (ResultsType)
                {
                    case ResultScreenType.Gameplay:
                    case ResultScreenType.Replay:
                        replay = Replay;
                        break;
                    case ResultScreenType.Score:
                        // Prevent watching online replays for now.
                        if (Score.IsOnline)
                        {
                            WatchOnlineReplay();
                            return;
                        }
                        else
                            replay = new Replay($"{ConfigManager.DataDirectory.Value}/r/{Score.Id}.qr");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "Unable to read replay file");
                Logger.Error(e, LogType.Runtime);
                return;
            }

            if (replay == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "Could not read replay");
                return;
            }

            Exit(() =>
            {
                if (AudioEngine.Track != null)
                {
                    lock (AudioEngine.Track)
                        AudioEngine.Track.Fade(10, 300);
                }

                // Load up the .qua file again
                var qua = MapManager.Selected.Value.LoadQua();
                MapManager.Selected.Value.Qua = qua;
                GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
                return new MapLoadingScreen(new List<Score>(), replay);
            });
        }

        /// <summary>
        ///     Exits the screen to retry the map
        /// </summary>
        public void ExitToRetryMap()
        {
            Exit(() => new MapLoadingScreen(new List<Score>()));
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
        }

        /// <summary>
        ///     Exits to watch a replay online.
        /// </summary>
        private void WatchOnlineReplay()
        {
            if (IsFetchingOnlineReplay)
                return;

            IsFetchingOnlineReplay = true;
            Logger.Important($"Fetching online replay for score: {Score.Id}", LogType.Network);

            var dialog = new DownloadingReplayDialog();
            DialogManager.Show(dialog);

            ThreadScheduler.Run(() =>
            {
                var dir = $"{ConfigManager.DataDirectory}/Downloads";
                var path = $"{dir}/{Score.Id}.qr";
                Directory.CreateDirectory(dir);

                try
                {
                    OnlineManager.Client?.DownloadReplay(Score.Id, path);
                    var replay = new Replay(path);

                    Exit(() =>
                    {
                        if (AudioEngine.Track != null)
                        {
                            lock (AudioEngine.Track)
                                AudioEngine.Track.Fade(10, 300);
                        }
                        GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;
                        return new MapLoadingScreen(new List<Score>(), replay);
                    });
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Network);
                    NotificationManager.Show(NotificationLevel.Error, "Failed to download online replay");
                }
                finally
                {
                    DialogManager.Dismiss(dialog);
                    File.Delete(path);
                    IsFetchingOnlineReplay = false;
                }
            });
        }

        /// <summary>
        ///     Changes discord rich presence to show results.
        /// </summary>
        private void ChangeDiscordPresence()
        {
            DiscordHelper.Presence.EndTimestamp = 0;

            // Don't change if we're loading in from a replay file.
            if (ResultsType == ResultScreenType.Replay || Gameplay.InReplayMode)
            {
                DiscordHelper.Presence.Details = "Idle";
                DiscordHelper.Presence.State = "In the Menus";
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
                return;
            }

            var state = Gameplay.Failed ? "Fail" : "Pass";
            var score = $"{ScoreProcessor.Score / 1000}k";
            var acc = $"{StringHelper.AccuracyToString(ScoreProcessor.Accuracy)}";
            var grade = Gameplay.Failed ? "F" : GradeHelper.GetGradeFromAccuracy(ScoreProcessor.Accuracy).ToString();
            var combo = $"{ScoreProcessor.MaxCombo}x";

            DiscordHelper.Presence.State = $"{state}: {grade} {score} {acc} {combo}";
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }
    }
}
