/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultKeyValueItem : Sprite
    {
        public ResultKeyValueItemType Type { get; }

        /// <summary>
        ///     The key or "Heading"
        /// </summary>
        public SpriteText TextKey { get; private set; }

        /// <summary>
        ///     The value this text represents
        /// </summary>
        public SpriteText TextValue { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public ResultKeyValueItem(ResultKeyValueItemType type, string key, string value)
        {
            Type = type;
            Alpha = 0f;
            Tint = Color.CornflowerBlue;

            CreateTextKey(key);
            CreateTextValue(value);

            switch (Type)
            {
                case ResultKeyValueItemType.Vertical:
                    Size = new ScalableVector2(Math.Max(TextKey.Width, TextValue.Width), TextKey.Height + TextValue.Height + 5);
                    break;
                case ResultKeyValueItemType.Horizontal:
                    Size = new ScalableVector2(TextKey.Width + 5 + TextValue.Width, TextKey.Height);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        private void CreateTextKey(string key) => TextKey = new SpriteText(Fonts.SourceSansProSemiBold, key, 13)
        {
            Parent = this,
            Alignment = Type == ResultKeyValueItemType.Vertical ? Alignment.TopCenter : Alignment.TopLeft,
            Tint = Colors.SecondaryAccent
        };

        /// <summary>

        /// </summary>
        /// <param name="value"></param>
        private void CreateTextValue(string value)
        {
            TextValue = new SpriteText(Fonts.SourceSansProSemiBold, value, 13)
            {
                Parent = this,
                Alignment = Type == ResultKeyValueItemType.Vertical ? Alignment.TopCenter : Alignment.TopLeft,
                Y = Type == ResultKeyValueItemType.Vertical ? TextKey.Height + 5 : 0,
            };

            if (Type == ResultKeyValueItemType.Horizontal)
                TextValue.X = TextKey.Width + 5;
        }
    }

    public enum ResultKeyValueItemType
    {
        Vertical,
        Horizontal
    }
}
