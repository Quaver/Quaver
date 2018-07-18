using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics;
using Quaver.Graphics.Graphing;
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
                Text = "No Statistics Available.\nPlay the map or watch the replay!"
            };

            // ONLY draw graph if we're coming from gameplay.
            if (Screen.Type == ResultsScreenType.FromGameplay)
            {
                var points = new List<Point>();

                for (var i = 0; i < Screen.ScoreProcessor.Stats.Count; i++)
                {
                    var point = Screen.ScoreProcessor.Stats[i];
                    var missValue = Screen.ScoreProcessor.JudgementWindow[Judgement.Miss];

                    // Make sure that all of the hits and misses are clamped to the
                    points.Add(new Point(i, MathHelper.Clamp((int) point.HitDifference, (int) -missValue, (int) missValue)));
                }

                // Create the list of custom lines w/ their colors.
                var judgementLineColors = new Dictionary<int, System.Drawing.Color>();
                foreach (var window in Screen.ScoreProcessor.JudgementWindow)
                {
                    var judgeColor = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[window.Key];
                    var convertedColor = System.Drawing.Color.FromArgb(judgeColor.R, judgeColor.G, judgeColor.B);

                    judgementLineColors.Add((int)-window.Value, convertedColor);
                    judgementLineColors.Add((int)window.Value, convertedColor);
                }

                // ReSharper disable once ObjectCreationAsStatement
                new Sprite
                {
                    Parent = sprite,
                    Size = new UDim2D(550, 200),
                    Alignment = Alignment.MidCenter,
                    Image = Graph.CreateStaticScatterPlot(points, new Vector2(550, 188), Colors.XnaToSystemDrawing(Color.Black),
                                                                3, judgementLineColors, judgementLineColors)
                };
            }

            return sprite;
        }
    }
}