using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Graphics;
using Quaver.Graphics.Graphing;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.States.Results.UI.Data
{
    internal class HitOffsetGraph : Sprite
    {
        internal HitOffsetGraph(ScoreProcessor processor)
        {
            var missValue = processor.JudgementWindow[Judgement.Miss];

            // Create the list of points needed to create the graph.
            var sum = 0;
            var totalHits = 0;
            var points = processor.Stats.Select((point, i) =>
            {
                // Add to  sum of hit differences for mean calculation.
                if (point.Type != HitStatType.Miss)
                {
                    sum += (int)Math.Abs(point.HitDifference);
                    totalHits++;
                }

                return new Point(i, MathHelper.Clamp((int) point.HitDifference, (int) -missValue, (int) missValue));
            }).ToList();

            // Create the list of custom lines w/ their colors.
            var judgementLineColors = new Dictionary<int, System.Drawing.Color>();
            foreach (var window in processor.JudgementWindow)
            {
                var judgeColor = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[window.Key];
                var convertedColor = System.Drawing.Color.FromArgb(judgeColor.R, judgeColor.G, judgeColor.B);

                judgementLineColors.Add((int)-window.Value, convertedColor);
                judgementLineColors.Add((int)window.Value, convertedColor);
            }

            Size = new UDim2D(550, 200);
            Alignment = Alignment.TopCenter;
            PosY = 10;

            // Create graph.
            Image = Graph.CreateStaticScatterPlot(points, new Vector2(550, 200), Colors.XnaToSystemDrawing(Color.Black),
                                                        3, judgementLineColors, judgementLineColors);

            var earlyText = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Early (-{missValue}ms)",
                TextScale = 0.42f,
                Alignment = Alignment.TopLeft,
                PosX = 10,
                PosY = 5
            };

            var earlyTextSize = earlyText.MeasureString() / 2f;

            earlyText.PosX += earlyTextSize.X;
            earlyText.PosY += earlyTextSize.Y;

            var lateText = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Late ({missValue}ms)",
                TextScale = 0.42f,
                Alignment = Alignment.BotLeft,
                PosX = 10,
                PosY = -5
            };

            var lateTextSize = lateText.MeasureString() / 2f;

            lateText.PosX += lateTextSize.X;
            lateText.PosY -= lateTextSize.Y;

            var meanText = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Mean: {sum / points.Count}ms",
                TextScale = 0.42f,
                Alignment = Alignment.TopRight,
                PosX = -10,
                PosY = 5
            };

            var meanTextSize = meanText.MeasureString() / 2f;

            meanText.PosX -= meanTextSize.X;
            meanText.PosY += meanTextSize.Y;

            // Find the standard deviation of the data hit differences.
            var average = (double)sum / totalHits;
            var sumOfSquaresOfDifferences = processor.Stats.Where(x => x.Type == HitStatType.Hit).Select(val => (Math.Abs(val.HitDifference) - average) * (Math.Abs(val.HitDifference) - average)).Sum();
            var standardDev = Math.Sqrt(sumOfSquaresOfDifferences / totalHits);

            var sdText = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Standard Deviation: {standardDev:0.##}ms",
                TextScale = 0.42f,
                Alignment = Alignment.BotRight,
                PosX = -10,
                PosY = -5
            };

            var sdTextSize = sdText.MeasureString() / 2f;

            sdText.PosX -= sdTextSize.X;
            sdText.PosY -= sdTextSize.Y;
        }
    }
}