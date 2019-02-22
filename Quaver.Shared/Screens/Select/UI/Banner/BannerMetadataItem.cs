/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Select.UI.Banner
{
    public class BannerMetadataItem : Sprite
    {
        /// <summary>
        ///     Text that displays the key of the metadata item
        /// </summary>
        private SpriteText Key { get; }

        /// <summary>
        ///     Text that displays the value of the metadata item.
        /// </summary>
        private SpriteText Value { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="font"></param>
        /// <param name="fontSize"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public BannerMetadataItem(string font, int fontSize, string key, string value)
        {
            Key = new SpriteText(font, key + ":", fontSize) { Parent = this, };

            Value = new SpriteText(font, value, fontSize)
            {
                Parent = this,
                X = Key.Width + 2,
                Tint = Colors.SecondaryAccent
            };

            Size = new ScalableVector2(Key.Width + Value.Width + 2, Key.Height);
            Alpha = 0;
        }

        /// <summary>
        ///     Updates the value oif the metadata item.
        /// </summary>
        /// <param name="val"></param>
        public void UpdateValue(string val)
        {
            Value.Text = val;
            Size = new ScalableVector2(Key.Width + Value.Width + 2, Key.Height);
        }
    }
}
