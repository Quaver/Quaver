using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Results.UI.Data
{
    internal class JudgementBreakdown : Sprite
    {
        /// <summary>
        ///     Reference to the score processor.
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <summary>
        ///     The text that displays
        /// </summary>
        private SpriteText HeaderText { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="processor"></param>
        internal JudgementBreakdown(ScoreProcessor processor)
        {
            Processor = processor;

            Size = new UDim2D(( GameBase.WindowRectangle.Width - 120 ) / 2f, 350);
            PosX = -SizeX / 2f - 10;

            Alpha = 0.35f;
            Tint = Color.Black;

            HeaderText = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Font = Fonts.AllerRegular16,
                TextColor = Colors.MainAccent,
                Text = "JUDGEMENT BREAKDOWN"
            };

            var headerSize = HeaderText.MeasureString();
            HeaderText.PosX = headerSize.X / 2f + 25;
            HeaderText.PosY = headerSize.Y / 2f + 20;

            // ReSharper disable once ObjectCreationAsStatement
            // Line below the header.
            var lineBreak = new Sprite
            {
                Parent = this,
                Size = new UDim2D(SizeX * 0.75f, 1f),
                Position = new UDim2D(HeaderText.PosX - headerSize.X / 2f - 2, HeaderText.PosY + headerSize.Y / 2f + 5),
                Tint = Color.White,
                Alpha = 0.85f
            };

            // It's very wonky to make a for loop out of this, so we'll just use a foreach here.
            var i = 0;
            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                var name = new SpriteText
                {
                    Parent = this,
                    Font = Fonts.AssistantRegular16,
                    Text = j.ToString(),
                    PosY = lineBreak.PosY + lineBreak.SizeY + i * 45 + 30
                };

                name.PosX = name.MeasureString().X / 2f + 25;

                var color = GameBase.Skin.Keys[GameMode.Keys4].JudgeColors[j];

                // Find the percentage of the current judgement.
                var percentage = (float)Processor.CurrentJudgements[j] / Processor.TotalJudgementCount;

                // Draw Progress Bar
                var progressBar = new Sprite
                {
                    Parent = this,
                    Position = new UDim2D(100, name.PosY - name.MeasureString().Y / 8f),
                    Tint = color,
                    SizeY = 15,
                    SizeX = (SizeX - 100) * percentage
                };

                if (progressBar.SizeX < 1)
                    progressBar.SizeX = 1;

                var judgementAmount = new SpriteText()
                {
                    Parent = this,
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
        }
    }
}