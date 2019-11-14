using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Overlays.Chatting.Messages.Scrolling;
using Quaver.Shared.Graphics.Overlays.Chatting.Messages.Textbox;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages
{
    public class ChatMessageContainer : Sprite, IResizable
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChannel { get; }

        /// <summary>
        /// </summary>
        private ChannelTopicHeader TopicHeader { get; set; }

        /// <summary>
        /// </summary>
        public ChatTextboxContainer TextboxContainer { get; private set; }

        /// <summary>
        ///     Contains the message scroll containers for each channel
        /// </summary>
        public Dictionary<ChatChannel, ChatMessageScrollContainer> MessageScrollContainers { get; } = new Dictionary<ChatChannel, ChatMessageScrollContainer>();

        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="size"></param>
        public ChatMessageContainer(Bindable<ChatChannel> activeChannel, ScalableVector2 size)
        {
            ActiveChannel = activeChannel;
            Size = size;
            Alpha = 0;

            CreateTopicHeader();
            CreateTextboxContainer();

            // Create a channel for all available container
            if (OnlineChat.JoinedChatChannels.Count != 0)
            {
                OnlineChat.JoinedChatChannels.ForEach(AddChannel);
                ActiveChannel.Value = OnlineChat.JoinedChatChannels.First();
            }

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Make sure the correct channel is displayed
            foreach (var container in MessageScrollContainers.Values)
            {
                if (ActiveChannel.Value == container.Channel && container.Parent != this)
                    container.Parent = this;
                else if (ActiveChannel.Value != container.Channel && container.Parent != null)
                    container.Parent = null;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTopicHeader()
            => TopicHeader = new ChannelTopicHeader(ActiveChannel, new ScalableVector2(Width, 56)) { Parent = this };

        /// <summary>
        /// </summary>
        private void CreateTextboxContainer() => TextboxContainer = new ChatTextboxContainer(ActiveChannel,
            new ScalableVector2(Width, 56))
        {
            Parent = this,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Height = size.Y.Value;

            foreach (var child in Children)
            {
                if (child is IResizable c)
                    c.ChangeSize(size);
            }

            foreach (var channel in MessageScrollContainers)
                channel.Value.ChangeSize(size);
        }

        /// <summary>
        /// </summary>
        /// <param name="chan"></param>
        public void AddChannel(ChatChannel chan)
        {
            if (MessageScrollContainers.ContainsKey(chan))
                return;

            var container = new ChatMessageScrollContainer(chan, new ScalableVector2(Width,
                Height - TopicHeader.Height - TextboxContainer.Height), TopicHeader.Height, TextboxContainer.Height)
            {
                Y = TopicHeader.Height,
                DestroyIfParentIsNull = false
            };

            MessageScrollContainers.Add(chan, container);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected)
            {
                UnsubcribeFromEvents();
                return;
            }

            SubscribeToEvents();
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            OnlineManager.Client.OnJoinedChatChannel += OnJoinedChatChannel;
        }

        /// <summary>
        /// </summary>
        private void UnsubcribeFromEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnJoinedChatChannel -= OnJoinedChatChannel;
        }

        /// <summary>
        ///     Called when joining a channel. Creates a new message container
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinedChatChannel(object sender, JoinedChatChannelEventArgs e)
        {
            var chan = OnlineChat.JoinedChatChannels.Find(x => x.Name == e.Channel);

            if (chan == null)
                return;

            AddChannel(chan);
        }
    }
}