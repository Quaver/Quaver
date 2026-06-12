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
        ///     The maximum amount of messages to create in a single update.
        /// </summary>
        private const int MaxMessagesFlushedPerUpdate = 8;

        /// <summary>
        ///     Extra pixels above and below the viewport to keep message drawables active while scrolling.
        /// </summary>
        private const int VisibleMessageBuffer = 300;

        /// <summary>
        ///     The total height of all messages
        /// </summary>
        private float TotalMessageHeight { get; set; }

        /// <summary>
        ///     If message drawables need to have their positions recalculated.
        /// </summary>
        private bool NeedsReflow { get; set; }

        /// <summary>
        ///     If the container should scroll to the bottom after the next reflow.
        /// </summary>
        private bool NeedsScrollToBottom { get; set; }

        /// <summary>
        ///     If the next bottom scroll should ignore the user's current scroll position.
        /// </summary>
        private bool ForceScrollToBottom { get; set; }

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

            if (NeedsReflow)
            {
                ReflowMessageDrawables();

                if (NeedsScrollToBottom)
                    ScrollToBottomIfNecessary(ForceScrollToBottom);

                NeedsScrollToBottom = false;
                ForceScrollToBottom = false;
            }

            UpdateVisibleMessageDrawables();

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

                var count = Math.Min(MessageHistoryQueue.Count, MaxMessagesFlushedPerUpdate);

                for (var i = 0; i < count; i++)
                    AddMessage(MessageHistoryQueue[i]);

                MessageHistoryQueue.RemoveRange(0, count);

                Logger.Important($"Flushed {count} messages from the `{Channel.Name}` message history queue. {MessageHistoryQueue.Count} messages remaining.", LogType.Runtime);
            }

            lock (MessageHistoryQueue)
            {
                if (MessageHistoryQueue.Count == 0)
                {
                    NeedsScrollToBottom = true;
                    ForceScrollToBottom = true;
                }
            }
        }

        /// <summary>
        ///     Takes all the messages in the queue and adds them to the container
        /// </summary>
        private void FlushMessageQueue()
        {
            lock (MessageQueue)
            {
                if (!HasRequestedMessageHistory || MessageQueue.Count == 0 || RequestHistoryTask.IsRunning || HasPendingHistoryMessages())
                    return;

                var count = Math.Min(MessageQueue.Count, MaxMessagesFlushedPerUpdate);

                for (var i = 0; i < count; i++)
                    AddMessage(MessageQueue[i]);

                MessageQueue.RemoveRange(0, count);

                Logger.Important($"Flushed {count} messages from the `{Channel.Name}` message queue. {MessageQueue.Count} messages remaining", LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private bool HasPendingHistoryMessages()
        {
            lock (MessageHistoryQueue)
                return MessageHistoryQueue.Count != 0;
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
                if (ContainsMessage(AvailableItems, message))
                    return;

                if (!ContainsMessage(Channel.Messages, message))
                    Channel.Messages.Add(message);

                AvailableItems.Add(message);

                var drawable = AddObject(AvailableItems.Count - 1);

                if (ShouldScrollToBottom(message))
                    NeedsScrollToBottom = true;

                NeedsReflow = true;
            }
        }

        /// <summary>
        ///     Positions each message after the previous message's actual rendered height.
        ///     This keeps multiline or late-resized messages from overlapping rows below them.
        /// </summary>
        private void ReflowMessageDrawables()
        {
            lock (Pool)
            {
                var y = 0f;

                foreach (var drawable in Pool)
                {
                    drawable.Y = y;
                    y += drawable.Height;
                }

                TotalMessageHeight = y;
                NeedsReflow = false;
                UpdateContentContainerSize();
            }
        }

        /// <summary>
        ///     Keeps only visible or near-visible messages parented to the content container.
        ///     Detached messages keep their layout data, but they do not update or draw while offscreen.
        /// </summary>
        private void UpdateVisibleMessageDrawables()
        {
            lock (Pool)
            {
                var visibleTop = -ContentContainer.Y - VisibleMessageBuffer;
                var visibleBottom = -ContentContainer.Y + Height + VisibleMessageBuffer;

                foreach (var drawable in Pool)
                {
                    var isVisible = drawable.Y + drawable.Height >= visibleTop && drawable.Y <= visibleBottom;

                    if (isVisible && drawable.Parent != ContentContainer)
                        AddContainedDrawable(drawable);
                    else if (!isVisible && drawable.Parent == ContentContainer)
                        RemoveContainedDrawable(drawable);
                }
            }
        }

        /// <summary>
        ///     Checks whether a message has already been added. Messages can be re-created as separate
        ///     objects when coming from history or queue paths, so compare stable message fields too.
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool ContainsMessage(List<ChatMessage> messages, ChatMessage message)
        {
            return messages.Any(x => ReferenceEquals(x, message)
                                     || x.SenderId == message.SenderId
                                     && x.Channel == message.Channel
                                     && x.Message == message.Message
                                     && x.Time == message.Time);
        }

        /// <summary>
        /// </summary>
        public void UpdateContentContainerSize() => ContentContainer.Height = Math.Max(Height, TotalMessageHeight);

        /// <summary>
        /// </summary>
        private void ScrollToBottomIfNecessary(bool force = false)
        {
            if (AvailableItems.Count == 0)
                return;

            if (!ShouldScrollToBottom(AvailableItems.Last(), force))
                return;

            ClearAnimations();
            ScrollTo(-TotalMessageHeight, 600);
        }

        /// <summary>
        /// </summary>
        /// <param name="latestMessage"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        private bool ShouldScrollToBottom(ChatMessage latestMessage, bool force = false)
        {
            return force || TotalMessageHeight - Height - Math.Abs(ContentContainer.Y) < 600 || latestMessage.IsFromSelf;
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
                DismissActiveRightClickOptions();
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
        public void DismissActiveRightClickOptions()
        {
            if (ActiveRightClickOptions == null)
                return;

            ActiveRightClickOptions.Visible = false;
            ActiveRightClickOptions.Parent = null;
            ActiveRightClickOptions.Destroy();
            ActiveRightClickOptions = null;
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
                NeedsReflow = false;
                NeedsScrollToBottom = false;
                ForceScrollToBottom = false;
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
            if (OnlineManager.Self != null && e.Message.SenderId == OnlineManager.Self.OnlineUser.Id)
                return;

            Channel.QueueMessage(e.Message);
        }
    }
}
