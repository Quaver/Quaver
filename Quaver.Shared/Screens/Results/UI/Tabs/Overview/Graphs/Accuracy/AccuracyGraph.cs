using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Result.UI;
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
        private List<(int, float)> AccuracyHistory { get; set; }

        /// <summary>
        ///     Downscaled container from parent container in order to fit the numbers
        /// </summary>
        private Sprite ContentContainer { get; set; }

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
                Size = new ScalableVector2(Width - 60, Height * 0.9f)
            };

            CreateAccuracyHistory();
            CreateDataPoints();
            CreateGridlinesAndLabels();
        }

        private void CreateAccuracyHistory()
        {
            if (!(Processor.Value is ScoreProcessorKeys keysProcessor)) return;
            // keysProcessor.Map is not set for replays
            var simulatedProcessor =
                new ScoreProcessorKeys(Map.Qua, keysProcessor.Mods, keysProcessor.Windows);

            AccuracyHistory = new List<(int, float)>();
            MinAccuracy = 100f;

            var i = 0;
            var previousTime = int.MinValue;
            foreach (var stat in keysProcessor.Stats)
            {
                simulatedProcessor.CalculateScore(stat.Judgement, stat.KeyPressType == KeyPressType.Release);
                var acc = simulatedProcessor.Accuracy;
                if (stat.SongPosition == previousTime)
                    AccuracyHistory.Remove(AccuracyHistory.Last()); // prevent multiple accuracies on a single time
                AccuracyHistory.Add((stat.SongPosition, acc));
                previousTime = stat.SongPosition;
                // skip the first judgements because of heavy fluctuations
                if (i > 20)
                    MinAccuracy = Math.Min(acc, MinAccuracy);
                i++;
            }

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
            var start = AccuracyHistory.First().Item1;
            var end = Map.SongLength;

            for (var i = 0; i < AccuracyHistory.Count; i++)
            {
                var (time, acc) = AccuracyHistory[i];
                var y = (acc - AccuracyStart) / (100f - AccuracyStart);

                var songProgress = (float) (time - start) / (end - start);
                var nextTime = i == AccuracyHistory.Count - 1 ? time : AccuracyHistory[i + 1].Item1;
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
                    Alpha = 1f,
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

                var line = new Sprite
                {
                    Parent = ContentContainer,
                    Alpha = 0.5f,
                    Tint = ColorHelper.HexToColor("#808080"),
                    Alignment = Alignment.TopCenter,
                    Y = relativeY * ContentContainer.Height,
                    Size = new ScalableVector2(ContentContainer.Width, 2),
                };

                var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{acc}%", 20,
                    false)
                {
                    Parent = line,
                    Alignment = Alignment.MidLeft,
                    Tint = ColorHelper.HexToColor("#808080")
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