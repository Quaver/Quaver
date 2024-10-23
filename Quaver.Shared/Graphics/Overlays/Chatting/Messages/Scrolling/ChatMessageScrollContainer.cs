using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client;
using Quaver.Server.Client.Events;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.BlockedUsers;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Scrolling
{
    public class ChatMessageScrollContainer : PoolableScrollContainer<ChatMessage>, IResizable
    {
        /// <summary>
        ///     The channel the scroll container is for
        /// </summary>
        public ChatChannel Channel { get; }

        /// <summary>
        /// </summary>
        private float HeaderHeight { get; }

        /// <summary>
        /// </summary>
        private float TextboxHeight { get; }

        /// <summary>
        ///     If the message history for this channel has already been requested
        /// </summary>
        private bool HasRequestedMessageHistory { get; set; }

        /// <summary>
        /// </summary>
        private TaskHandler<int, int> RequestHistoryTask { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel LoadingIcon { get; set; }

        /// <summary>
        ///     Chat messages that are apart of the message history queue
        /// </summary>
        private List<ChatMessage> MessageHistoryQueue { get; } = new List<ChatMessage>();

        /// <summary>
        ///     The queue for incoming messages from the server
        /// </summary>
        private List<ChatMessage> MessageQueue { get; } = new List<ChatMessage>();

        /// <summary>
        ///     The total height of all messages
        /// </summary>
        private float TotalMessageHeight { get; set; }

        /// <summary>
        ///     The currently active right click options for the screen
        /// </summary>
        public RightClickOptions ActiveRightClickOptions { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="size"></param>
        /// <param name="headerHeight"></param>
        /// <param name="textboxHeight"></param>
        public ChatMessageScrollContainer(ChatChannel channel, ScalableVector2 size, float headerHeight, float textboxHeight)
            : base(new List<ChatMessage>(), int.MaxValue, 0, size, size)
        {
            Channel = channel;
            HeaderHeight = headerHeight;
            TextboxHeight = textboxHeight;

            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            RequestHistoryTask = new TaskHandler<int, int>(RunRequestHistoryTask);
            Channel.MessageQueued += OnMessageQueued;
            Channel.Closed += OnChannelClosed;

            CreateLoadingWheel();
            Pool = new List<PoolableSprite<ChatMessage>>();

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnConnectionStatusChanged += OnConnectionStatusChanged;

                if (OnlineManager.Connected)
                    OnlineManager.Client.OnChatMessageReceived += OnChatMessageReceived;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt)
                           && !OnlineChat.Instance.IsJoinChannelDialogOpen();

            LoadingIcon.Visible = RequestHistoryTask.IsRunning || !OnlineManager.Connected;

            RequestMessageHistoryIfNecessary();
            FlushMessageHistoryQueue();
            FlushMessageQueue();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            UnsubscribeFromEvents();
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<ChatMessage> CreateObject(ChatMessage item, int index)
            => new DrawableChatMessage(this, item, index);

        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public void ChangeSize(ScalableVector2 size)
        {
            Height = size.Y.Value - HeaderHeight - TextboxHeight;
            UpdateContentContainerSize();
        }

        /// <summary>
        ///     Requests the channel's message history.
        /// </summary>
        private void RequestMessageHistoryIfNecessary()
        {
            if (HasRequestedMessageHistory || !OnlineManager.Connected)
                return;

            Logger.Important($"Fetching message history for channel: {Channel.Name}", LogType.Runtime);

            HasRequestedMessageHistory = true;
            RequestHistoryTask.Run(0);
        }

        /// <summary>
        /// </summary>
        /// <param name="val"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private int RunRequestHistoryTask(int val, CancellationToken token)
        {
            // While visual testing, the online client can be overwritten, so just create a new client.
            var client = OnlineManager.Client ?? new OnlineClient();
            var history = client?.GetChannelMessageHistory(Channel);

            // Add messages to the chat history queue, so they can wait to be processed.
            if (history != null)
            {
                lock (MessageHistoryQueue)
                {
                    foreach (var message in history.Messages)
                    {
                        if (BlockedUsers.IsUserBlocked(message.User.Id))
                            continue;

                        MessageHistoryQueue.Add(message.ToChatMessage(message.User.ToUser()));
                    }
                }
            }

            Logger.Important($"Finished Fetching message history for channel: {Channel.Name} w/ {history?.Messages.Count} messages!",
                LogType.Runtime);

            return 0;
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() =>LoadingIcon = new LoadingWheel
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(44, 44),
            Visible = false
        };

        /// <summary>
        ///     Takes all the messages in the message history queue and displays them
        /// </summary>
        private void FlushMessageHistoryQueue()
        {
            lock (MessageHistoryQueue)
            {
                if (!HasRequestedMessageHistory || MessageHistoryQueue.Count == 0)
                    return;

                foreach (var message in MessageHistoryQueue)
                    AddMessage(message);

                Logger.Important($"Flushed {MessageHistoryQueue.Count} messages from the `{Channel.Name}` message history queue.", LogType.Runtime);
                MessageHistoryQueue.Clear();
            }

            ScrollToBottomIfNecessary(true);
        }

        /// <summary>
        ///     Takes all the messages in the queue and adds them to the container
        /// </summary>
        private void FlushMessageQueue()
        {
            lock (MessageQueue)
            {
                if (!HasRequestedMessageHistory || MessageQueue.Count == 0 || RequestHistoryTask.IsRunning)
                    return;

                foreach (var message in MessageQueue)
                    AddMessage(message);

                Logger.Important($"Flushed {MessageQueue.Count} messages from the `{Channel.Name}` message queue.", LogType.Runtime);
                MessageQueue.Clear();
            }
        }

        /// <summary>
        ///     Adds a message at the bottom of the list
        /// </summary>
        /// <param name="message"></param>
        private void AddMessage(ChatMessage message)
        {
            lock (AvailableItems)
            lock (Pool)
            {
                if (!Channel.Messages.Contains(message))
                    Channel.Messages.Add(message);

                if (!AvailableItems.Contains(message))
                    AvailableItems.Add(message);

                var drawable = AddObject(AvailableItems.Count - 1);

                if (Pool.Count < PoolSize)
                    AddContainedDrawable(drawable);

                drawable.Y = TotalMessageHeight;
                TotalMessageHeight += drawable.Height;

                UpdateContentContainerSize();
                ScrollToBottomIfNecessary();
            }
        }

        /// <summary>
        /// </summary>
        public void UpdateContentContainerSize() => ContentContainer.Height = Math.Max(Height, TotalMessageHeight);

        /// <summary>
        /// </summary>
        private void ScrollToBottomIfNecessary(bool force = false)
        {
            if (TotalMessageHeight - Height - Math.Abs(ContentContainer.Y) >= 600 && !AvailableItems.Last().IsFromSelf && !force)
                return;

            ClearAnimations();
            ScrollTo(-TotalMessageHeight, 600);
        }

        /// <summary>
        ///     Called when a message has been queued up to be sent to the channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageQueued(object sender, MessageQueuedEventArgs e)
        {
            lock (MessageQueue)
            {
                if (MessageQueue.Contains(e.Message))
                    return;

                MessageQueue.Add(e.Message);
            }
        }

        /// <summary>
        ///     When the channel closes, dispose of everything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChannelClosed(object sender, Server.Client.Structures.ChannelClosedEventArgs e)
        {
            Destroy();
            RecalculateContainerHeight();

            OnlineChat.JoinedChatChannels.RemoveAll(x => x.Name == e.Channel.Name);
            OnlineChat.Instance.MessageContainer.MessageScrollContainers.Remove(Channel);
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
        private void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            // Clear the message queue and display the loading wheel inherently
            AddScheduledUpdate(() =>
            {
                AvailableItems.Clear();
                Pool.ForEach(x => x.Destroy());
                Pool.Clear();
                TotalMessageHeight = 0;
                UpdateContentContainerSize();
            });

            if (e.Status != ConnectionStatus.Connected)
            {
                UnsubscribeFromEvents();
                return;
            }

            // Set this to false, because it will automatically request the message history again
            HasRequestedMessageHistory = false;

            SubscribeToEvents();
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            OnlineManager.Client.OnChatMessageReceived += OnChatMessageReceived;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnChatMessageReceived -= OnChatMessageReceived;
        }

        /// <summary>
        ///     Called when receiving a chat message from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (!HasRequestedMessageHistory)
                return;

            if (BlockedUsers.IsUserBlocked(e.Message.SenderId))
                return;

            // Public Chats
            if (Channel.Name.StartsWith("#") && Channel.Name != e.Message.Channel)
                return;

            // Private Chats (the channel name is the sender's name)
            if (!Channel.Name.StartsWith("#"))
            {
                if (Channel.Name != e.Message.SenderName)
                    return;

                if (e.Message.Channel != OnlineManager.Self.OnlineUser.Username)
                    return;
            }

            // Don't handle messages from offline users
            if (!OnlineManager.OnlineUsers.ContainsKey(e.Message.SenderId))
                return;

            e.Message.Sender = OnlineManager.OnlineUsers[e.Message.SenderId];

            // Don't display messages from self.
            if (e.Message.Sender == OnlineManager.Self)
                return;

            Channel.QueueMessage(e.Message);
        }
    }
}