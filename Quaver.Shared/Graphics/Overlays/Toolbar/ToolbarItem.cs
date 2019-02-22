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
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Overlays.Toolbar
{
    public class ToolbarItem : Button
    {
        /// <summary>
        ///     The bottom line sprite, used to visually show if it's highlighted.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <summary>
        ///     If the toolbar item is actually selected.
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        ///     Keeps track of if a sound has played when hovering over the button.
        /// </summary>
        private bool HoverSoundPlayed { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates a left-aligned one w/ a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        internal ToolbarItem(string name, Action onClick, bool selected = false)
        {
            Size = new ScalableVector2(165, 45);
            Initialize(onClick, selected);

            // Create the text in the middle of the button.
            var text = new SpriteText(Fonts.Exo2Regular, name, 14)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
            };
        }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates toolbar item with an icon.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        internal ToolbarItem(Texture2D icon, Action onClick, bool selected = false)
        {
            Size = new ScalableVector2(70, 45);
            Initialize(onClick, selected);

            var iconSprite = new Sprite
            {
                Parent = this,
                Image = icon,
                Size = new ScalableVector2(20, 20),
                Alignment = Alignment.MidCenter
            };
        }

        /// <summary>
        ///     Initializes the toolbar item, used in the constructors.
        /// </summary>
        /// <param name="onClick"></param>
        /// <param name="selected"></param>
        private void Initialize(Action onClick, bool selected = false)
        {
            Clicked += (o, e) => onClick();
            IsSelected = selected;

            Tint = Color.Black;
            Alpha = IsSelected ? 0.1f : 0;

            BottomLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(Width, 1),
                Tint = Color.White,
                Width = IsSelected ? Width : 0,
                Y = 1
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Change the size of the line based on if it's hovered/already selected.
            if (IsHovered)
            {
                BottomLine.Width = MathHelper.Lerp(BottomLine.Width, Width, (float) Math.Min(GameBase.Game.TimeSinceLastFrame / 60f, 1));
                Alpha = 0.05f;

                // Make sure the hover sound only plays one time.
                if (!HoverSoundPlayed)
                {
                    SkinManager.Skin.SoundHover.CreateChannel().Play();
                    HoverSoundPlayed = true;
                }
            }
            else
            {
                HoverSoundPlayed = false;

                if (!IsSelected)
                {
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, 0, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60f, 1));
                    Alpha = 0f;
                }
                else
                {
                    Alpha = 0.1f;
                }
            }

            base.Update(gameTime);
        }
    }
}
