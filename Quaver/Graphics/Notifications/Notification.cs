using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Resources;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Graphics.Notifications
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
        ///     The notification content text.
        /// </summary>
        private SpriteText Content { get; }

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

            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Container.Height, Container.Height),
                X = 2,
                Image = image
            };

            Content = new SpriteText(BitmapFonts.Exo2SemiBold, text, 12, true, (int) (Width - Avatar.Width - 5))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                X = Avatar.X + Avatar.Width + 5,
                Y = 10
            };

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
                Alpha = MathHelper.Lerp(Alpha, 0, (float) Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));

            // Fade out if it has been shown a long time.
            if (!HasBeenClicked && TimeElapsedSinceShown >= 2500)
                Alpha = MathHelper.Lerp(Alpha, 0, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 240, 1));

            base.Update(gameTime);
        }
    }
}
