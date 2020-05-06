using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Replays;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Results.UI;
using Quaver.Shared.Screens.Results.UI.Header.Contents.Tabs;
using Wobble.Bindables;
using Wobble.Input;

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
        public Bindable<ScoreProcessor> Processor { get; }

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

            Processor = new Bindable<ScoreProcessor>(screen.Ruleset.ScoreProcessor)
            {
                Value =
                {
                    PlayerName = ConfigManager.Username.Value,
                    Date = DateTime.Now,
                    Windows = JudgementWindowsDatabaseCache.Selected.Value,
                    StandardizedProcessor = screen.Ruleset.StandardizedReplayPlayer.ScoreProcessor,
                }
            };

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}