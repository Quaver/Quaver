using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Results.UI
{
    public class JudgementBreakdown : HeaderedContainer
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
                            : base(new Vector2((WindowManager.Width - 120) / 2f, 310),
                                "Judgement Breakdown", Fonts.AllerRegular16, 0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Processor = processor;
            X = -Width / 2f - 10;

            Content = CreateContent();
            Content.Parent = this;
            Content.Y = 50;
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
                Size = new ScalableVector2(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            // It's very wonky to make a for loop out of this, so we'll just use a foreach here.
            var i = 0;
            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j == Judgement.Ghost)
                    break;

                var name = new SpriteText(Fonts.Exo2Regular24, j.ToString().ToUpper())
                {
                    Parent = content,
                    Y = i * 35 + 22,
                    TextScale = 0.50f,
                    TextColor = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[j]
                };

                name.X = name.MeasureString().X / 2f + 25;

                var color = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[j];

                // Find the percentage of the current judgement.
                var percentage = (float)Processor.CurrentJudgements[j] / Processor.TotalJudgementCount;

                // Draw Progress Bar
                var progressBar = new Sprite
                {
                    Parent = content,
                    Position = new ScalableVector2(100, name.Y),
                    Tint = color,
                    Height = 14,
                    Width = (content.Width - 225) * percentage
                };

                progressBar.Y -= progressBar.Height / 2f;

                if (progressBar.Width < 1)
                    progressBar.Width = 1;

                var judgementAmount = new SpriteText(Fonts.Exo2Regular24, $"{Processor.CurrentJudgements[j]:N0} ({percentage * 100:0.0}%)")
                {
                    Parent = content,
                    TextColor = color,
                    Position = new ScalableVector2(progressBar.X + progressBar.Width + 10, progressBar.Y),
                    TextScale = 0.50f
                };

                var judgementAmountSize = judgementAmount.MeasureString() / 2;
                judgementAmount.X += judgementAmountSize.X;
                judgementAmount.Y += judgementAmountSize.Y / 2f + 2;

                i++;
            }

            var dividerLine = new Sprite
            {
                Parent = content,
                Size = new ScalableVector2(content.Width - 50, 1),
                Alignment = Alignment.BotCenter,
                Y = -40
            };

            var totalJudgements = new SpriteText(Fonts.Exo2Regular24, $"TOTAL JUDGEMENTS: {Processor.TotalJudgementCount:N0}")
            {
                Parent = content,
                TextScale = 0.50f,
                Alignment = Alignment.BotLeft
            };

            totalJudgements.X = totalJudgements.MeasureString().X / 2f + 25;
            totalJudgements.Y = dividerLine.Y + totalJudgements.MeasureString().Y / 2f + 10;

            var bestJudgement = GetBestJudgement();

            var bestJudgeValue = new SpriteText(Fonts.Exo2Regular24, bestJudgement.ToString().ToUpper())
            {
                Parent = content,
                TextScale = 0.50f,
                Alignment = Alignment.BotRight,
                TextColor = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[bestJudgement]
            };

            bestJudgeValue.Y = dividerLine.Y + bestJudgeValue.MeasureString().Y / 2f + 10;
            bestJudgeValue.X = -bestJudgeValue.MeasureString().X / 2f - 25;

            var bestJudgementText = new SpriteText(Fonts.Exo2Regular24, "BEST JUDGEMENT:")
            {
                Parent = content,
                TextScale = 0.50f,
                Alignment = Alignment.BotRight,
            };

            bestJudgementText.Y = dividerLine.Y + bestJudgementText.MeasureString().Y / 2f + 10;
            bestJudgementText.X = -bestJudgementText.MeasureString().X - bestJudgeValue.X - bestJudgementText.MeasureString().X / 2f + 15;

            return content;
        }

        /// <summary>
        ///     Gets the best judgement out of all of them.
        /// </summary>
        /// <returns></returns>
        private Judgement GetBestJudgement() => Processor.CurrentJudgements.FirstOrDefault(x => x.Value == Processor.CurrentJudgements.Values.Max()).Key;
    }
}
