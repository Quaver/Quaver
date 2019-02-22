/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Navigation
{
    public class EditorNavigationBar : Sprite
    {
        /// <summary>
        ///     The items aligned to the left side of the navigation bar.
        /// </summary>
        private List<EditorControlButton> LeftAlignedItems { get; }

        /// <summary>
        ///     The items aligned to the left side of the navigation bar.
        /// </summary>
        private List<EditorControlButton> RightAlignedItems { get; }

        /// <summary>
        /// </summary>
        /// <param name="leftAlignedItems"></param>
        /// <param name="rightAlignedItems"></param>
        public EditorNavigationBar(List<EditorControlButton> leftAlignedItems, List<EditorControlButton> rightAlignedItems)
        {
            Size = new ScalableVector2(WindowManager.Width, 36);
            Tint = ColorHelper.HexToColor("#1d1d1d");

            LeftAlignedItems = leftAlignedItems;
            RightAlignedItems = rightAlignedItems;
            AlignLeftItems();
            AlignRightItems();
        }

        /// <summary>
        /// </summary>
        private void AlignLeftItems()
        {
            for (var i = 0; i < LeftAlignedItems.Count; i++)
            {
                LeftAlignedItems[i].Parent = this;
                LeftAlignedItems[i].Alignment = Alignment.MidLeft;
                LeftAlignedItems[i].Size = new ScalableVector2(Height * 0.65f, Height * 0.65f);
                LeftAlignedItems[i].X = i * LeftAlignedItems[i].Width + ( i * 24 ) + 20;
            }
        }

        /// <summary>
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightAlignedItems.Count; i++)
            {
                RightAlignedItems[i].Parent = this;
                RightAlignedItems[i].Alignment = Alignment.MidRight;
                RightAlignedItems[i].Size = new ScalableVector2(Height * 0.65f, Height * 0.65f);
                RightAlignedItems[i].X = i * -RightAlignedItems[i].Width + ( i * -24 ) - 20;
            }
        }
    }
}
