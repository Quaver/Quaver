using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// <inheritdoc />
    /// <summary>
    ///     Graph for the Hit offset, used on the results screen.
    /// </summary>
    internal class HitOffsetGraph : Sprite
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="processor"></param>
        internal HitOffsetGraph(ScoreProcessor processor)
        {
            // Grab the miss judgement value, so we can use it to create the list of points.
            var missValue = processor.JudgementWindow[Judgement.Miss];

            // Create the list of points needed to create the graph.
            var points = processor.Stats.Select((point, i) => new Point(i, MathHelper.Clamp((int) point.HitDifference, (int) -missValue, (int) missValue))).ToList();

            // Create the container for custom judgement line colors for the graph.
            var judgementLineColors = new Dictionary<int, System.Drawing.Color>();

            // Go through each window and create a new line with the window's color &
            // positive and negative window values.
            foreach (var window in processor.JudgementWindow)
            {
                var judgeColor = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[window.Key];
                var convertedColor = System.Drawing.Color.FromArgb(judgeColor.R, judgeColor.G, judgeColor.B);

                judgementLineColors.Add((int)-window.Value, convertedColor);
                judgementLineColors.Add((int)window.Value, convertedColor);
            }

            // Set size position, and alignment.
            Size = new UDim2D(550, 200);
            Alignment = Alignment.TopCenter;
            PosY = 10;

            // Create the actual graph and set it to this sprite's texture.
            Image = Graph.CreateStaticScatterPlot(points, new Vector2(SizeX, SizeY), Colors.XnaToSystemDrawing(Color.Black),
                                                        3, judgementLineColors, judgementLineColors);

            // Create the text that displays the early miss indicator.
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

            // Position the early text correctly.
            var earlyTextSize = earlyText.MeasureString() / 2f;
            earlyText.PosX += earlyTextSize.X;
            earlyText.PosY += earlyTextSize.Y;

            // Create the text that displays the late miss indicator.
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

            // Position the late text correctly.
            var lateTextSize = lateText.MeasureString() / 2f;
            lateText.PosX += lateTextSize.X;
            lateText.PosY -= lateTextSize.Y;
        }

        /// <inheritdoc />
        /// <summary>
        ///    Disposes the graph upon destroy.
        /// </summary>
        internal override void Destroy()
        {
            Task.Run(() => Image.Dispose());
            base.Destroy();
        }
    }
}