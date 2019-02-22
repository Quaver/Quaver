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

namespace Quaver.Shared.Graphics.Overlays.Toolbar
{
    public class Toolbar : Sprite
    {
                /// <summary>
        ///     The line at the bottom of the toolbar.
        /// </summary>
        internal Sprite BottomLine { get; }

        /// <summary>
        ///     The buttons (left-aligned) displayed on the toolbar.
        /// </summary>
        private List<ToolbarItem> Buttons { get; }

        /// <summary>
        ///     The icons (right-aligned)
        /// </summary>
        private List<ToolbarItem> Icons { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="buttons"></param>
        /// <param name="icons"></param>
        /// <param name="size"></param>
        internal Toolbar(List<ToolbarItem> buttons, List<ToolbarItem> icons, ScalableVector2 size)
        {
            Buttons = buttons;
            Icons = icons;
            Size = size;
            Tint = Color.Black;
            Y = 0;
            Alpha = 0f;

            BottomLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Tint = Color.White,
                Width = Width - 160,
                Height = 1,
                Alpha = 0.3f,
                Y = Height
            };

            InitializeToolbarItems();
            InitializeIcons();
        }

        /// <summary>
        ///     Inits left aligned options.
        /// </summary>
        private void InitializeToolbarItems()
        {
            for (var i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];

                button.Parent = BottomLine;
                button.Y = -button.Height;

                if (i != 0)
                    button.X = Buttons[i - 1].Width * i;
            }
        }

        /// <summary>
        ///     Inits right aligned icons.
        /// </summary>
        private void InitializeIcons()
        {
            for (var i = 0; i < Icons.Count; i++)
            {
                var icon = Icons[i];

                icon.Parent = BottomLine;
                icon.Y = -icon.Height;

                icon.X = BottomLine.Width - icon.Width * i - icon.Width;
            }
        }
    }
}
