/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultJudgementBreakdown : Sprite
    {
        /// <summary>
        ///     Reference to the parent score container
        /// </summary>
        private ResultScoreContainer Container { get; }

        /// <summary>
        ///     The ScoreProcessor that this
        /// </summary>
        private ScoreProcessor Processor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="processor"></param>
        public ResultJudgementBreakdown(ResultScoreContainer container, ScoreProcessor processor)
        {
            Container = container;
            Processor = processor;

            Size = new ScalableVector2(container.VerticalDividerLine.X - 2 * 2, 268);
            Tint = Color.Black;
            Alpha = 0;

            CreateGraph();
        }

        /// <summary>
        /// </summary>
        private void CreateGraph()
        {
            var i = 0;

            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j == Judgement.Ghost)
                    continue;

                var color = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[j];

                var judgementName = new SpriteTextBitmap(FontsBitmap.GothamRegular, j.ToString().ToUpper())
                {
                    Parent = this,
                    Y = i * 50 + 24,
                    X = 15,
                    Tint = color,
                    FontSize = 16
                };

                var percentage = Processor.CurrentJudgements[j] == 0 ? 0 : (float)Processor.CurrentJudgements[j] / Processor.TotalJudgementCount;

                var progressBar = new Sprite
                {
                    Parent = judgementName,
                    Alignment = Alignment.MidLeft,
                    Tint = color,
                    Height = 14,
                    Width = (Width - 300) * percentage,
                    X = 80,
                };

                if (progressBar.Width <= 1)
                    progressBar.Width = 1;

                var judgementAmount = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"{Processor.CurrentJudgements[j]:N0} ({percentage * 100:0.0}%)")
                {
                    Parent = progressBar,
                    Alignment = Alignment.MidLeft,
                    Tint = color,
                    X = progressBar.Width + 10,
                    FontSize = 15
                };

                var windows = new SpriteTextBitmap(FontsBitmap.GothamRegular, $"")
                {
                    Parent = progressBar,
                    Alignment = Alignment.MidLeft,
                    Tint = color,
                    X = judgementAmount.X + judgementAmount.Width + 3,
                    FontSize = 15
                };

                if (Container.StandardizedProcessor != null)
                    windows.Text = $" - {Processor.JudgementWindow[j] / ModHelper.GetRateFromMods(Processor.Mods)}ms";
                else if (Container.Screen.ResultsType == ResultScreenType.Score
                         && Container.Screen.Score.JudgementWindowPreset != JudgementWindowsDatabaseCache.Standard.Name
                         && Container.Screen.Score.JudgementWindowPreset != null)
                {
                    switch (j)
                    {
                        case Judgement.Marv:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowMarv + "ms";
                            break;
                        case Judgement.Perf:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowPerf + "ms";
                            break;
                        case Judgement.Great:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowGreat + "ms";
                            break;
                        case Judgement.Good:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowGood+ "ms";
                            break;
                        case Judgement.Okay:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowOkay + "ms";
                            break;
                        case Judgement.Miss:
                            windows.Text = " - " + Container.Screen.Score.JudgementWindowMiss + "ms";
                            break;
                    }
                }

                i++;
            }
        }
    }
}
