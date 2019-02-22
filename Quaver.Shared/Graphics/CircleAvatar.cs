/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics
{
    public class CircleAvatar : SpriteMaskContainer
    {
        /// <summary>
        ///     The sprite for the masked avatar.
        /// </summary>
        public Sprite AvatarSprite { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="image"></param>
        public CircleAvatar(ScalableVector2 size, Texture2D image)
        {
            Image = FontAwesome.Get(FontAwesomeIcon.fa_circle);
            Size = size;

            AvatarSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = Size,
                Image = image,
            };

            AddContainedSprite(AvatarSprite);
        }
    }
}
