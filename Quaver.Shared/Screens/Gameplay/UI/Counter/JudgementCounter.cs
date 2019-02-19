/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Skinning;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.UI.Counter
{
    /// <inheritdoc />
    /// <summary>
    ///     Displays all the current judgements + KPS
    /// </summary>
    public class JudgementCounter : Container
    {
        /// <summary>
        ///     Reference to the ruleset.
        /// </summary>
        public GameplayScreen Screen { get; }

        /// <summary>
        ///     The list of judgement displays.
        /// </summary>
        private Dictionary<Judgement, JudgementCounterItem> JudgementDisplays { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        public JudgementCounter(GameplayScreen screen)
        {
            Screen = screen;

            // Create the judgement displays.
            JudgementDisplays = new Dictionary<Judgement, JudgementCounterItem>();

            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];
            for (var i = 0; i < Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count; i++)
            {
                var key = (Judgement)i;
                var color = SkinManager.Skin.Keys[Screen.Map.Mode].JudgeColors[key];

                // Default it to an inactive color.
                JudgementDisplays[key] = new JudgementCounterItem(this, key, new Color(color.R / 2, color.G / 2, color.B / 2),
                    new Vector2(skin.JudgementCounterSize, skin.JudgementCounterSize))
                {
                    Alignment = Alignment.MidRight,
                    Parent = this,
                    Image = SkinManager.Skin.JudgementOverlay,
                    Alpha = skin.JudgementCounterAlpha
                };

                // Normalize the position of the first one so that all the rest will be completely in the middle.
                if (i == 0)
                {
                    Y = Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count * -JudgementDisplays[key].Height / 2f;
                    continue;
                }

                JudgementDisplays[key].Y = JudgementDisplays[(Judgement)(i - 1)].Y + JudgementDisplays[key].Height + 5;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Update the judgement counts of each one.
            foreach (var item in JudgementDisplays)
            {
                if (Screen.Ruleset.ScoreProcessor.CurrentJudgements[item.Key] != JudgementDisplays[item.Key].JudgementCount)
                    JudgementDisplays[item.Key].JudgementCount = Screen.Ruleset.ScoreProcessor.CurrentJudgements[item.Key];

                UpdateTextAndSize(JudgementDisplays[item.Key], dt);
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Makes sure that the text is changed to a singular number when collapsing.
        /// </summary>
        /// <param name="counterItem"></param>
        /// <param name="dt"></param>
        private void UpdateTextAndSize(JudgementCounterItem counterItem, double dt)
        {
            // Tween size and pos back to normal
            var skin = SkinManager.Skin.Keys[Screen.Map.Mode];

            counterItem.Width = MathHelper.Lerp(counterItem.Width, skin.JudgementCounterSize, (float)Math.Min(dt / 180, 1));
            counterItem.Height = MathHelper.Lerp(counterItem.Height, skin.JudgementCounterSize, (float)Math.Min(dt / 180, 1));
            counterItem.X = MathHelper.Lerp(counterItem.X, 0, (float) Math.Min(dt / 180, 1));
        }
    }
}
