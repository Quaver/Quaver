using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Graphing;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.States.Results.UI.Statistics
{
    /// <inheritdoc />
    /// <summary>
    ///     Graph on the results screen that
    /// </summary>
    internal class HealthGraph : Sprite
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="processor"></param>
        internal HealthGraph(ScoreProcessor processor)
        {
            var points = processor.Stats.Select((point, i) => new Point(i, (int) point.Health)).ToList();

            // Set size position, and alignment.
            Size = new UDim2D(550, 200);
            Alignment = Alignment.TopCenter;
            PosY = 10;

            Image = Graph.CreateStaticLine(points, new Vector2(SizeX, SizeY), 2);

            // Create the text that displays 100% label.
            var health100Text = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Health (100%)",
                TextScale = 0.42f,
                Alignment = Alignment.TopLeft,
                PosX = 10,
                PosY = 5,
                TextColor = Color.LimeGreen
            };

            // Position the early text correctly.
            var health100TextSize = health100Text.MeasureString() / 2f;
            health100Text.PosX += health100TextSize.X;
            health100Text.PosY += health100TextSize.Y;

            // Create the text that displays 0%
            var health0Text = new SpriteText
            {
                Parent = this,
                Font = Fonts.Exo2Regular24,
                Text = $"Health (0%)",
                TextScale = 0.42f,
                Alignment = Alignment.BotLeft,
                PosX = 10,
                PosY = -5,
                TextColor = Color.Red
            };

            // Position the late text correctly.
            var health0TextSize = health0Text.MeasureString() / 2f;
            health0Text.PosX += health0TextSize.X;
            health0Text.PosY -= health0TextSize.Y;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal override void Destroy()
        {
            Task.Run(() => Image.Dispose());
            base.Destroy();
        }
    }
}