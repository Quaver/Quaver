using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.API.Replays.Virtual;
using Quaver.Server.Client;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Results
{
    public sealed class ResultsScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Results;

        /// <summary>
        /// </summary>
        public ResultsScreenType ScreenType { get; }

        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        ///     The gameplay screen if <see cref="ScreenType"/> is Gameplay.
        /// </summary>
        public GameplayScreen Gameplay { get; }

        /// <summary>
        ///     The local/online score if <see cref="ScreenType"/> is Score.
        /// </summary>
        public Score Score { get; }

        /// <summary>
        ///     The replay of the score if <see cref="ScreenType"/> is Replay.
        /// </summary>
        public Replay Replay { get; private set; }

        /// <summary>
        ///     Active multiplayer game for this results screen if one exists
        /// </summary>
        public MultiplayerGame MultiplayerGame { get; }

        /// <summary>
        ///     Players on team 1 in the multiplayer game
        /// </summary>
        public List<ScoreProcessor> MultiplayerTeam1Users { get; }

        /// <summary>
        ///     Players on team 2 in the multiplayer game
        /// </summary>
        public List<ScoreProcessor> MultiplayerTeam2Users { get; }

        /// <summary>
        /// </summary>
        public Bindable<ScoreProcessor> Processor { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<ResultsScreenTabType> ActiveTab { get; } = new Bindable<ResultsScreenTabType>(ResultsScreenTabType.Overview);

        /// <summary>
        /// </summary>
        public Bindable<bool> IsSubmittingScore { get; } = new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<ScoreSubmissionResponse> ScoreSubmissionStats { get; } = new Bindable<ScoreSubmissionResponse>(null);

        /// <summary>
        ///     If the user is in progress of converting their score
        /// </summary>
        public bool IsConvertingScore { get; private set; }

        /// <summary>
        ///     If the user has already fixed their local offset
        /// </summary>
        public bool FixedLocalOffset { get; private set; }

        /// <summary>
        ///     Audio channel to play the applause sound
        /// </summary>
        public AudioSampleChannel ApplauseChannel { get; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScreen(GameplayScreen screen)
        {
            ScreenType = ResultsScreenType.Gameplay;
            Gameplay = screen;
            Map = MapManager.FindMapFromMd5(screen.MapHash) ?? MapManager.Selected.Value;

            InitializeGameplayResultsScreen(screen);
            Replay = Gameplay.LoadedReplay ?? Gameplay.ReplayCapturer.Replay;

            SetDiscordRichPresence();
            View = new ResultsScreenView(this);

            if (!Gameplay.Failed)
            {
                ApplauseChannel = SkinManager.Skin.SoundApplause.CreateChannel();
                ApplauseChannel.Play();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="game"></param>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public ResultsScreen(GameplayScreen screen, MultiplayerGame game, List<ScoreProcessor> team1,
            List<ScoreProcessor> team2)
        {
            ScreenType = ResultsScreenType.Gameplay;
            Gameplay = screen;
            Map = MapManager.FindMapFromMd5(screen.MapHash) ?? MapManager.Selected.Value;
            MultiplayerGame = game;
            MultiplayerTeam1Users = team1;
            MultiplayerTeam2Users = team2;

            InitializeGameplayResultsScreen(screen);
            Replay = Gameplay.LoadedReplay ?? Gameplay.ReplayCapturer.Replay;

            SetDiscordRichPresence();
            View = new ResultsScreenView(this);
        }

        /// <summary>
        ///     Multiplayer game results screen from a Score object
        /// </summary>
        /// <param name="map"></param>
        /// <param name="game"></param>
        /// <param name="score"></param>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public ResultsScreen(Map map, MultiplayerGame game, Score score, List<ScoreProcessor> team1, List<ScoreProcessor> team2)
        {
            ScreenType = ResultsScreenType.Score;
            Score = score;
            Map = map;
            MultiplayerGame = game;
            MultiplayerTeam1Users = team1;
            MultiplayerTeam2Users = team2;

            InitializeScoreResultsScreen();

            SetDiscordRichPresence();
            View = new ResultsScreenView(this);
        }

        /// <summary>
        /// </summary>
        public ResultsScreen(Map map, Score score)
        {
            ScreenType = ResultsScreenType.Score;
            Score = score;
            Map = map;

            InitializeScoreResultsScreen();

            SetDiscordRichPresence();
            View = new ResultsScreenView(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="replay"></param>
        public ResultsScreen(Map map, Replay replay)
        {
            Replay = replay;
            Map = map;
            ScreenType = ResultsScreenType.Replay;
            Processor = new Bindable<ScoreProcessor>(new ScoreProcessorKeys(replay));

            ConvertScoreToJudgementWindows(null);

            View = new ResultsScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;
            Gameplay?.SendReplayFramesToServer(true);

            base.OnFirstUpdate();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleInput();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (ApplauseChannel != null && ApplauseChannel.HasPlayed && !ApplauseChannel.HasStopped)
                ApplauseChannel.Stop();

            Processor.Dispose();
            ActiveTab.Dispose();
            IsSubmittingScore.Dispose();
            ScoreSubmissionStats.Dispose();

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnScoreSubmitted -= OnScoreSubmitted;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        public void ExitToMenu()
        {
            if (OnlineManager.CurrentGame != null)
                Exit(() => new MultiplayerGameScreen());
            else
                Exit(() => new SelectionScreen());
        }

        /// <summary>
        /// </summary>
        public void RetryMap()
        {
            if (ModManager.IsActivated(ModIdentifier.Coop))
                ModManager.RemoveMod(ModIdentifier.Coop);

            Exit(() => new MapLoadingScreen(MapManager.Selected.Value.Scores.Value));
        }

        /// <summary>
        /// </summary>
        public void WatchReplay()
        {
            if (ScreenType == ResultsScreenType.Score && Score.IsOnline)
            {
                WatchOnlineReplay();
                return;
            }

            var replay = GetReplay();

            if (replay == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while retrieving the replay for this score!");
                return;
            }

            Exit(() => new MapLoadingScreen(MapManager.Selected.Value.Scores.Value, replay));
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Replay DownloadOnlineReplay()
        {
            var onlineReplay = Score.DownloadOnlineReplay();

            if (onlineReplay == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "The replay for this score could not be downloaded.");
                return null;
            }

            return onlineReplay;
        }

        /// <summary>
        ///     Shows a dialog to download and watch an online score's replay
        /// </summary>
        private void WatchOnlineReplay()
        {
            DialogManager.Show(new LoadingDialog("FETCHING REPLAY", "Downloading replay! Please wait...",
                DownloadAndWatchOnlineReplay));
        }

        /// <summary>
        ///     Downloads <see cref="Score"/>'s online replay and watches it
        /// </summary>
        private void DownloadAndWatchOnlineReplay()
        {
            var onlineReplay = DownloadOnlineReplay();

            if (onlineReplay == null)
                return;

            Exit(() => new MapLoadingScreen(MapManager.Selected.Value.Scores.Value, onlineReplay));
        }

        /// <summary>
        ///     Handles the entire export process for each results screen type
        /// </summary>
        public void ExportReplay()
        {
            if (ScreenType == ResultsScreenType.Score && Score.IsOnline)
            {
                ExportOnlineReplay();
                return;
            }

            var replay = GetReplay();

            if (replay == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while retrieving the replay for this score!");
                return;
            }

            NotificationManager.Show(NotificationLevel.Info, "Please wait while your replay is being exported...");
            ExportAndHighlightReplay(replay);
        }

        /// <summary>
        ///     Writes a replay file and opens it in the file manager
        /// </summary>
        /// <param name="replay"></param>
        private void ExportAndHighlightReplay(Replay replay)
        {
            ThreadScheduler.Run(() =>
            {
                try
                {
                    var path = $@"{ConfigManager.ReplayDirectory.Value}/{replay.PlayerName} - " +
                               $"{StringHelper.FileNameSafeString($"{Map.Artist} - {Map.Title} - {Map.DifficultyName}")}.qr";

                    replay.Write(path);
                    Utils.NativeUtils.HighlightInFileManager(path);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "An error occured while exporting the replay!");
                }
            });
        }

        /// <summary>
        ///     Shows a dialog to download and export the online replay
        /// </summary>
        private void ExportOnlineReplay()
        {
            DialogManager.Show(new LoadingDialog("FETCHING REPLAY", "Downloading replay! Please wait...",
                DownloadAndExportOnlineReplay));
        }

        /// <summary>
        /// </summary>
        private void DownloadAndExportOnlineReplay()
        {
            var onlineReplay = DownloadOnlineReplay();

            if (onlineReplay == null)
                return;

            ExportAndHighlightReplay(onlineReplay);
        }

        /// <summary>
        /// </summary>
        public void FixLocalOffset()
        {
            if (FixedLocalOffset)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You have already fixed your local offset for this map! There's no need to do it again.");
                return;
            }

            if (Processor.Value.Stats == null || Processor.Value.Stats.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "There is no data to be able to fix your local offset!");
                return;
            }

            var stats = Processor.Value.GetHitStatistics();

            // Local offset is scaled with rate, so the adjustment depends on the rate the
            // score was played on.
            var change = stats.Mean * ModHelper.GetRateFromMods(Processor.Value.Mods);
            var newOffset = (int)Math.Round(Map.LocalOffset - change);

            var dialog = new YesNoDialog("FIX LOCAL OFFSET",
                $"Your local offset for this map will be changed from {Map.LocalOffset} ms to {newOffset} ms.", () =>
                {
                    Map.LocalOffset = newOffset;
                    MapDatabaseCache.UpdateMap(Map);
                    NotificationManager.Show(NotificationLevel.Success, $"Local offset for this map was set to {Map.LocalOffset} ms.");
                    FixedLocalOffset = true;
                });

            DialogManager.Show(dialog);
        }

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (Exiting)
                return;

            if (DialogManager.Dialogs.Count > 0)
                return;

            HandleKeyPressEscape();
            HandleKeyPressTab();
            HandleKeyPressRetry();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            ExitToMenu();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressTab()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            var index = (int)ActiveTab.Value;
            var length = Enum.GetNames(typeof(ResultsScreenTabType)).Length;

            int newIndex;
            if (KeyboardManager.IsShiftDown())
            {
                if (index - 1 >= 0)
                    newIndex = index - 1;
                else
                    newIndex = length - 1;
            }
            else
            {
                if (index + 1 < length)
                    newIndex = index + 1;
                else
                    newIndex = 0;
            }

            var val = (ResultsScreenTabType)newIndex;

            if (val == ResultsScreenTabType.Multiplayer && MultiplayerGame == null)
                return;

            ActiveTab.Value = val;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressRetry()
        {
            if (!KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyRestartMap?.Value ?? Keys.OemTilde))
                return;

            if (OnlineManager.IsSpectatingSomeone)
                return;

            if (OnlineManager.CurrentGame != null)
                return;

            switch (ScreenType)
            {
                case ResultsScreenType.Gameplay:
                    RetryMap();
                    break;
                case ResultsScreenType.Replay:
                    WatchReplay();
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Replay GetReplay()
        {
            switch (ScreenType)
            {
                case ResultsScreenType.Gameplay:
                    return Gameplay.LoadedReplay ?? Gameplay.ReplayCapturer.Replay;
                case ResultsScreenType.Score:
                    if (!Score.IsOnline)
                    {
                        var path = $"{ConfigManager.DataDirectory}/r/{Score.Id}.qr";
                        return File.Exists(path) ? new Replay(path) : null;
                    }
                    break;
                case ResultsScreenType.Replay:
                    return Replay;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        /// <summary>
        /// </summary>
        private void InitializeGameplayResultsScreen(GameplayScreen screen)
        {
            Processor = new Bindable<ScoreProcessor>(screen.Ruleset.ScoreProcessor)
            {
                Value =
                {
                    Windows = JudgementWindowsDatabaseCache.Selected.Value,
                    Date = DateTime.Now,
                    StandardizedProcessor = screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor,
                    Stats = screen.Ruleset.ScoreProcessor.Stats
                }
            };

            var inputManager = (KeysInputManager)screen.Ruleset.InputManager;

            if (screen.InReplayMode)
            {
                Processor.Value.PlayerName = screen.LoadedReplay.PlayerName;
                Processor.Value.Stats = inputManager.ReplayInputManager.VirtualPlayer.ScoreProcessor.Stats;

                if (ModManager.Mods.HasFlag(ModIdentifier.Autoplay))
                    Processor.Value = screen.Ruleset.ScoreProcessor;
                else if (screen.SpectatorClient != null)
                {
                    Processor.Value = inputManager.ReplayInputManager.VirtualPlayer.ScoreProcessor;
                    Processor.Value.Windows = JudgementWindowsDatabaseCache.Standard;
                }
                else
                    Processor.Value.Date = screen.LoadedReplay.Date;

                // Remove the paused modifier if the replay had it enabled.
                if (ModManager.IsActivated(ModIdentifier.Paused))
                    ModManager.RemoveMod(ModIdentifier.Paused);

                return;
            }

            Processor.Value.PlayerName = ConfigManager.Username.Value;
            SubmitScore(screen);
        }

        private void InitializeScoreResultsScreen()
        {
            Processor = new Bindable<ScoreProcessor>(new ScoreProcessorKeys(Score.ToReplay()))
            {
                Value =
                {
                    Windows = new JudgementWindows
                    {
                        Name = Score.JudgementWindowPreset,
                        Marvelous = Score.JudgementWindowMarv,
                        Perfect = Score.JudgementWindowPerf,
                        Great = Score.JudgementWindowGreat,
                        Good = Score.JudgementWindowGood,
                        Okay = Score.JudgementWindowOkay,
                        Miss = Score.JudgementWindowMiss
                    },
                    SteamId = (ulong) Score.SteamId,
                    UserId = Score.PlayerId
                }
            };

            // Check to see if the replay exists for this Score and virtually play it, so HitStats
            // can be retrieved.
            if (!Score.IsOnline)
            {
                var path = $"{ConfigManager.DataDirectory}/r/{Score.Id}.qr";

                if (File.Exists(path))
                {
                    try
                    {
                        var replay = new Replay(path);

                        var qua = MapManager.Selected.Value.LoadQua();
                        qua.ApplyMods((ModIdentifier)Score.Mods);
                        if (replay.Mods.HasFlag(ModIdentifier.Randomize))
                        {
                            qua.RandomizeLanes(replay.RandomizeModifierSeed);
                        }

                        var virtualPlayer = new VirtualReplayPlayer(replay, qua, Processor.Value.Windows);
                        virtualPlayer.PlayAllFrames();

                        Processor.Value.Stats = virtualPlayer.ScoreProcessor.Stats;
                        Processor.Value.StandardizedProcessor = new ScoreProcessorKeys(qua, (ModIdentifier)Score.Mods) { Accuracy = (float)Score.RankedAccuracy };

                        Replay = replay;
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to virtual play local score replay!", LogType.Runtime);
                        Logger.Error(e, LogType.Runtime);
                    }
                }
            }

            // TODO: Place player on the correct team processor list
            if (MultiplayerGame != null)
            {
                // Temporary
                for (var i = 0; i < 4; i++)
                    MultiplayerTeam1Users.Add(Processor.Value);
            }
        }
        /// <summary>
        ///     Handles both the online and local score submission process
        /// </summary>
        private void SubmitScore(GameplayScreen screen)
        {
            if (screen.HasQuit || screen.InReplayMode)
                return;

            const string skipping = "Skipping score submission due to:";
            var processor = screen.Ruleset.ScoreProcessor;

            if (screen is TournamentGameplayScreen || ModManager.IsActivated(ModIdentifier.Coop))
            {
                Logger.Important($"{skipping} score submission is unavailable for tournament gameplay screens.", LogType.Runtime);
                return;
            }

            if (processor.CurrentJudgements[Judgement.Miss] == processor.TotalJudgementCount)
            {
                Logger.Important($"{skipping} missing every single note (map not played).", LogType.Runtime);
                return;
            }

            var replay = screen.ReplayCapturer.Replay;
            replay.PauseCount = screen.PauseCount;
            replay.RandomizeModifierSeed = screen.Map.RandomizeModifierSeed;
            replay.FromScoreProcessor(processor);

            // Mark score as submitting online
            if (OnlineManager.Status.Value != ConnectionStatus.Disconnected)
            {
                IsSubmittingScore.Value = true;
                OnlineManager.Client.OnScoreSubmitted += OnScoreSubmitted;
            }

            ThreadScheduler.Run(() => SubmitLocalScore(screen, replay));
            ThreadScheduler.Run(() => IsSubmittingScore.Value = SubmitOnlineScore(screen, replay));
        }

        /// <summary>
        ///     Handles the local score submission & replay save process
        /// </summary>
        private void SubmitLocalScore(GameplayScreen screen, Replay replay)
        {
            var processor = screen.Ruleset.ScoreProcessor;

            // Handle which profile username is going to be attached to the score/replay
            var profileName = UserProfileDatabaseCache.Selected.Value.Username;
            var username = !string.IsNullOrEmpty(profileName) ? profileName : ConfigManager.Username.Value;

            var scrollSpeed = Map.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K.Value : ConfigManager.ScrollSpeed7K.Value;
            var rankedAccuracy = screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor.Accuracy;

            var score = Score.FromScoreProcessor(processor, screen.MapHash, username, scrollSpeed,
                screen.PauseCount, screen.Map.RandomizeModifierSeed, JudgementWindowsDatabaseCache.Selected.Value);

            // Calculate performance rating
            score.DifficultyProcessorVersion = DifficultyProcessorKeys.Version;
            score.RatingProcessorVersion = RatingProcessorKeys.Version;
            score.PerformanceRating = processor.Failed ? 0 : new RatingProcessorKeys(screen.Map.SolveDifficulty(processor.Mods).OverallDifficulty).CalculateRating(rankedAccuracy);
            score.RankedAccuracy = rankedAccuracy;

            // Select proper local profile id to attach with this score for ranking
            if (UserProfileDatabaseCache.Selected.Value.Id != 0 && !UserProfileDatabaseCache.Selected.Value.IsOnline)
                score.LocalProfileId = UserProfileDatabaseCache.Selected.Value.Id;

            var scoreId = -1;

            try
            {
                scoreId = ScoreDatabaseCache.InsertScoreIntoDatabase(score);
                Logger.Important($"Successfully saved score to database with id: {scoreId}", LogType.Runtime);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, $"Failed to save local score to the database. Please check the logs!");
                Logger.Error(e, LogType.Runtime);
            }

            try
            {
                if (scoreId == -1)
                    return;

                var path = $"{ConfigManager.DataDirectory}/r/{scoreId}.qr";

                replay.Write(path);
                Logger.Important($"Successfully saved replay to path: {path}", LogType.Runtime);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "There was an error when saving your replay. Check Runtime.log for more details.");
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Handles submission of scores to the server
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="replay"></param>
        private bool SubmitOnlineScore(GameplayScreen screen, Replay replay)
        {
            const string skipping = "Skipping online score submission due to:";

            // Don't submit scores if disconnected from the server completely.
            if (OnlineManager.Status.Value == ConnectionStatus.Disconnected)
            {
                Logger.Important($"{skipping} being fully disconnected", LogType.Network);
                return false;
            }

            // Don't submit scores that have unranked modifiers
            if (ModManager.CurrentModifiersList.Any(x => !x.Ranked()))
            {
                IsSubmittingScore.Value = false;
                Logger.Important($"{skipping} having unranked modifiers activated", LogType.Network);
                return false;
            }

            // User paused during gameplay
            if (screen.PauseCount != 0 || screen.Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.Paused))
            {
                IsSubmittingScore.Value = false;
                Logger.Important($"{skipping} pausing during gameplay.", LogType.Runtime);
                return false;
            }

            // Playing in a multiplayer, but the match was aborted.
            if (screen.IsMultiplayerGame && !screen.IsPlayComplete)
            {
                IsSubmittingScore.Value = false;
                Logger.Important($"{skipping} play was not completed (exited early in multiplayer match)", LogType.Runtime);
                return false;
            }

            var processor = screen.Ruleset.ScoreProcessor;

            // If the user fails the score on custom judgements, don't submit the score.
            if (JudgementWindowsDatabaseCache.Selected.Value != JudgementWindowsDatabaseCache.Standard &&
                processor.Failed)
            {
                Logger.Important($"{skipping} due to failing on custom judgements", LogType.Runtime);
                return false;
            }

            // User is playing on different windows (or multiplayer), so their score needs to be converted to Standard*.
            // This will validate if the user failed at any point during the play as well. Their score will need
            // to be submitted at the point of failure in both scenarios.
            if (JudgementWindowsDatabaseCache.Selected.Value != JudgementWindowsDatabaseCache.Standard || screen.IsMultiplayerGame || screen.FailedDuringGameplay)
            {
                var virtualPlayer = new VirtualReplayPlayer(screen.ReplayCapturer.Replay, screen.Map, new JudgementWindows(), true);
                var originalProcessor = screen.Ruleset.ScoreProcessor;
                processor = virtualPlayer.ScoreProcessor;

                Logger.Important($"Player used non-standard judgement windows for their play. Converting score...", LogType.Runtime);

                foreach (var _ in virtualPlayer.Replay.Frames)
                {
                    virtualPlayer.PlayNextFrame();

                    // Stop at the point of failure in the converted version
                    if (virtualPlayer.ScoreProcessor.Failed)
                    {
                        Logger.Important($"Player failed on Standard* windows during the conversion. Submitting at the point of failure", LogType.Runtime);
                        break;
                    }
                }

                Logger.Important($"Original Acc: {originalProcessor.Accuracy}% | Standard* Acc: {processor.Accuracy}%", LogType.Runtime);
            }

            // Start score submission process
            Logger.Important($"Beginning to submit score on map: {screen.MapHash} to the server...", LogType.Network);

            var submissionMd5 = screen.MapHash;

            // For un-submitted maps, ask the server if it knows about the MD5. If it doesn't and the map is non-Quaver,
            // try the converted .qua MD5, too. If that fails and the user is a donator, upload the map.
            if (Map.RankedStatus == RankedStatus.NotSubmitted)
            {
                var info = OnlineManager.Client?.RetrieveMapInfo(submissionMd5);

                // For non-Quaver maps, try the .qua MD5 (and use that for upload).
                if (info == null && Map.Game != MapGame.Quaver)
                {
                    submissionMd5 = Map.GetAlternativeMd5();
                    Logger.Important($"Unsubmitted map not found by original MD5. Trying .qua MD5: {submissionMd5}...", LogType.Network);
                    info = OnlineManager.Client?.RetrieveMapInfo(submissionMd5);
                }

                if (info == null)
                {
                    Logger.Important($"Unsubmitted map was not found on the server.", LogType.Network);

                    if (!OnlineManager.IsDonator)
                        // Non-donators can't upload maps.
                        return false;

                    // Map is not uploaded, so we have to provide the server with it.
                    var success = OnlineManager.Client?.UploadUnsubmittedMap(Map.LoadQua(), submissionMd5, Map.Md5Checksum);

                    // The map upload wasn't successful, so we can assume that our score shouldn't be submitted
                    if (success != null && !success.Value)
                    {
                        Logger.Error($"Unsubmitted map upload was not successful. Skipping score submission", LogType.Network);
                        return false;
                    }

                    Logger.Important($"Successfully uploaded unsubmitted map to the server!", LogType.Network);
                }
            }

            var scrollSpeed = ((HitObjectManagerKeys)screen.Ruleset.HitObjectManager).DefaultGroupController.AdjustedScrollSpeed;

            // Submit score to the server...
            OnlineManager.Client?.Submit(new OnlineScore(submissionMd5, replay, processor, (int)scrollSpeed,
                ModHelper.GetRateFromMods(ModManager.Mods), Gameplay.TimePlayEnd, OnlineManager.CurrentGame));

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScoreSubmitted(object sender, ScoreSubmissionEventArgs e)
        {
            // Hasn't submitted successfully yet.
            if (e.Response == null)
                return;

            IsSubmittingScore.Value = false;
            ScoreSubmissionStats.Value = e.Response;
            Logger.Important($"Received score submission response with status: {e.Response.Status}", LogType.Network);
        }

        /// <summary>
        /// </summary>
        /// <param name="windows"></param>
        public void ConvertScoreToJudgementWindows(JudgementWindows windows)
        {
            if (Replay == null)
            {
                NotificationManager.Show(NotificationLevel.Warning, "There is no replay data available to convert this score!");
                return;
            }

            IsConvertingScore = true;

            ThreadScheduler.Run(() =>
            {
                lock (Processor.Value)
                {
                    var qua = Map.LoadQua();
                    qua.ApplyMods(Replay.Mods);
                    if (Replay.Mods.HasFlag(ModIdentifier.Randomize))
                    {
                        qua.RandomizeLanes(Replay.RandomizeModifierSeed);
                    }

                    var virtualPlayer = new VirtualReplayPlayer(Replay, qua, windows, true);

                    virtualPlayer.ScoreProcessor.PlayerName = Processor.Value.PlayerName;
                    virtualPlayer.ScoreProcessor.Date = Processor.Value.Date;
                    virtualPlayer.ScoreProcessor.StandardizedProcessor = Processor.Value.StandardizedProcessor;

                    foreach (var _ in virtualPlayer.Replay.Frames)
                    {
                        if (virtualPlayer.ScoreProcessor.Failed)
                            break;

                        virtualPlayer.PlayNextFrame();
                    }

                    Processor.Value = virtualPlayer.ScoreProcessor;
                    IsConvertingScore = false;
                }
            });
        }

        /// <summary>
        /// </summary>
        public void SetDiscordRichPresence()
        {
            try
            {
                DiscordHelper.Presence.PartySize = 0;
                DiscordHelper.Presence.PartyMax = 0;
                DiscordHelper.Presence.StartTimestamp = 0;
                DiscordHelper.Presence.EndTimestamp = 0;
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(ConfigManager.SelectedGameMode.Value);

                RichPresenceHelper.UpdateRichPresence("In the Menus", "Results Screen");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0);
    }
}