using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Input;

namespace Quaver.Graphics.Overlays.Chat.Components.Messages
{
    public class ChatMessageContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent overlay.
        /// </summary>
        private ChatOverlay Overlay { get; }

        /// <summary>
        ///     The channel this container is for.
        /// </summary>
        public ChatChannel Channel { get; }

        /// <summary>
        ///     The list of drawable chat messages in this container.
        /// </summary>
        private List<DrawableChatMessage> DrawableChatMessages { get; }

        /// <summary>
        ///     Stores the height of all of the messages combined.
        /// </summary>
        private float TotalMessageHeight { get; set; }

        /// <summary>
        ///     The divider line for this container.
        /// </summary>
        private Sprite DividerLine { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ChatMessageContainer(ChatOverlay overlay, ChatChannel channel)
            : base(new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height),
                new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height))
        {
            Overlay = overlay;
            Channel = channel;

            DrawableChatMessages = new List<DrawableChatMessage>();

            Parent = overlay.MessageContainer;
            SetChildrenVisibility = true;
            Y = Overlay.CurrentTopicContainer.Height;
            Tint = Color.Black;
            Alpha = 0.85f;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X -= 1;

            SpriteBatchOptions.BlendState = BlendState.NonPremultiplied;

            ScrollSpeed = 150;
            EasingType = Easing.EaseOutQuint;
            TimeToCompleteScroll = 1500;

            DividerLine = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 2),
                Y = -2,
                Alpha = 0.35f
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Only allow the container to be scrollable if the mouse is actually on top of the area.
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && Overlay.ActiveChannel == Channel;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Adds a drawable chat message to this container.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void AddMessage(ChatChannel channel, ChatMessage message)
        {
            // In the event that we need to add a new drawable, add one.
            var msg = new DrawableChatMessage(Overlay.ChannelMessageContainers[channel], message);

            // Calculate the message Y
            if (DrawableChatMessages.Count > 0)
            {
                var lastMessage = DrawableChatMessages.Last();
                msg.Y = lastMessage.Y + lastMessage.Height;
            }

            DrawableChatMessages.Add(msg);

            // Recalculate the height of the content container.
            TotalMessageHeight += msg.Height;

            if (TotalMessageHeight > Overlay.MessageContainer.Height - Overlay.CurrentTopicContainer.Height)
                ContentContainer.Height = TotalMessageHeight;

            AddContainedDrawable(msg);

            // Get how far we're scrolled down.
            var scrollDiff = ContentContainer.Height - Height - Math.Abs(ContentContainer.Y);

            // Depending on how scrolled up the user is, we'll want to snap back down to the bottom
            if (scrollDiff < 220)
                ScrollTo(-ContentContainer.Height, 800);

            msg.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, msg.X, 0, 200));
        }
    }
}