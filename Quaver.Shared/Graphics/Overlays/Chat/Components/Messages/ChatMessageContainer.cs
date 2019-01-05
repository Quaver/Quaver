/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Overlays.Chat.Components.Messages.Drawable;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Messages
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
        public ChatChannel Channel { get; set; }

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

        /// <summary>
        ///     The y position of the content container in the previous frame.
        /// </summary>
        public float PreviousContentContainerY { get; set; }

        /// <summary>
        ///     The maximum amount of messages to be shown at a given time.
        /// </summary>
        public const int MAX_MESSAGES_SHOWN = 15;

        /// <summary>
        ///     The index at which the messages are starting to be shown.
        /// </summary>
        public int PoolStartingIndex { get; set; }

        public bool PurgeInProgress { get; set; }

        /// <summary>
        ///     A queue of messages waiting to be filled into the chat.
        /// </summary>
        private List<ChatMessage> MessageQueue { get; set; } = new List<ChatMessage>();

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

            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
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
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && Overlay.ActiveChannel == Channel && Overlay.IsOnTop;

            // Handle pool shifting when scrolling up or down.
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;
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

            // We might not have user information ready, so request it.
            if (!msg.Message.Sender.HasUserInfo)
                OnlineManager.Client.RequestUserInfo(new List<int> { msg.Message.SenderId });

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

            // Get how far we're scrolled down.
            var scrollDiff = ContentContainer.Height - Height - Math.Abs(ContentContainer.Y);

            // Depending on how scrolled up the user is, we'll want to snap back down to the bottom
            if (scrollDiff < 220)
                ScrollTo(-ContentContainer.Height, 800);

            AddContainedDrawable(msg);
            msg.Animations.Add(new Animation(AnimationProperty.X, Easing.OutQuint, msg.X, 0, 400));
        }

        /// <summary>
        ///     Handles shifting of the pool based on the way the user has scrolled.
        /// </summary>
        private void HandlePoolShifting(Direction direction)
        {
            DrawableChatMessage message;

            switch (direction)
            {
                case Direction.Forward:
                    // First run a check to see if we even have a message in this position.
                    if (DrawableChatMessages.ElementAtOrDefault(PoolStartingIndex) == null
                        || DrawableChatMessages.ElementAtOrDefault(PoolStartingIndex + MAX_MESSAGES_SHOWN) == null)
                        return;

                    // Check the top message at the pool starting index to see if it is still in range
                    message = DrawableChatMessages[PoolStartingIndex];

                    var newRect = Rectangle.Intersect(message.ScreenRectangle, ScreenRectangle);

                    if (!newRect.IsEmpty)
                        return;

                    // Since we're shifting forward, we can safely remove the button that has gone off-screen.
                    RemoveContainedDrawable(DrawableChatMessages[PoolStartingIndex]);

                    // Now add the button that is forward.
                    AddContainedDrawable(DrawableChatMessages[PoolStartingIndex + MAX_MESSAGES_SHOWN]);

                    // Increment the starting index to shift it.
                    PoolStartingIndex++;
                    break;
                case Direction.Backward:
                    // First run a check to see if we even have a message in this position.
                    if (DrawableChatMessages.ElementAtOrDefault(PoolStartingIndex - 1) == null
                        || DrawableChatMessages.ElementAtOrDefault(PoolStartingIndex + MAX_MESSAGES_SHOWN - 1) == null)
                        return;

                    message = DrawableChatMessages[PoolStartingIndex + MAX_MESSAGES_SHOWN - 1];

                    var rect = Rectangle.Intersect(message.ScreenRectangle, ScreenRectangle);

                    if (!rect.IsEmpty)
                        return;

                    // Since we're scrolling up, we need to shift backwards.
                    // Remove the drawable from the bottom one.
                    RemoveContainedDrawable(DrawableChatMessages[PoolStartingIndex + MAX_MESSAGES_SHOWN - 1]);
                    AddContainedDrawable(DrawableChatMessages[PoolStartingIndex - 1]);

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        ///    Purges messages from a given user.
        /// </summary>
        /// <param name="id"></param>
        public void PurgeUserMessages(int id)
        {
            lock (DrawableChatMessages)
            {
                // A cached version of the muted text, so we don't have to keep creating the same one.
                Texture2D mutedTextImage = null;
                var mutedTextSize = new ScalableVector2(0, 0);

                // Scan the previous 50 chat messages.
                for (var i = DrawableChatMessages.Count - 1; i >= 0 && i >= DrawableChatMessages.Count - 50; i--)
                {
                    var msg = DrawableChatMessages[i];

                    if (msg.Message.Sender.OnlineUser.Id != id)
                        continue;

                    // If a message is red, then you know it is already past muted.
                    if (msg.TextMessageContent.Tint == Color.Crimson)
                        break;

                    if (mutedTextImage != null)
                    {
                        msg.TextMessageContent.Tint = Color.Crimson;
                        msg.TextMessageContent.Image = mutedTextImage;
                        msg.TextMessageContent.Size = mutedTextSize;
                        continue;
                    }

                    msg.TextMessageContent.Tint = Color.Crimson;
                    msg.TextMessageContent.Text = "This message has been removed by a moderator.";
                    mutedTextImage = msg.TextMessageContent.Image;
                    mutedTextSize = msg.TextMessageContent.Size;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        public void PurgeUserMessagesBroken(int userId)
        {
            lock (DrawableChatMessages)
            lock (ContentContainer.Animations)
            {
                for (var i = DrawableChatMessages.Count - 1; i >= 0; i--)
                {
                    var msg = DrawableChatMessages[i];

                    if (msg.Message.Sender.OnlineUser.Id != userId)
                        continue;

                    RemoveContainedDrawable(msg);
                    DrawableChatMessages.Remove(msg);

                    TotalMessageHeight -= msg.Height;
                    PoolStartingIndex--;

                    if (PoolStartingIndex <= 0)
                        PoolStartingIndex = 0;
                }

                for (var i = 0; i < DrawableChatMessages.Count; i++)
                {
                    var msg = DrawableChatMessages[i];

                    if (i == 0)
                    {
                        msg.Y = 0;
                        continue;
                    }

                    msg.Y = DrawableChatMessages[i - 1].Y + DrawableChatMessages[i - 1].Height;
                }

                if (TotalMessageHeight > Overlay.MessageContainer.Height - Overlay.CurrentTopicContainer.Height)
                    ContentContainer.Height = TotalMessageHeight;
                else
                    ContentContainer.Height = Overlay.MessageContainer.Height - Overlay.CurrentTopicContainer.Height;

                ContentContainer.ClearAnimations();
                ContentContainer.Y = -ContentContainer.Height;
                PreviousContentContainerY = -ContentContainer.Height;
                TargetY = -ContentContainer.Height;
                PreviousTargetY = -ContentContainer.Height;

                for (var i = 0; i < DrawableChatMessages.Count; i++)
                {
                    if (i >= DrawableChatMessages.Count)
                        break;

                    var msg = DrawableChatMessages[i];

                    if (i >= PoolStartingIndex)
                    {
                        if (msg.Parent != ContentContainer)
                            AddContainedDrawable(msg);
                    }
                    else
                    {
                        if (msg.Parent == ContentContainer)
                            RemoveContainedDrawable(msg);
                    }
                }
            }
        }
    }
}
