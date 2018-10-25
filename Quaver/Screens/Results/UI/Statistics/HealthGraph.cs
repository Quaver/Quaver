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
            var health100Text = new SpriteText(BitmapFonts.Exo2Regular, $"Health (100%)", 14)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = 10,
                Y = 5,
                Tint = Color.LimeGreen
            };

            // Create the text that displays 0%
            var health0Text = new SpriteText(BitmapFonts.Exo2Regular, $"Health (0%)", 14)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = 10,
                Y = -5,
                Tint = Color.Red
            };
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
