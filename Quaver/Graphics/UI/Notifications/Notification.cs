using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.UI.Notifications
{
    internal class Notification : Button
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
        internal double TimeElapsedSinceShown { get; set;  }

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

            Size = new UDim2D(350, 80);
            Tint = Color.White;
            SetChildrenAlpha = true;

            Container = new Sprite
            {
                Parent = this,
                Tint = Colors.DarkGray,
                Size = new UDim2D(SizeX - 2, SizeY - 2),
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(1, 1)
            };

            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new UDim2D(Container.SizeY, Container.SizeY),
                PosX = 2,
                Image = image
            };
                    
            Content = new SpriteText
            {
                Parent = this,
                Font = Fonts.AllerRegular16,
                Alignment = Alignment.TopLeft,
                TextAlignment = Alignment.TopLeft,
                TextBoxStyle = TextBoxStyle.WordwrapMultiLine,
                Text = text,
                TextScale = 0.70f,
                Size = new UDim2D(SizeX - Avatar.SizeX, Container.SizeY),
                PosX = Avatar.PosX + Avatar.SizeX + 10,
                PosY = 10
            };
            
            // Set on click handler delegate if it exists.
            if (onClick != null)
                Clicked += onClick;

            Tint = BorderColor;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            FadeToColor(IsHovered ? Color.White : BorderColor, dt, 120);

            // Fade out super fast if the button was clicked.
            if (HasBeenClicked)
                Alpha = GraphicsHelper.Lerp(0, Alpha, Math.Min(dt / 60, 1));
                
            // Fade out if it has been shown a long time.
            if (!HasBeenClicked && TimeElapsedSinceShown >= 1200)
                Alpha = GraphicsHelper.Lerp(0, Alpha, Math.Min(dt / 240, 1));
            
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOut()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOver()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnClicked()
        {
            HasBeenClicked = true;
            base.OnClicked();
        }
    }
}