/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Menu.UI.Jukebox
{
    public class JukeboxButton : ImageButton
    {
        public JukeboxButton(Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.75f : 1, (float) Math.Min(dt / 60, 1));

            base.Update(gameTime);
        }
    }
}
