/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
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
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The list of judgement displays.
        /// </summary>
        private Dictionary<Judgement, JudgementCounterItem> JudgementDisplays { get; }

        /// <summary>
        ///     The size of each display item.
        /// </summary>
        public static Vector2 DisplayItemSize { get; } = new Vector2(45, 45);

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        public JudgementCounter(GameplayScreen screen)
        {
            Screen = screen;

            // Create the judgement displays.
            JudgementDisplays = new Dictionary<Judgement, JudgementCounterItem>();
            for (var i = 0; i < Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count; i++)
            {
                var key = (Judgement)i;
                var color = SkinManager.Skin.Keys[Screen.Map.Mode].JudgeColors[key];

                // Default it to an inactive color.
                JudgementDisplays[key] = new JudgementCounterItem(this, key, new Color(color.R / 2, color.G / 2, color.B / 2), new Vector2(DisplayItemSize.Y, DisplayItemSize.Y))
                {
                    Alignment = Alignment.MidRight,
                    Parent = this,
                    Image = SkinManager.Skin.JudgementOverlay,
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
        private static void UpdateTextAndSize(JudgementCounterItem counterItem, double dt)
        {
            // Tween size and pos back to normal
            counterItem.Width = MathHelper.Lerp(counterItem.Width, DisplayItemSize.Y, (float)Math.Min(dt / 180, 1));
            counterItem.Height = MathHelper.Lerp(counterItem.Height, DisplayItemSize.Y, (float)Math.Min(dt / 180, 1));
            counterItem.X = MathHelper.Lerp(counterItem.X, 0, (float) Math.Min(dt / 180, 1));
        }
    }
}
