using System;
using System.Collections.Generic;
using System.Linq;
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

        private float AccuracyStart { get; set; }

        /// <summary>
        ///     Accuracy data points throughout the score
        /// </summary>
        private List<(int, float)> AccuracyHistory { get; set; }

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
                Size = new ScalableVector2(Width - 125, Height * 0.9f)
            };

            CreateAccuracyHistory();
            CreateGridlinesAndLabels();
            CreateDataPoints();
        }

        private void CreateAccuracyHistory()
        {
            AccuracyHistory = new List<(int, float)>();
            MinAccuracy = 1.0f;
            if (!(Processor.Value is ScoreProcessorKeys keysProcessor)) return;
            // keysProcessor.Map is not set for replays
            var simulatedProcessor =
                new ScoreProcessorKeys(Map.Qua, keysProcessor.Mods, keysProcessor.Windows);

            var i = 0;
            foreach (var stat in keysProcessor.Stats)
            {
                simulatedProcessor.CalculateScore(stat.Judgement, stat.KeyPressType == KeyPressType.Release);
                var acc = simulatedProcessor.Accuracy / 100;
                var dataPoint = (stat.SongPosition, acc);
                AccuracyHistory.Add(dataPoint);
                // skip the first 10 judgements because of heavy fluctuations
                if (i < 10)
                    i++;
                else
                    MinAccuracy = Math.Min(acc, MinAccuracy);
            }
        }

        /// <summary>
        /// </summary>
        private void CreateGridlinesAndLabels()
        {
            float accStepSize;
            switch (GradeHelper.GetGradeFromAccuracy(MinAccuracy * 100))
            {
                case Grade.SS:
                    accStepSize = 0.0025f;
                    break;
                case Grade.S:
                    accStepSize = 0.010f;
                    break;
                case Grade.A:
                    accStepSize = 0.025f;
                    break;
                case Grade.B:
                    accStepSize = 0.050f;
                    break;
                default:
                    accStepSize = 0.100f;
                    break;
            }

            AccuracyStart = MinAccuracy - (MinAccuracy % accStepSize);
            var lineCount =
                (int) Math.Round((1.0f - AccuracyStart) / accStepSize); // Round because of possible float inaccuracy

            // <= because we also want to draw the final line
            for (var i = 0; i <= lineCount; i++)
            {
                var relativeY = (float) i / lineCount;
                var acc = Math.Round(100f - i * accStepSize * 100, 2);

                var line = new Sprite
                {
                    Parent = ContentContainer,
                    Alpha = 0.5f,
                    Tint = ColorHelper.HexToColor("#808080"),
                    Alignment = Alignment.TopCenter,
                    Y = relativeY * ContentContainer.Height,
                    Size = new ScalableVector2(Width, 2),
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

        private void CreateDataPoints()
        {
            var start = AccuracyHistory.First().Item1;
            var end = AccuracyHistory.Last().Item1;

            for (var i = 0; i < AccuracyHistory.Count; i++)
            {
                var (time, acc) = AccuracyHistory[i];
                var nextTime = i == AccuracyHistory.Count - 1 ? 1 : AccuracyHistory[i + 1].Item1;

                var songProgress = (float) (time - start) / (end - start);
                var nextSongProgress = (float) (nextTime - start) / (end - start);

                var y = (acc - AccuracyStart) / (1f - AccuracyStart);
                var width = nextSongProgress - songProgress;

                var point = new Sprite
                {
                    Parent = ContentContainer,
                    Alignment = Alignment.TopLeft,
                    X = songProgress * ContentContainer.Width,
                    Y = (1f - y) * ContentContainer.Height,
                    Size = new ScalableVector2(width * ContentContainer.Width, 5),
                    Visible = true,
                    Alpha = 1f,
                    Tint = ColorHelper.HexToColor("#ffffff"),
                };
            }
        }
    }
}