using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Heading
{
    public class ResultsOverviewScoreContainer : Sprite
    {
        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        protected List<DrawableResultsScoreMetric> Items { get; set; } = new List<DrawableResultsScoreMetric>();

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        public ResultsOverviewScoreContainer(Map map, Bindable<ScoreProcessor> processor)
        {
            Map = map;
            Processor = processor;

            Image = SkinManager.Skin?.Results?.ResultsScoreContainerPanel ?? UserInterface.ResultsScoreContainerPanel;
            Size = new ScalableVector2(ResultsScreenView.CONTENT_WIDTH - ResultsTabContainer.PADDING_X, Image.Height);
        }

        /// <summary>
        /// </summary>
        protected virtual void SetItems()
        {
            var rating = new RatingProcessorKeys(Map.LoadQua().SolveDifficulty(Processor.Value.Mods, true).OverallDifficulty);
            var accuracy = Processor.Value?.StandardizedProcessor?.Accuracy ?? Processor.Value.Accuracy;

            Items.AddRange(new []
            {
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelMaxCombo ?? UserInterface.ResultsLabelMaxCombo, $"{Processor.Value.MaxCombo:n0}x"),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelAccuracy ?? UserInterface.ResultsLabelAccuracy, StringHelper.AccuracyToString(Processor.Value.Accuracy)),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelPerformanceRating ?? UserInterface.ResultsLabelPerformanceRating,
                    $"{StringHelper.RatingToString(rating.CalculateRating(accuracy))}", ColorHelper.HexToColor("#E9B736")),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelRankedAccuracy ?? UserInterface.ResultsLabelRankedAccuracy, StringHelper.AccuracyToString(accuracy)),
                new DrawableResultsScoreMetric(SkinManager.Skin?.Results?.ResultsLabelTotalScore ?? UserInterface.ResultsLabelTotalScore,
                    $"{Processor.Value.Score:n0}"),
            });
        }

        /// <summary>
        /// </summary>
        public void CreateItems()
        {
            SetItems();

            var widthSum = Items.First().Width * Items.Count;
            var widthPer = (Width- widthSum) / (Items.Count + 1);

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];

                item.Parent = this;
                item.Alignment = Alignment.MidLeft;
                item.X = widthPer;

                if (i != 0)
                {
                    var last = Items[i - 1];
                    item.X = last.X + last.Width + widthPer;
                }
            }
        }
    }
}