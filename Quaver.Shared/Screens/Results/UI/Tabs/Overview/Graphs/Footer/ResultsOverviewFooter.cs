using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer
{
    public class ResultsOverviewFooter : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> IsSubmittingScore { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreSubmissionResponse> ScoreSubmissionStats { get; }

        /// <summary>
        /// </summary>
        private ResultsSubmittingScore SubmittingScore { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="isSubmittingScore"></param>
        /// <param name="scoreSubmissionStats"></param>
        /// <param name="size"></param>
        public ResultsOverviewFooter(Map map, Bindable<ScoreProcessor> processor, Bindable<bool> isSubmittingScore,
            Bindable<ScoreSubmissionResponse> scoreSubmissionStats, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            IsSubmittingScore = isSubmittingScore;
            ScoreSubmissionStats = scoreSubmissionStats;
            Size = size;
            Alpha = 0;

            if (IsSubmittingScore.Value)
                CreateSubmittingScore();

            IsSubmittingScore.ValueChanged += OnSubmittingScoreChanged;
            ScoreSubmissionStats.ValueChanged += OnScoreSubmissionStatsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            IsSubmittingScore.ValueChanged -= OnSubmittingScoreChanged;
            ScoreSubmissionStats.ValueChanged -= OnScoreSubmissionStatsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateSubmittingScore() => SubmittingScore = new ResultsSubmittingScore(21)
        {
            Parent = this,
            Alignment = Alignment.MidCenter
        };

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubmittingScoreChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (e.Value)
                return;

            SubmittingScore?.FadeOut();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScoreSubmissionStatsChanged(object sender, BindableValueChangedEventArgs<ScoreSubmissionResponse> e)
        {
            if (e.Value == null || e.Value.Status != 200  || e.Value.Stats == null)
                return;

            var stats = new List<ResultsOverviewFooterStat>()
            {
                new ResultsOverviewFooterStat("GLOBAL RANK", $"#{e.Value.Stats.NewGlobalRank:n0}"),
                new ResultsOverviewFooterStat("COUNTRY RANK", $"#{e.Value.Stats.NewCountryRank:n0}"),
                new ResultsOverviewFooterStat("OVERALL RATING", $"{StringHelper.RatingToString(e.Value.Stats.OverallPerformanceRating)}"),
                new ResultsOverviewFooterStat("OVERALL ACCURACY", $"{StringHelper.AccuracyToString((float) e.Value.Stats.OverallAccuracy)}"),
            };

            var widthSum = stats.Sum(x => x.Width);
            var widthPer = (Width- widthSum) / (stats.Count + 1);

            for (var i = 0; i < stats.Count; i++)
            {
                var item = stats[i];

                item.Parent = this;
                item.Alignment = Alignment.MidLeft;
                item.X = widthPer;

                if (i != 0)
                {
                    var last = stats[i - 1];
                    item.X = last.X + last.Width + widthPer;
                }

                item.FadeIn(350);
            }
        }
    }
}