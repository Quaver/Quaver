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

namespace Quaver.Shared.Graphics.Online.Playercard
{
    public class IconedText : Sprite
    {
        /// <summary>
        ///     The sprite for the icon.
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        ///     The text for the icon.
        /// </summary>
        public SpriteText Value { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        public IconedText(Texture2D icon, string text)
        {
            Alpha = 0;
            SetChildrenVisibility = true;

            Icon = new Sprite
            {
                Parent = this,
                Image = icon,
                Size = new ScalableVector2(20, 20),
                UsePreviousSpriteBatchOptions = true,
            };

            Value = new SpriteText(Fonts.Exo2BoldItalic, " ", 24)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                X = Icon.Width + 2,
                Y = 2
            };

            UpdateValue(text);
        }

        /// <summary>
        ///     Updates the value of the text.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateValue(string text)
        {
            Value.Text = text;
            Value.Size = new ScalableVector2(Value.Width * 0.45f, Value.Height * 0.45f);

            // Calculate the size of the entire thing
            Size = new ScalableVector2(Value.X + Value.Width, Icon.Height);
        }
    }
}
