using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Results.UI;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Quaver.Shared.Screens.Tournament.Gameplay;
using Wobble.Bindables;
using Wobble.Input;
using Wobble.Logging;

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
        public Map Map { get; }

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
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScreen(GameplayScreen screen)
        {
            Map = MapManager.Selected.Value;

            InitializeGameplayResultsScreen(screen);

            View = new ResultsScreenView(this);
        }

        /// <summary>
        /// </summary>
        public ResultsScreen(Map map, Score score)
        {
            Map = map;

            Processor = new Bindable<ScoreProcessor>(new ScoreProcessorKeys(score.ToReplay()))
            {
                Value =
                {
                    Windows = new JudgementWindows
                    {
                        Name = score.JudgementWindowPreset,
                        Marvelous = score.JudgementWindowMarv,
                        Perfect = score.JudgementWindowPerf,
                        Great = score.JudgementWindowGreat,
                        Good = score.JudgementWindowGood,
                        Okay = score.JudgementWindowOkay,
                        Miss = score.JudgementWindowMiss
                    }
                }
            };

            View = new ResultsScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Processor.Dispose();
            ActiveTab.Dispose();
            IsSubmittingScore.Dispose();
            ScoreSubmissionStats.Dispose();
            base.Destroy();
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

            var inputManager = (KeysInputManager) screen.Ruleset.InputManager;

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

            SubmitLocalScore(screen, replay);
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
            score.RatingProcessorVersion = RatingProcessorKeys.Version;
            score.PerformanceRating = processor.Failed ? 0 : new RatingProcessorKeys(Map.DifficultyFromMods(processor.Mods)).CalculateRating(rankedAccuracy);
            score.RankedAccuracy = rankedAccuracy;

            // Select proper local profile id to attach with this score for ranking
            if (UserProfileDatabaseCache.Selected.Value.Id != 0 && !UserProfileDatabaseCache.Selected.Value.IsOnline)
                score.UserProfileId = UserProfileDatabaseCache.Selected.Value.Id;

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}