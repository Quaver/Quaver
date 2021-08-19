using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Rating
{
    public class RatingGraph : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private RatingProcessor RatingProcessor { get; set; }

        /// <summary>
        ///     The minimum rating in the score, used for scaling the graph
        /// </summary>
        private float MinRating { get; set; }

        /// <summary>
        ///     The maximum rating attainable in the map, used for scaling the graph
        /// </summary>
        private float MaxRating => (float) RatingProcessor.CalculateRating(100f);

        /// <summary>
        ///     The accuracy interval between each grid line
        /// </summary>
        private float RatingStep { get; set; }

        /// <summary>
        ///     How many grid lines to draw in total
        /// </summary>
        private int GridLineCount => (int) Math.Floor((MaxRating - RatingStart) / RatingStep);

        /// <summary>
        ///     The lower bound of the graph
        /// </summary>
        private float RatingStart => MinRating - RatingStep - (MinRating % RatingStep);

        /// <summary>
        ///     Accuracy data points throughout the score
        /// </summary>
        private List<(int, float, float)> DataPoints { get; set; }

        /// <summary>
        ///     Accuracy data if the player hit perfectly after the fail/quit point
        /// </summary>
        private List<(int, float, float)> MaximumPossibleHistory { get; set; }

        /// <summary>
        ///     Downscaled container from parent container in order to fit the numbers
        /// </summary>
        private Sprite ContentContainer { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public RatingGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            Size = size;
            Alpha = 0;

            ContentContainer = new Sprite
            {
                Parent = this,
                Alpha = 0f,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Width - 80, Height * 0.9f)
            };

            CreateAccuracyHistory();
            CreateDataPoints();
            CreateGridlinesAndLabels();
        }

        private void CreateAccuracyHistory()
        {
            if (!(Processor.Value is ScoreProcessorKeys keysProcessor))
                return;

            var qua = Map.Qua ?? Map.LoadQua();

            var simulatedProcessor = new ScoreProcessorKeys(qua, keysProcessor.Mods, keysProcessor.Windows);
            var difficulty = simulatedProcessor.Map.SolveDifficulty(keysProcessor.Mods).OverallDifficulty;

            RatingProcessor = new RatingProcessorKeys(difficulty);
            DataPoints = new List<(int, float, float)>();
            MinRating = MaxRating;

            var i = 0;
            var previousTime = int.MinValue;

            foreach (var stat in keysProcessor.Stats)
            {
                simulatedProcessor.CalculateScore(stat.Judgement, stat.KeyPressType == KeyPressType.Release);

                var acc = simulatedProcessor.Accuracy;
                var rating = (float) RatingProcessor.CalculateRating(acc);

                // Prevent multiple accuracies on a single time
                if (DataPoints.Count > 0 && stat.SongPosition == previousTime)
                    DataPoints.Remove(DataPoints.Last());

                DataPoints.Add((stat.SongPosition, acc, rating));
                previousTime = stat.SongPosition;

                // skip the first judgements because of heavy fluctuations
                if (i > 20)
                    MinRating = Math.Min(rating, MinRating);

                i++;
            }

            // Score was quit or failed
            if (simulatedProcessor.TotalJudgementCount < simulatedProcessor.GetTotalJudgementCount())
            {
                MaximumPossibleHistory = new List<(int, float, float)>();
                var hitObjectsLeftToPlay = qua.HitObjects.Where(h => h.StartTime > DataPoints.Last().Item1);

                // Separate ordered list is required because of hits being out of order if you go though each hit object
                // and take the start/end time at that moment
                // time, isLN
                var hitTimes = new List<(int, bool)>();

                foreach (var hitObject in hitObjectsLeftToPlay)
                {
                    hitTimes.Add((hitObject.StartTime, false));

                    if (hitObject.IsLongNote)
                        hitTimes.Add((hitObject.EndTime, true));
                }

                foreach (var (time, isLn) in hitTimes.OrderBy(h => h.Item1))
                {
                    simulatedProcessor.CalculateScore(Judgement.Marv, isLn);

                    var acc = simulatedProcessor.Accuracy;
                    var rating = (float) RatingProcessor.CalculateRating(acc);
                    MaximumPossibleHistory.Add((time, acc, rating));
                }
            }

            // Determine optimal step size for grid lines
            // 99.5, 99, 98, 96, 90, 80, 75
            foreach (var step in new[] {0.10f, 0.25f, 0.5f, 1.0f, 2.0f, 5.0f, 10.0f})
            {
                RatingStep = step;
                if (GridLineCount <= 10)
                    break;
            }
        }

        private void CreateDataPoints()
        {
            DrawDataPointsFromHistory(DataPoints, 1f);

            if (MaximumPossibleHistory != null)
                DrawDataPointsFromHistory(MaximumPossibleHistory, 0.2f);
        }

        private void DrawDataPointsFromHistory(IReadOnlyList<(int, float, float)> history, float opacity)
        {
            var start = DataPoints.First().Item1;
            var end = Map.SongLength;

            for (var i = 0; i < history.Count; i++)
            {
                var (time, acc, rating) = history[i];
                var y = (rating - RatingStart) / (MaxRating - RatingStart);

                var songProgress = (float) (time - start) / (end - start);
                var nextTime = i == history.Count - 1 ? time : history[i + 1].Item1;
                var nextSongProgress = (float) (nextTime - start) / (end - start);
                var width = nextSongProgress - songProgress;

                var point = new Sprite
                {
                    Parent = ContentContainer,
                    Alignment = Alignment.TopLeft,
                    X = songProgress * ContentContainer.Width,
                    Y = (1f - y) * ContentContainer.Height,
                    Size = new ScalableVector2(width * ContentContainer.Width, y * ContentContainer.Height),
                    Visible = true,
                    Alpha = opacity,
                    Tint = GetColor(acc)
                };
            }
        }

        /// <summary>
        /// </summary>
        private void CreateGridlinesAndLabels()
        {
            // Max Rating
            DrawGridLine(MaxRating, 0f, false);

            // <= because we also want to draw the final line
            for (var i = 1; i <= GridLineCount; i++)
            {
                var relativeY = (float) i / GridLineCount;
                var rating = Math.Round(RatingStart + (GridLineCount - i) * RatingStep, 2);
                var isSubGridLine = RatingStep < 0.25 && rating % 0.5 > 0 ||
                                    RatingStep >= 0.25 && RatingStep < 1 && rating % 1 > 0 ||
                                    RatingStep >= 1 && RatingStep < 5 && rating % 5 > 0 ||
                                    RatingStep >= 5 && RatingStep < 10 && rating % 10 > 0 ||
                                    RatingStep >= 10 && RatingStep < 50 && rating % 50 > 0;

                DrawGridLine(rating, relativeY, isSubGridLine);
            }
        }


        private void DrawGridLine(double rating, float relativeY, bool isSubGridLine)
        {
            var alpha = 0.5f;
            var textAlpha = 1.0f;

            var thickness = 3;
            if (isSubGridLine)
            {
                alpha /= 3;
                textAlpha /= 2;
                thickness = 2;
            }

            var line = new Sprite
            {
                Parent = ContentContainer,
                Alpha = alpha,
                Tint = ColorHelper.HexToColor("#808080"),
                Alignment = Alignment.TopCenter,
                Y = relativeY * ContentContainer.Height,
                Size = new ScalableVector2(ContentContainer.Width, thickness),
            };

            var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{rating:f2}", 20,
                false)
            {
                Parent = line,
                Alignment = Alignment.MidLeft,
                Tint = ColorHelper.HexToColor("#808080"),
                Alpha = textAlpha
            };

            text.X -= text.Width + 10;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Color GetColor(float accuracy)
        {
            switch (GradeHelper.GetGradeFromAccuracy(accuracy))
            {
                case Grade.X:
                    return new Color(251, 255, 182);
                case Grade.SS:
                    return new Color(255, 241, 137);
                case Grade.S:
                    return new Color(255, 231, 107);
                case Grade.A:
                    return new Color(86, 254, 110);
                case Grade.B:
                    return new Color(0, 209, 255);
                case Grade.C:
                    return new Color(217, 107, 206);
                default:
                    return new Color(249, 100, 93);
            }
        }
    }
}