using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Results.UI.Data
{
    internal class ResultsScoreStatistics : HeaderedSprite
    {
        /// <summary>
        ///     Reference to the score processor that was played.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScoreStatistics(ResultsScreen screen)
                                    : base(new Vector2(screen.UI.JudgementBreakdown.SizeX, screen.UI.JudgementBreakdown.SizeY),
                                        "Statistics", Fonts.AllerRegular16, 0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Screen = screen;
            PosX = SizeX / 2f + 10;

            Content = CreateContent();
            Content.Parent = this;
            Content.PosY = 50;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override Sprite CreateContent()
        {
            var sprite = new Sprite
            {
                Parent = this,
                Size = new UDim2D(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            var comingSoon = new SpriteText()
            {
                Parent = sprite,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Font = Fonts.AssistantRegular16,
                Text = "Coming Soon"
            };

            return sprite;
        }
    }
}