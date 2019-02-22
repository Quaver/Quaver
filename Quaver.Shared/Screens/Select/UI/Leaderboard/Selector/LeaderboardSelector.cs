/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard.Selector
{
    public class LeaderboardSelector : Sprite
    {
        /// <summary>
        ///     Reference to the parent view.
        /// </summary>
        public SelectScreenView View { get; }

        /// <summary>
        ///     The line displayed at the bottom of the selector.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        ///     The list of buttons to click on in the leaderboard.
        /// </summary>
        private List<LeaderboardSelectorItem> Items { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="items"></param>
        public LeaderboardSelector(SelectScreenView view, List<LeaderboardSelectorItem> items)
        {
            Items = items;
            View = view;
            Position = new ScalableVector2(28 - View.Banner.Border.Thickness, View.Banner.Y + View.Banner.Height + 10);
            Size = new ScalableVector2(View.Banner.Width, 40);

            Tint = Color.CornflowerBlue;
            Alpha = 0;

            CreateBottomLine();
            AlignItems();
        }

        /// <summary>
        ///    Creates the line at the bottom of the selector.
        /// </summary>
        private void CreateBottomLine() => BottomLine = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 2),
            Alpha = 0.90f
        };

        /// <summary>
        ///     Aligns the items from left to right.
        /// </summary>
        private void AlignItems()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                item.Parent = this;
                item.X = i * item.Width;
            }
        }
    }
}
