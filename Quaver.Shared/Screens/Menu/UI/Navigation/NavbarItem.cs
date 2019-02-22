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

namespace Quaver.Shared.Screens.Menu.UI.Navigation
{
    public class NavbarItem : Button
    {
        /// <summary>
        ///     The bottom line that displays when the item is selected/hovered.
        /// </summary>
        public Sprite BottomLine { get; private set; }

        /// <summary>
        ///     If the item is currently selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        ///     Dictates if the line is upside down
        /// </summary>
        private bool IsUpsideDown { get; set; }

        /// <summary>
        ///     Dictates if we'll always be showing the navbar line
        /// </summary>
        private bool AlwaysShowLine { get; set; }

        /// <summary>
        ///     The color of the line when selected
        /// </summary>
        private Color _lineColor = Color.White;
        public Color LineColor
        {
            get => _lineColor;
            set
            {
                _lineColor = value;
                BottomLine.Tint = _lineColor;
            }
        }

        /// <summary>
        ///     If true, the box will be highlighted when the button is hovered
        /// </summary>
        public bool HighlightOnHover { get; set; }

        /// <summary>
        /// </summary>
        public NavbarItem()
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Icon + Name
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        /// <param name="isUpsideDown"></param>
        /// <param name="alwaysShowLine"></param>
        public NavbarItem(string name, bool selected = false, EventHandler clickAction = null, bool isUpsideDown = false,
            bool alwaysShowLine = false, bool highlightOnHover = false) : base(clickAction)
        {
            Selected = selected;
            IsUpsideDown = isUpsideDown;
            AlwaysShowLine = alwaysShowLine;
            HighlightOnHover = highlightOnHover;

            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(175, 45);
            Tint = Color.Black;

            Alpha = Selected ? 0.25f: 0;

            var text = new SpriteText(Fonts.Exo2SemiBold, name, 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Y = 2
            };

            CreateBottomLine();
        }

        /// <summary>
        ///     Icon
        /// </summary>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        /// <param name="isUpsideDown"></param>
        /// <param name="alwaysShowLine"></param>
        /// <param name="highlightOnHover"></param>
        public NavbarItem(Texture2D tex, bool selected = false, EventHandler clickAction = null, bool isUpsideDown = false,
            bool alwaysShowLine = false, bool highlightOnHover = false) : base(clickAction)
        {
            Selected = selected;
            IsUpsideDown = isUpsideDown;
            AlwaysShowLine = alwaysShowLine;
            HighlightOnHover = highlightOnHover;

            UsePreviousSpriteBatchOptions = true;
            Size = new ScalableVector2(175, 45);
            Tint = Color.Black;

            Alpha = Selected ? 0.25f: 0;

            var image = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(136, 29),
                Image = tex
            };

            CreateBottomLine();
        }

        /// <summary>
        ///     Only icon
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        public NavbarItem(Texture2D icon, bool selected = false, EventHandler clickAction = null) : base(clickAction)
        {
            Selected = selected;
            UsePreviousSpriteBatchOptions = true;
            Tint = Color.Black;
            Alpha = Selected ? 0.25f: 0;
            Size = new ScalableVector2(45, 45);

            var centerIcon = new Sprite()
            {
                Parent = this,
                Image = icon,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width * 0.65f, Height * 0.65f),
            };

            CreateBottomLine();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (BottomLine != null)
            {
                if (Selected || IsHovered || AlwaysShowLine)
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, Width, (float) Math.Min(dt / 60, 1));
                else
                    BottomLine.Width = MathHelper.Lerp(BottomLine.Width, 0, (float) Math.Min(dt / 60, 1));
            }

            if (HighlightOnHover && !Selected)
                Alpha = MathHelper.Lerp(Alpha, HighlightOnHover && IsHovered ? 0.25f : 0, (float) Math.Min(dt / 240, 1));

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the bottom line for the item.
        /// </summary>
        protected void CreateBottomLine() => BottomLine = new Sprite()
        {
            Parent = this,
            Alignment = IsUpsideDown ? Alignment.TopCenter : Alignment.BotCenter,
            Size = new ScalableVector2(Selected ? Width : 0, 3),
            Y = IsUpsideDown ? -3 : 3,
            Tint = LineColor
        };
    }
}
