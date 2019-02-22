/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataScrollContainer : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private EditorMetadataChanger Changer { get; }

        /// <summary>
        /// </summary>
        public List<EditorMetadataItem> Items { get; }

        /// <summary>
        /// </summary>
        /// <param name="changer"></param>
        /// <param name="items"></param>
        public EditorMetadataScrollContainer(EditorMetadataChanger changer, List<EditorMetadataItem> items) : base(new ScalableVector2(0, 0), new ScalableVector2(0, 0))
        {
            Changer = changer;
            Items = items;
            Size = new ScalableVector2(Changer.Width, Changer.Height - Changer.HeaderBackground.Height - Changer.FooterBackground.Height);
            Alpha = 0;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X += 8;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            var totalHeight = 0f;

            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                AddContainedDrawable(item);
                totalHeight += item.Height;

                const int spacing = 13;

                if (i == 0)
                {
                    item.Y = spacing;
                    continue;
                }

                item.Y = Items[i - 1].Y + Items[i - 1].Height + spacing;
                totalHeight += spacing;
            }

            if (ContentContainer.Height < totalHeight)
                ContentContainer.Height = Height;
        }
    }
}
