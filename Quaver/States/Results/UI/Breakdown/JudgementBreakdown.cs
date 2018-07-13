using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Results.UI.Breakdown
{
    internal class JudgementBreakdown : HeaderedSprite
    {
        /// <summary>
        ///     Reference to the score processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="processor"></param>
        public JudgementBreakdown(ScoreProcessor processor)
                            : base(new Vector2((GameBase.WindowRectangle.Width - 120) / 2f, 285),
                                "Judgement Breakdown", Fonts.AllerRegular16, 0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Processor = processor;
            PosX = -SizeX / 2f - 10;

            Content = CreateContainer();
            Content.Parent = this;
            Content.PosY = 50;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected sealed override Sprite CreateContainer()
        {
            var content = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            // It's very wonky to make a for loop out of this, so we'll just use a foreach here.
            var i = 0;
            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                var name = new SpriteText
                {
                    Parent = content,
                    Font = Fonts.AssistantRegular16,
                    Text = j.ToString(),
                    PosY =  i * 35 + 30,
                    TextScale = 0.90f
                };

                name.PosX = name.MeasureString().X / 2f + 25;

                var color = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[j];

                // Find the percentage of the current judgement.
                var percentage = (float)Processor.CurrentJudgements[j] / Processor.TotalJudgementCount;

                // Draw Progress Bar
                var progressBar = new Sprite
                {
                    Parent = content,
                    Position = new UDim2D(100, name.PosY - name.MeasureString().Y / 8f),
                    Tint = color,
                    SizeY = 15,
                    SizeX = (content.SizeX - 100) * percentage
                };

                if (progressBar.SizeX < 1)
                    progressBar.SizeX = 1;

                var judgementAmount = new SpriteText()
                {
                    Parent = content,
                    Text = $"{Processor.CurrentJudgements[j]:N0} ({percentage * 100:0.0}%)",
                    Font = Fonts.AllerRegular16,
                    TextColor = color,
                    Position = new UDim2D(progressBar.PosX + progressBar.SizeX + 10, name.PosY),
                    TextScale = 0.75f,
                };

                var judgementAmountSize = judgementAmount.MeasureString() / 2;
                judgementAmount.PosX += judgementAmountSize.X;
                judgementAmount.PosY += judgementAmountSize.Y / 2f;

                i++;
            }

            return content;
        }
    }
}