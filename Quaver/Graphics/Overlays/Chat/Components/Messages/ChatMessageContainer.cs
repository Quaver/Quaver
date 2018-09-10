using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Overlays.Chat.Components.Messages.Drawable;
using Quaver.Server.Client.Structures;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ChatMessageContainer(ChatOverlay overlay, ChatChannel channel)
            : base(new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height),
                new ScalableVector2(overlay.MessageContainer.Width, overlay.MessageContainer.Height - overlay.CurrentTopicContainer.Height + 1))
        {
            Overlay = overlay;
            Channel = channel;

            DrawableChatMessages = new List<DrawableChatMessage>();

            Parent = overlay.MessageContainer;
            SetChildrenVisibility = true;
            Y = Overlay.CurrentTopicContainer.Height;
            Tint = Color.Black;

            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 8;
            Scrollbar.X -= 1;

            SpriteBatchOptions.BlendState = BlendState.NonPremultiplied;
        }

        /// <summary>
        ///     Adds a drawable chat message to this container.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public void AddMessage(ChatChannel channel, ChatMessage message)
        {
            // Check the last message in the channel.
            /*if (DrawableChatMessages.Count > 0)
            {
                var lastMessage = DrawableChatMessages.Last();

                // If the last message in the channel is by the same user, we'll want to just add onto their
                // previous message.
                if (lastMessage != null && lastMessage.Message.SenderId == message.SenderId)
                {
                    var currTime = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                    if (currTime - lastMessage.Message.Time < 5000)
                    {
                        lastMessage.TextMessageContent.Text += "\n" + message.Message;
                        lastMessage.RecalculateHeight();

                        // Change the last message's current time to now, so that later messages can receive the same effect.
                        lastMessage.Message.Time = currTime;
                        return;
                    }
                }
            }*/

            // In the event that we need to add a new drawable, add one.
            var msg = new DrawableChatMessage(Overlay.ChannelMessageContainers[channel], message);

            // Calculate the message Y
            if (DrawableChatMessages.Count > 0)
            {
                var lastMessage = DrawableChatMessages.Last();
                msg.Y = lastMessage.Y + lastMessage.Height;
            }

            DrawableChatMessages.Add(msg);
            AddContainedDrawable(msg);

            msg.Transformations.Add(new Transformation(TransformationProperty.X, Easing.Linear, msg.X, 0, 150));
        }
    }
}