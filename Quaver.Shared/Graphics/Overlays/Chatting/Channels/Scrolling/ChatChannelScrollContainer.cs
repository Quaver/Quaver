using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Channels.Scrolling
{
    public class ChatChannelScrollContainer : PoolableScrollContainer<ChatChannel>, IResizable
    {
        /// <summary>
        /// </summary>
        private Bindable<ChatChannel> ActiveChatChannel { get; }

        /// <summary>
        ///     The size of the header. Used to determine the amount we should subtract when
        ///     resizing the container
        /// </summary>
        private float HeaderHeight { get; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="activeChannel"></param>
        /// <param name="headerHeight"></param>
        /// <param name="size"></param>
        public ChatChannelScrollContainer(Bindable<ChatChannel> activeChannel, float headerHeight, ScalableVector2 size)
            : base(OnlineChat.JoinedChatChannels, int.MaxValue, 0, size, size)
        {
            ActiveChatChannel = activeChannel;
            HeaderHeight = headerHeight;
            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            CreatePool();

            AvailableItems.ForEach(x => x.Closed += (sender, args) => Remove(args.Channel));

            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            HandleKeyPressTab();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<ChatChannel> CreateObject(ChatChannel item, int index)
            => new DrawableChatChannel(ActiveChatChannel, this, item, index);

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Height = size.Y.Value - HeaderHeight;
            RecalculateContainerHeight();
        }

        /// <summary>
        ///     Adds a chat channel to the list
        /// </summary>
        /// <param name="chan"></param>
        public void Add(ChatChannel chan)
        {
            AvailableItems.Add(chan);
            AddObjectToBottom(chan, false);
            chan.Closed += (sender, args) => Remove(chan);
        }

        /// <summary>
        ///     Removes a chat channel from the list
        /// </summary>
        /// <param name="chan"></param>
        public void Remove(ChatChannel chan)
        {
            var item = Pool.Find(x => x.Item == chan);

            lock (AvailableItems)
                AvailableItems.Remove(chan);

            OnlineChat.JoinedChatChannels.RemoveAll(x => x.Name == chan.Name);

            // Switch to the next available channel
            if (ActiveChatChannel.Value == chan)
            {
                var index = AvailableItems.IndexOf(chan);

                if (index - 1 >= 0)
                    ActiveChatChannel.Value = AvailableItems[index - 1];
                else if (index + 1 < AvailableItems.Count - 1)
                    ActiveChatChannel.Value = AvailableItems[index + 1];
            }

            // Remove the item if it exists in the pool.
            if (item != null)
            {
                item.Destroy();
                RemoveContainedDrawable(item);
                Pool.Remove(item);
            }

            RecalculateContainerHeight();

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Index = i;
                Pool[i].ClearAnimations();
                Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
                Pool[i].UpdateContent(Pool[i].Item, i);
            }

            switch (AvailableItems.Count)
            {
                // No more chats are available after removing.
                case 0:
                    ActiveChatChannel.Value = null;
                    break;
                case 1:
                    ActiveChatChannel.Value = AvailableItems.First();
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="rco"></param>
        public void ActivateRightClickOptions(RightClickOptions rco)
        {
            if (ActiveRightClickOptions != null)
            {
                ActiveRightClickOptions.Visible = false;
                ActiveRightClickOptions.Parent = null;
                ActiveRightClickOptions.Destroy();
            }

            ActiveRightClickOptions = rco;
            ActiveRightClickOptions.Parent = this;

            ActiveRightClickOptions.ItemContainer.Height = 0;
            ActiveRightClickOptions.Visible = true;

            var x = MathHelper.Clamp(MouseManager.CurrentState.X - ActiveRightClickOptions.Width - AbsolutePosition.X, 0,
                Width - ActiveRightClickOptions.Width);

            var y = MathHelper.Clamp(MouseManager.CurrentState.Y - AbsolutePosition.Y, 0,
                Height - ActiveRightClickOptions.Items.Count * ActiveRightClickOptions.Items.First().Height);

            ActiveRightClickOptions.Position = new ScalableVector2(x, y);
            ActiveRightClickOptions.Open(350);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected)
            {
                UnsubscribeFromEvents();
                RemoveSpecialChannels();
                return;
            }

            SubscribeToEvents();
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            OnlineManager.Client.OnJoinedChatChannel += OnJoinedChatChannel;
            OnlineManager.Client.OnLeftChatChannel += OnLeftChatChannel;
            OnlineManager.Client.OnChatMessageReceived += OnChatMessageReceived;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            OnlineManager.Client.OnJoinedChatChannel -= OnJoinedChatChannel;
            OnlineManager.Client.OnLeftChatChannel -= OnLeftChatChannel;
            OnlineManager.Client.OnChatMessageReceived -= OnChatMessageReceived;
        }

        /// <summary>
        ///     Called when successfully joining a chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJoinedChatChannel(object sender, JoinedChatChannelEventArgs e)
        {
            // Try to find the channel in the available ones we have
            var chan = OnlineChat.AvailableChatChannels.Find(x => x.Name == e.Channel);

            // Channel could not be found, so create a new one
            if (chan == null)
            {
                chan = new ChatChannel()
                {
                    Name = e.Channel,
                    Description = "No Description",
                    AllowedUserGroups = UserGroups.Normal
                };
            }

            // If an existing channel already exists, use that one rather than creating an entirely new one.
            var existingChannel = OnlineChat.JoinedChatChannels.Find(x => x.Name == e.Channel);

            if (existingChannel != null)
                chan = existingChannel;
            else
            {
                Add(chan);
                ActiveChatChannel.Value = chan;
            }

            Logger.Important($"Joined chat channel: {chan.Name} | {chan.Description}", LogType.Runtime);
        }

        /// <summary>
        ///     Called when successfully leaving a chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeftChatChannel(object sender, LeftChatChannelEventArgs e)
        {
            var chan = OnlineChat.JoinedChatChannels.Find(x => x.Name == e.ChannelName);
            chan?.Close();

            if (chan != null)
                Logger.Important($"Left chat channel: {chan.Name} | {chan.Description}", LogType.Runtime);
        }

        /// <summary>
        ///     Called when receiving a chat message.
        ///     Used to create new private message channels if one doesn't exist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (e.Message.SenderId == OnlineManager.Self.OnlineUser.Id)
                return;

            // Private message
            if (!e.Message.Channel.StartsWith("#"))
            {
                ChatChannel chatChannel = null;

                // Try to find a channel with their name
                var channel = Pool.Find(x => x.Item.Name == e.Message.SenderName);

                // No channel with their name was found, so create one
                if (channel == null)
                {
                    chatChannel = new ChatChannel()
                    {
                        Name = e.Message.SenderName,
                        Description = "Private Chat",
                        IsUnread = true
                    };

                    Add(chatChannel);
                    OnlineChat.Instance.MessageContainer.AddChannel(chatChannel);
                }
                else
                {
                    chatChannel = channel.Item;

                    if (ActiveChatChannel.Value != channel.Item)
                    {
                        channel.Item.IsUnread = true;
                        channel.UpdateContent(channel.Item, channel.Index);
                    }
                }

                // Send a message with the active channel
                if (!OnlineChat.Instance.IsOpen)
                {
                    NotificationManager.Show(NotificationLevel.Info, $"{e.Message.SenderName} has sent you a message. " +
                                                                     $"Click here to read it!", (o, args) =>
                    {
                        DialogManager.Show(new OnlineHubDialog());

                        var joinedChannel = OnlineChat.JoinedChatChannels.Find(x => x.Name == chatChannel.Name);

                        if (joinedChannel != null)
                            ActiveChatChannel.Value = chatChannel;
                    });
                }

                return;
            }

            // Try to find the public channel
            var publicChannel = Pool.Find(x => x.Item.Name == e.Message.Channel);

            if (publicChannel == null)
                return;

            // Mark the channel as unread
            if (ActiveChatChannel.Value != publicChannel.Item)
            {
                publicChannel.Item.IsUnread = true;
                publicChannel.UpdateContent(publicChannel.Item, publicChannel.Index);
            }
        }

        /// <summary>
        ///     Removes 'special' channels from the pool.
        ///     Examples:
        ///         - #multiplayer_<game_hash>
        ///         - #spectator_<user_id>
        ///         - #multi_team_<game_hash>
        /// </summary>
        private void RemoveSpecialChannels()
        {
            var special = Pool.FindAll(x => x.Item.Name.StartsWith("#multi") || x.Item.Name.StartsWith("#spectator"));
            special.ForEach(x => Remove(x.Item));
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressTab()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Tab))
                return;

            if (AvailableItems.Count == 0)
                return;

            var index = AvailableItems.IndexOf(ActiveChatChannel.Value);

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift))
            {
                ActiveChatChannel.Value = index - 1 >= 0 ? AvailableItems[index - 1] : AvailableItems[AvailableItems.Count - 1];
                return;
            }

            ActiveChatChannel.Value = index + 1 < AvailableItems.Count ? AvailableItems[index + 1] : AvailableItems.First();
        }
    }
}