/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Result.UI;
using Quaver.Shared.Screens.Select;
using Wobble;
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
       ///     Multiplayer scores (if in a multiplayer match)
       /// </summary>
        public List<ScoreboardUser> MultiplayerScores { get; }

        /// <summary>
        /// </summary>
        private MultiplayerScreen MultiplayerScreen { get; }

        /// <summary>
        /// </summary>
        public Dictionary<ScoreboardUser, ResultScoreContainer> CachedScoreContainers { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="gameplay"></param>
        /// <param name="multiplayerScores"></param>
        /// <param name="multiplayerScreen"></param>
        public ResultScreen(GameplayScreen gameplay, List<ScoreboardUser> multiplayerScores = null, MultiplayerScreen multiplayerScreen = null)
        {
            Gameplay = gameplay;
            ResultsType = ResultScreenType.Gameplay;
            ScoreProcessor = Gameplay.Ruleset.ScoreProcessor;
            MultiplayerScores = multiplayerScores;
            MultiplayerScreen = multiplayerScreen;

            InitializeIfGameplayType();
            ChangeDiscordPresence();

            if (MultiplayerScores != null)
            {
                Logger.Important($"Multiplayer Player Game Finished!", LogType.Network);

                MultiplayerScores.ForEach(x =>
                {
                    var modsString = "None";

                    try
                    {
                        modsString = ModHelper.GetModsString(x.Processor.Mods);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    Logger.Important($"{(x.UsernameRaw)}: {x.Processor.Score}, {x.Processor.Accuracy}, " +
                                     $"{x.Processor.TotalJudgementCount}, {x.RatingProcessor.CalculateRating(x.Processor)} | " +
                                     $"{modsString}", LogType.Network);
                });
            }

            View = new ResultScreenView(this);
            CacheMultiplayerScoreContainers();
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
            CacheMultiplayerScoreContainers();
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
            CacheMultiplayerScoreContainers();
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

        public override void Destroy()
        {
            if (CachedScoreContainers != null)
            {
                foreach (var container in CachedScoreContainers)
                    container.Value.Destroy();
            }

            base.Destroy();
        }

        /// <summary>
        ///     Handles input for the entire screen.
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0 || Exiting)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                ExitToMenu();

            if (MouseManager.IsUniqueClick(MouseButton.Right))
            {
                var view = View as ResultScreenView;

                if (view?.SelectedMultiplayerUser?.Value != null)
                    view.SelectedMultiplayerUser.Value = null;
            }
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

                if (Replay.Mods.HasFlag(ModIdentifier.Autoplay))
                    ScoreProcessor = Gameplay.Ruleset.ScoreProcessor;
                else if (Gameplay.SpectatorClient != null)
                {
                    var im = Gameplay.Ruleset.InputManager as KeysInputManager;
                    ScoreProcessor = im.ReplayInputManager.VirtualPlayer.ScoreProcessor;
                }
                else
                    ScoreProcessor = new ScoreProcessorKeys(Replay);

                ScoreProcessor.Stats = Gameplay.Ruleset.ScoreProcessor.Stats;

                // Remove all modifiers that was played during the replay, so the user doesn't still have them
                // activated when they go back to select. (autoplay is the only exception)
                if (!ModManager.IsActivated(ModIdentifier.Autoplay))
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
            if (Gameplay.Ruleset.ScoreProcessor.CurrentJudgements[Judgement.Miss] == Gameplay.Ruleset.ScoreProcessor.TotalJudgementCount)
                return;

            ThreadScheduler.Run(SaveLocalScore);

            // Don't submit scores if disconnected from the server completely.
            if (OnlineManager.Status.Value == ConnectionStatus.Disconnected)
                return;

            if (Gameplay.IsMultiplayerGame)
            {
                if (!Gameplay.IsPlayComplete)
                {
                    Logger.Important($"Skipping score submission due to play not being complete in multiplayer match", LogType.Network);
                    return;
                }
            }

            if (Gameplay.Ruleset.ScoreProcessor.Failed && !Gameplay.Ruleset.StandardizedReplayPlayer.ScoreProcessor.Failed)
            {
                Logger.Important($"Skipping score submission due to failing on custom windows, but not on standardized", LogType.Network);
                return;
            }

            ThreadScheduler.Run(() =>
            {
                Logger.Important($"Beginning to submit score on map: {Gameplay.MapHash}", LogType.Network);

                var map = Map;

                var submissionMd5 = Gameplay.MapHash;

                if (map.Game != MapGame.Quaver)
                    submissionMd5 = map.GetAlternativeMd5();

                // For any unsubmitted maps, ask the server if it has the .qua already cached
                // if it doesn't, then we need to provide it.
                if (map.RankedStatus == RankedStatus.NotSubmitted && OnlineManager.IsDonator)
                {
                    var info = OnlineManager.Client?.RetrieveMapInfo(submissionMd5);

                    // Map is not uploaded, so we have to provide the server with it.
                    if (info == null)
                    {
                        Logger.Important($"Unsubmitted map is not cached on the server. Need to provide!", LogType.Network);
                        var success = OnlineManager.Client?.UploadUnsubmittedMap(Gameplay.Map, submissionMd5, map.Md5Checksum);

                        // The map upload wasn't successful, so we can assume that our score shouldn't be submitted
                        if (success != null && !success.Value)
                        {
                            Logger.Error($"Unsubmitted map upload was not successful. Skipping score submission", LogType.Network);
                            return;
                        }
                    }
                }

                OnlineManager.Client?.Submit(new OnlineScore(submissionMd5, Gameplay.ReplayCapturer.Replay,
                    Gameplay.Ruleset.StandardizedReplayPlayer.ScoreProcessor, ScrollSpeed, ModHelper.GetRateFromMods(ModManager.Mods), TimeHelper.GetUnixTimestampMilliseconds(),
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
                var localScore = Score.FromScoreProcessor(Gameplay.Ruleset.ScoreProcessor, Gameplay.MapHash, ConfigManager.Username.Value, ScrollSpeed,
                    Gameplay.PauseCount, Gameplay.Map.RandomizeModifierSeed);

                localScore.RatingProcessorVersion = RatingProcessorKeys.Version;
                localScore.RankedAccuracy = Gameplay.Ruleset.StandardizedReplayPlayer.ScoreProcessor.Accuracy;

                var windows = JudgementWindowsDatabaseCache.Selected.Value;

                localScore.JudgementWindowPreset = windows.Name;
                localScore.JudgementWindowMarv = windows.Marvelous;
                localScore.JudgementWindowPerf = windows.Perfect;
                localScore.JudgementWindowGreat = windows.Great;
                localScore.JudgementWindowGood= windows.Good;
                localScore.JudgementWindowOkay = windows.Okay;
                localScore.JudgementWindowMiss = windows.Miss;

                if (ScoreProcessor.Failed)
                    localScore.PerformanceRating = 0;
                else
                    localScore.PerformanceRating = new RatingProcessorKeys(Map.DifficultyFromMods(Gameplay.Ruleset.ScoreProcessor.Mods)).CalculateRating(Gameplay.Ruleset.StandardizedReplayPlayer.ScoreProcessor.Accuracy);

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

            if (OnlineManager.CurrentGame != null)
            {
                var view = View as ResultScreenView;

                if (view?.SelectedMultiplayerUser?.Value != null)
                {
                    view.SelectedMultiplayerUser.Value = null;
                    return;
                }

                Exit(() => MultiplayerScreen ?? new MultiplayerScreen(OnlineManager.CurrentGame));
                MultiplayerScreen.SetRichPresence();
                return;
            }

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
                        Replay = replay;
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
            if (OnlineManager.IsSpectatingSomeone)
                OnlineManager.Client?.StopSpectating();

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
            if (OnlineManager.IsSpectatingSomeone)
                OnlineManager.Client?.StopSpectating();

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

            if (OnlineManager.CurrentGame == null)
                DiscordHelper.Presence.State = $"{state}: {grade} {score} {acc} {combo}";
            else
            {
                if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                {
                    var redTeamAverage = GetTeamAverage(MultiplayerTeam.Red);
                    var blueTeamAverage = GetTeamAverage(MultiplayerTeam.Blue);

                    DiscordHelper.Presence.State = $"Red: {redTeamAverage:0.00} vs. Blue: {blueTeamAverage:0.00}";
                }
                else
                {
                    DiscordHelper.Presence.State = $"{StringHelper.AddOrdinal(MultiplayerScores.First().Rank)} " +
                                                   $"Place: {MultiplayerScores.First().RatingProcessor.CalculateRating(ScoreProcessor):0.00} {acc} {grade}";
                }
            }

            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <summary>
        ///     Gets the average rating of an individual team
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double GetTeamAverage(MultiplayerTeam team)
        {
            List<ScoreboardUser> users;

            switch (team)
            {
                case MultiplayerTeam.Red:
                    users = MultiplayerScores.FindAll(x => x.Scoreboard.Team == MultiplayerTeam.Red && !x.HasQuit);
                    break;
                case MultiplayerTeam.Blue:
                    users = MultiplayerScores.FindAll(x => x.Scoreboard.Team == MultiplayerTeam.Blue && !x.HasQuit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(team), team, null);
            }

            if (users.Count == 0)
                return 0;

            var sum = 0d;

            users.ForEach(x =>
            {
                var rating = x.CalculateRating();

                if (x.Processor.MultiplayerProcessor.IsEliminated || x.Processor.MultiplayerProcessor.IsRegeneratingHealth)
                    rating = 0;

                sum += rating;
            });

            return sum / users.Count;
        }

        /// <summary>
        ///     Creates and caches score containers for
        /// </summary>
        private void CacheMultiplayerScoreContainers()
        {
            if (MultiplayerScores == null)
                return;

            CachedScoreContainers = new Dictionary<ScoreboardUser, ResultScoreContainer>();

            var self = MultiplayerScores.Find(x => x.Type == ScoreboardUserType.Self);

            var view = (ResultScreenView) View;
            CachedScoreContainers.Add(self, view.ScoreContainer);

            foreach (var s in MultiplayerScores)
            {
                if (s.Type == ScoreboardUserType.Self)
                    continue;

                ScoreProcessor = s.Processor;
                CachedScoreContainers.Add(s, new ResultScoreContainer(this)
                {
                    Parent = view.MainContainer,
                    Visible = s.Type == ScoreboardUserType.Self
                });
            }

            ScoreProcessor = self.Processor;
        }

        /// <summary>
        ///     Returns the score processor to use. Loads hit stats from a replay if needed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ScoreProcessor GetScoreProcessor()
        {
            // Handles the case when watching a replay in its entirety. This uses the preprocessed
            // ScoreProcessor/Replay from gameplay to get a 100% accurate score output.
            // Also avoids having to process the replay again (as done below).
            if (Gameplay != null && Gameplay.InReplayMode)
            {
                var im = Gameplay.Ruleset.InputManager as KeysInputManager;
                return im?.ReplayInputManager.VirtualPlayer.ScoreProcessor;
            }

            // If we already have stats (for example, this is a result screen right after a player finished playing a map), use them.
            if (ScoreProcessor.Stats != null)
                return ScoreProcessor;

            // Otherwise, get the stats from a replay.
            Replay replay = null;

            // FIXME: unify this logic with watching a replay from a ResultScreen.
            try
            {
                switch (ResultsType)
                {
                    case ResultScreenType.Gameplay:
                    case ResultScreenType.Replay:
                        replay = Replay;
                        break;
                    case ResultScreenType.Score:
                        // Don't do anything for online replays since they aren't downloaded yet.
                        if (!Score.IsOnline)
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
            }

            // Load a replay if we got one.
            if (replay == null)
                return ScoreProcessor;

            var qua = Map.LoadQua();
            qua.ApplyMods(replay.Mods);
            if (replay.Mods.HasFlag(ModIdentifier.Randomize))
                qua.RandomizeLanes(replay.RandomizeModifierSeed);

            JudgementWindows windows = null;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Score != null && Score.JudgementWindowPreset != JudgementWindowsDatabaseCache.Standard.Name && Score.JudgementWindowMarv != 0)
            {
                windows = new JudgementWindows()
                {
                    Marvelous = Score.JudgementWindowMarv,
                    Perfect = Score.JudgementWindowPerf,
                    Great = Score.JudgementWindowGreat,
                    Good = Score.JudgementWindowGood,
                    Okay = Score.JudgementWindowOkay,
                    Miss = Score.JudgementWindowMiss
                };
            }

            var player = new VirtualReplayPlayer(replay, qua, windows);
            player.PlayAllFrames();

            return player.ScoreProcessor;
        }
    }
}
