/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Notifications
{
    public class Notification : Button
    {
        /// <summary>
        ///     The color of the notification.
        /// </summary>
        private Color BorderColor { get; }

        /// <summary>
        ///     The container for the actual notification.
        ///     The notification sprite itself is just a border.
        /// </summary>
        private Sprite Container { get; }

        /// <summary>
        ///     This container is used for centering and scaling the avatar sprite while maintaining a set width and height.
        ///
        ///     This was done due to the fact that Texture2D can not be scaled without affecting the sprite's width and height values.
        /// </summary>
        private Container AvatarContainer { get; }

        /// <summary>
        ///     The notification content text.
        /// </summary>
        private SpriteTextBitmap Content { get; }

        /// <summary>
        ///     The avatar sprite of the notification, depending on the type.
        /// </summary>
        private Sprite Avatar { get; }

        /// <summary>
        ///     The time this notification has been fully shown without
        ///     being hovered over.
        /// </summary>
        internal double TimeElapsedSinceShown { get; set; }

        /// <summary>
        ///     Keeps track of if the notification has been clicked, so we can fade it out
        /// </summary>
        internal bool HasBeenClicked { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="image"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="onClick"></param>
        internal Notification(Texture2D image, string text, Color color, EventHandler onClick = null)
        {
            BorderColor = color;
            Size = new ScalableVector2(350, 80);
            Tint = Color.White;
            SetChildrenAlpha = true;

            Container = new Sprite
            {
                Parent = this,
                Tint = Colors.DarkGray,
                Size = new ScalableVector2(Width - 2, Height - 2),
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(1, 1)
            };

            AvatarContainer = new Container
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Container.Height, Container.Height),
                X = 2,
            };

            Avatar = new Sprite
            {
                Parent = AvatarContainer,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(AvatarContainer.Height * 0.75f, AvatarContainer.Height * 0.75f),
                Image = image
            };

            Content = new SpriteTextBitmap(FontsBitmap.GothamRegular, text)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = AvatarContainer.X + AvatarContainer.Width + 5,
                FontSize = 15,
                MaxWidth = (int)(Width - AvatarContainer.Width - 25),
                Y = 10
            };

            AdjustHeightIfRequired();

            Clicked += (o, e) => HasBeenClicked = true;

            // Set on click handler delegate if it exists.
            if (onClick != null)
                Clicked += onClick;

            Tint = BorderColor;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(IsHovered ? Color.White : BorderColor, gameTime.ElapsedGameTime.TotalMilliseconds, 120);

            // Fade out super fast if the button was clicked.
            if (HasBeenClicked)
                Alpha = MathHelper.Lerp(Alpha, 0, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));

            // Fade out if it has been shown a long time.
            if (!HasBeenClicked && TimeElapsedSinceShown >= 4000)
                Alpha = MathHelper.Lerp(Alpha, 0, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 400, 1));

            Avatar.Alpha = Alpha;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (RectangleF.Intersection(ScreenRectangle, NotificationManager.Container.ScreenRectangle).IsEmpty)
                return;

            base.Draw(gameTime);
        }

        private void AdjustHeightIfRequired()
        {
            if (Content.Height < Height)
                return;

            Height = Content.Height + 10 + Content.Y;
            Container.Height = Height - 2;
        }
    }
}