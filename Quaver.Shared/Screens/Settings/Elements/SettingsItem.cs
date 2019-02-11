/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsItem : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteText Name { get; }

        protected Color HoverColor { get; set; } = ColorHelper.HexToColor("#cacaca");

        protected Color UnhoverColor { get; set; } = Color.Black;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        public SettingsItem(SettingsDialog dialog, string name)
        {
            Size = new ScalableVector2(dialog.ContentContainer.Width - dialog.DividerLine.X - 10, 40);
            Tint = UnhoverColor;
            Alpha = 0.65f;

            Name = new SpriteText(Fonts.SourceSansProSemiBold, name, 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 20
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="name"></param>
        public SettingsItem(Drawable sprite, string name)
        {
            Size = new ScalableVector2(sprite.Width - 10, 40);
            Tint = Color.Black;
            Alpha = 0.65f;

            Name = new SpriteText(Fonts.Exo2SemiBold, name, 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 15
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                    ? HoverColor : UnhoverColor, gameTime.ElapsedGameTime.TotalMilliseconds, 70);

            base.Update(gameTime);
        }
    }
}
