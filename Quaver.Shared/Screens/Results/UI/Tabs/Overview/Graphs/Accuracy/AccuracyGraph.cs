using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
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

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Accuracy
{
    public class AccuracyGraph : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        ///     The minimum accuracy in the score, used for scaling the graph
        /// </summary>
        private float MinAccuracy { get; set; }

        /// <summary>
        ///     The accuracy interval between each grid line
        /// </summary>
        private float AccuracyStep { get; set; }

        /// <summary>
        ///     How many grid lines to draw in total
        /// </summary>
        private int GridLineCount => (int) Math.Round((100f - AccuracyStart) / AccuracyStep);

        /// <summary>
        ///     The lower bound of the graph
        /// </summary>
        private float AccuracyStart => MinAccuracy - AccuracyStep - (MinAccuracy % AccuracyStep);

        /// <summary>
        ///     Accuracy data points throughout the score
        /// </summary>
        private List<(int, float)> AccuracyDataHistory { get; set; }

        /// <summary>
        ///     Accuracy data if the player hit perfectly after the fail/quit point
        /// </summary>
        private List<(int, float)> MaximumPossibleHistory { get; set; }

        /// <summary>
        ///     Downscaled container from parent container in order to fit the numbers
        /// </summary>
        private Sprite ContentContainer { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public AccuracyGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
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

            AccuracyDataHistory = new List<(int, float)>();
            MinAccuracy = 100f;

            var i = 0;
            var previousTime = int.MinValue;

            foreach (var stat in keysProcessor.Stats)
            {
                simulatedProcessor.CalculateScore(stat.Judgement, stat.KeyPressType == KeyPressType.Release);

                var acc = simulatedProcessor.Accuracy;

                // Prevent multiple accuracies on a single time
                if (AccuracyDataHistory.Count > 0 && stat.SongPosition == previousTime)
                    AccuracyDataHistory.Remove(AccuracyDataHistory.Last());

                AccuracyDataHistory.Add((stat.SongPosition, acc));
                previousTime = stat.SongPosition;

                // skip the first judgements because of heavy fluctuations
                if (i > 20)
                    MinAccuracy = Math.Min(acc, MinAccuracy);

                i++;
            }

            // Score was quit or failed
            if (simulatedProcessor.TotalJudgementCount < simulatedProcessor.GetTotalJudgementCount())
            {
                MaximumPossibleHistory = new List<(int, float)>();
                var hitObjectsLeftToPlay = qua.HitObjects.Where(h => h.StartTime > AccuracyDataHistory.Last().Item1);

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
                    MaximumPossibleHistory.Add((time, acc));
                }
            }

            // Determine optimal step size for grid lines
            // 99.5, 99, 98, 96, 90, 80, 75
            foreach (var step in new[] {0.10f, 0.25f, 0.5f, 1.0f, 2.0f, 5.0f, 10.0f})
            {
                AccuracyStep = step;
                if (GridLineCount <= 10)
                    break;
            }
        }

        private void CreateDataPoints()
        {
            DrawDataPointsFromHistory(AccuracyDataHistory, 1f);

            if (MaximumPossibleHistory != null)
                DrawDataPointsFromHistory(MaximumPossibleHistory, 0.2f);
        }

        private void DrawDataPointsFromHistory(IReadOnlyList<(int, float)> history, float opacity)
        {
            var start = AccuracyDataHistory.First().Item1;
            var end = Map.SongLength;

            for (var i = 0; i < history.Count; i++)
            {
                var (time, acc) = history[i];
                var y = (acc - AccuracyStart) / (100f - AccuracyStart);

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
            // <= because we also want to draw the final line
            for (var i = 0; i <= GridLineCount; i++)
            {
                var relativeY = (float) i / GridLineCount;
                var acc = Math.Round(100f - i * AccuracyStep, 2);
                var alpha = 0.5f;
                var textAlpha = 1.0f;
                var thickness = 3;

                // is sub grid line
                if (AccuracyStep < 0.25 && acc % 0.5 > 0 ||
                    AccuracyStep >= 0.25 && AccuracyStep < 1 && acc % 1 > 0 ||
                    AccuracyStep >= 1 && AccuracyStep < 5 && acc % 5 > 0 ||
                    AccuracyStep >= 5 && AccuracyStep < 10 && acc % 10 > 0 ||
                    AccuracyStep >= 10 && AccuracyStep < 50 && acc % 50 > 0)
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

                var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{acc:f2}%", 20,
                    false)
                {
                    Parent = line,
                    Alignment = Alignment.MidLeft,
                    Tint = ColorHelper.HexToColor("#808080"),
                    Alpha = textAlpha
                };

                text.X -= text.Width + 10;
            }
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