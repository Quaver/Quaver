using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Point = System.Drawing.Point;

namespace Quaver.Screens.Results.UI.Statistics
{
    public class HealthGraph : Sprite
    {
        /// <inheritdoc />
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="processor"></param>
        public HealthGraph(ScoreProcessor processor)
        {
            var points = processor.Stats.Select((point, i) => new Point(i, (int)point.Health)).ToList();

            // Set size position, and alignment.
            Size = new ScalableVector2(550, 200);
            Alignment = Alignment.TopCenter;
            Y = 10;

            // Image = Graph.CreateStaticLine(points, new Vector2(Width, Height), 2);

            // Create the text that displays 100% label.
            var health100Text = new SpriteText(Fonts.Exo2Regular24, $"Health (100%)")
            {
                Parent = this,
                TextScale = 0.42f,
                Alignment = Alignment.TopLeft,
                X = 10,
                Y = 5,
                TextColor = Color.LimeGreen
            };

            // Position the early text correctly.
            var health100TextSize = health100Text.MeasureString() / 2f;
            health100Text.X += health100TextSize.X;
            health100Text.Y += health100TextSize.Y;

            // Create the text that displays 0%
            var health0Text = new SpriteText(Fonts.Exo2Regular24, $"Health (0%)")
            {
                Parent = this,
                TextScale = 0.42f,
                Alignment = Alignment.BotLeft,
                X = 10,
                Y = -5,
                TextColor = Color.Red
            };

            // Position the late text correctly.
            var health0TextSize = health0Text.MeasureString() / 2f;
            health0Text.X += health0TextSize.X;
            health0Text.Y -= health0TextSize.Y;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // TODO: When graphs are hooked back up.
            // Image.Dispose();

            base.Destroy();
        }
    }
}
