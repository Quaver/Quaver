using System;
using System.Linq;
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
    internal class JudgementBreakdown : HeaderedContainer
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
                            : base(new Vector2((GameBase.WindowRectangle.Width - 120) / 2f, 310),
                                "Judgement Breakdown", Fonts.AllerRegular16, 0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Processor = processor;
            PosX = -SizeX / 2f - 10;

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
                    Font = Fonts.Exo2Regular24,
                    Text = j.ToString().ToUpper(),
                    PosY =  i * 35 + 22,
                    TextScale = 0.50f,
                    TextColor = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[j]
                };

                name.PosX = name.MeasureString().X / 2f + 25;

                var color = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[j];

                // Find the percentage of the current judgement.
                var percentage = (float)Processor.CurrentJudgements[j] / Processor.TotalJudgementCount;

                // Draw Progress Bar
                var progressBar = new Sprite
                {
                    Parent = content,
                    Position = new UDim2D(100, name.PosY),
                    Tint = color,
                    SizeY = 14,
                    SizeX = (content.SizeX - 225) * percentage
                };

                progressBar.PosY -= progressBar.SizeY / 2f;

                if (progressBar.SizeX < 1)
                    progressBar.SizeX = 1;

                var judgementAmount = new SpriteText()
                {
                    Parent = content,
                    Text = $"{Processor.CurrentJudgements[j]:N0} ({percentage * 100:0.0}%)",
                    Font = Fonts.Exo2Regular24,
                    TextColor = color,
                    Position = new UDim2D(progressBar.PosX + progressBar.SizeX + 10, progressBar.PosY),
                    TextScale = 0.50f
                };

                var judgementAmountSize = judgementAmount.MeasureString() / 2;
                judgementAmount.PosX += judgementAmountSize.X;
                judgementAmount.PosY += judgementAmountSize.Y / 2f + 2;

                i++;
            }

            var dividerLine = new Sprite
            {
                Parent = content,
                Size = new UDim2D(content.SizeX - 50, 1),
                Alignment = Alignment.BotCenter,
                PosY = -40
            };

            var totalJudgements = new SpriteText()
            {
                Parent = content,
                Font = Fonts.Exo2Regular24,
                TextScale = 0.50f,
                Text = $"TOTAL JUDGEMENTS: {Processor.TotalJudgementCount:N0}",
                Alignment = Alignment.BotLeft
            };

            totalJudgements.PosX = totalJudgements.MeasureString().X / 2f + 25;
            totalJudgements.PosY = dividerLine.PosY + totalJudgements.MeasureString().Y / 2f + 10;

            var bestJudgement = GetBestJudgement();

            var bestJudgeValue = new SpriteText()
            {
                Parent = content,
                Font = Fonts.Exo2Regular24,
                TextScale = 0.50f,
                Text = bestJudgement.ToString().ToUpper(),
                Alignment = Alignment.BotRight,
                TextColor = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[bestJudgement]
            };

            bestJudgeValue.PosY = dividerLine.PosY + bestJudgeValue.MeasureString().Y / 2f + 10;
            bestJudgeValue.PosX = -bestJudgeValue.MeasureString().X / 2f - 25;

            var bestJudgementText = new SpriteText()
            {
                Parent = content,
                Font = Fonts.Exo2Regular24,
                TextScale = 0.50f,
                Text = "BEST JUDGEMENT:",
                Alignment = Alignment.BotRight,
            };

            bestJudgementText.PosY = dividerLine.PosY + bestJudgementText.MeasureString().Y / 2f + 10;
            bestJudgementText.PosX = -bestJudgementText.MeasureString().X - bestJudgeValue.PosX - bestJudgementText.MeasureString().X / 2f + 15;

            return content;
        }

        /// <summary>
        ///     Gets the best judgement out of all of them.
        /// </summary>
        /// <returns></returns>
        private Judgement GetBestJudgement() => Processor.CurrentJudgements.FirstOrDefault(x => x.Value == Processor.CurrentJudgements.Values.Max()).Key;
    }
}