/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Menu.UI.Buttons
{
    public class ToolButton : ImageButton
    {
        /// <summary>
        ///     The icon current
        /// </summary>
        private Sprite Icon { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="middleImage"></param>
        /// <param name="clickAction"></param>
        public ToolButton(Texture2D middleImage, EventHandler clickAction = null) : base(UserInterface.BlankBox, clickAction)
        {
            Tint = Color.Black;
            Alpha = 0.25f;
            Size = new ScalableVector2(56, 48);
            Icon = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(28, 28),
                Alignment = Alignment.MidCenter,
                Image = middleImage
            };
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(IsHovered ? Color.White : Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            base.Update(gameTime);
        }
    }
}
