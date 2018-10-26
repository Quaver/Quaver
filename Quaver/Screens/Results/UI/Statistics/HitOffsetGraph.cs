using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Resources;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;
using Point = System.Drawing.Point;

namespace Quaver.Screens.Results.UI.Statistics
{
    public class HitOffsetGraph : Sprite
    {
        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="processor"></param>
        public HitOffsetGraph(ScoreProcessor processor)
        {
            // Grab the miss judgement value, so we can use it to create the list of points.
            var missValue = processor.JudgementWindow[Judgement.Miss];

            // Create the list of points needed to create the graph.
            var points = processor.Stats.Select((point, i) => new Point(i, MathHelper.Clamp((int)point.HitDifference, (int)-missValue, (int)missValue))).ToList();

            // Create the container for custom judgement line colors for the graph.
            var judgementLineColors = new Dictionary<int, System.Drawing.Color>();

            // Go through each window and create a new line with the window's color &
            // positive and negative window values.
            foreach (var window in processor.JudgementWindow)
            {
                var judgeColor = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[window.Key];
                var convertedColor = System.Drawing.Color.FromArgb(judgeColor.R, judgeColor.G, judgeColor.B);

                judgementLineColors.Add((int)-window.Value, convertedColor);
                judgementLineColors.Add((int)window.Value, convertedColor);
            }

            // Set size position, and alignment.
            Size = new ScalableVector2(550, 200);
            Alignment = Alignment.TopCenter;
            Y = 10;

            // Create the actual graph and set it to this sprite's texture.
            //Image = Graph.CreateStaticScatterPlot(points, new Vector2(Width, Height), Colors.XnaToSystemDrawing(Color.Black),
            //                                            3, judgementLineColors, judgementLineColors);

            // Create the text that displays the early miss indicator.
            var earlyText = new SpriteText(BitmapFonts.Exo2Regular, $"Early (-{missValue}ms)", 14)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = 10,
                Y = 5
            };

            // Create the text that displays the late miss indicator.
            var lateText = new SpriteText(BitmapFonts.Exo2Regular, $"Late ({missValue}ms)", 14)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = 10,
                Y = -5
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///    Disposes the graph upon destroy.
        /// </summary>
        public override void Destroy()
        {
            // TODO: When graphs are hooked back up.
            // Image.Dispose();

            base.Destroy();
        }
    }
}
