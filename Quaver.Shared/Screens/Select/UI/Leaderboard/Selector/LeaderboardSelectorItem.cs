/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard.Selector
{
    public class LeaderboardSelectorItem : Button
    {
        /// <summary>
        ///     The SpriteText that displays the item of the selector.
        /// </summary>
        private SpriteText ItemText { get; }

        /// <summary>
        ///     The line at the bottom of the item.
        /// </summary>
        private Sprite BottomLine { get; }

        /// <summary>
        ///     Determines if the selector item is
        /// </summary>
        public bool Selected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="selected"></param>
        /// <param name="action"></param>
        public LeaderboardSelectorItem(string text, bool selected = false, EventHandler action = null)
        {
            Selected = selected;
            Size = new ScalableVector2(145, 40);
            Tint = Color.Black;
            Alpha = Selected ? 0.25f: 0;

            ItemText = new SpriteText(Fonts.Exo2SemiBold, text, 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            BottomLine = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(Selected ? Width : 0, 3)
            };

            if (action != null)
                Clicked += action;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (BottomLine != null)
            {
                if (Selected || IsHovered)
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, Width, (float) Math.Min(dt / 60, 1));
                else
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, 0, (float) Math.Min(dt / 60, 1));
            }

            base.Update(gameTime);
        }
    }
}
