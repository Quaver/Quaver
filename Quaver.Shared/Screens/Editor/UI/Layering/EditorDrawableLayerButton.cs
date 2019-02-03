/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorDrawableLayerButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private EditorLayerCompositor Compositor { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="compositor"></param>
        /// <param name="image"></param>
        /// <param name="clickAction"></param>
        public EditorDrawableLayerButton(EditorLayerCompositor compositor, Texture2D image, EventHandler clickAction = null)
            : base(image, clickAction) => Compositor = compositor;

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = Rectangle.Intersect(ScreenRectangle, Compositor.ScrollContainer.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}
