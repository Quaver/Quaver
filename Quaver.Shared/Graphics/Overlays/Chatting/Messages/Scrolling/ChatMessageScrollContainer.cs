using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

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
        ///     The persistent message source shared by every view of this channel.
        /// </summary>
        private ChatMessageStore MessageStore { get; }

        /// <summary>
        ///     If this renderer belongs to the F8 chat overlay.
        /// </summary>
        private bool IsOverlayView { get; }

        /// <summary>
        ///     If the store's initial history has been queued for display.
        /// </summary>
        private bool HasQueuedInitialHistory { get; set; }

        /// <summary>
        ///     If messages received while F8 was closed should be staged before rendering again.
        /// </summary>
        private bool ShouldStageStoreCatchUp { get; set; }

        /// <summary>
        ///     The last message-store generation rendered by this view.
        /// </summary>
        private long AppliedStoreGeneration { get; set; } = -1;

        /// <summary>
        ///     The last message-store version rendered by this view.
        /// </summary>
        private long AppliedStoreVersion { get; set; } = -1;

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
        ///     The maximum number of messages retained for a channel.
        /// </summary>
        private const int MaxRetainedMessages = 250;

        /// <summary>
        ///     The interval between refreshes of visible relative timestamps.
        /// </summary>
        private const int MessageTimestampRefreshInterval = 10_000;

        /// <summary>
        ///     The elapsed time since visible timestamps were last refreshed.
        /// </summary>
        private double ElapsedTimeSinceTimestampRefresh { get; set; }

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

        /// <summary>
        ///     Last visibility state applied by the chat overlay.
        /// </summary>
        private bool? AppliedVisibility { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="size"></param>
        /// <param name="headerHeight"></param>
        /// <param name="textboxHeight"></param>
        public ChatMessageScrollContainer(ChatChannel channel, ScalableVector2 size, float headerHeight, float textboxHeight, bool isOverlayView = false) : base(new List<ChatMessage>(), int.MaxValue, 0, size, size)
        {
            Channel = channel;
            HeaderHeight = headerHeight;
            TextboxHeight = textboxHeight;
            IsOverlayView = isOverlayView;
            MessageStore = ChatMessageStore.GetOrCreate(channel);

            Alpha = 0;

            Scrollbar.Width = 4;
            Scrollbar.Tint = Color.White;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 220;

            Channel.Closed += OnChannelClosed;

            CreateLoadingWheel();
            Pool = new List<PoolableSprite<ChatMessage>>();

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsOverlayView && OnlineChat.Instance != null && !OnlineChat.Instance.IsOpen)
                return;

            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt)
                           && !OnlineChat.Instance.IsJoinChannelDialogOpen();

            SynchronizeMessageStore();
            FlushMessageHistoryQueue();
            FlushMessageQueue();

            var isLoadingHistory = IsLoadingMessageHistory();
            LoadingIcon.Visible = isLoadingHistory || !OnlineManager.Connected;

            if (!isLoadingHistory && NeedsReflow)
            {
                ReflowMessageDrawables();

                if (NeedsScrollToBottom)
                    ScrollToBottomIfNecessary(ForceScrollToBottom);

                NeedsScrollToBottom = false;
                ForceScrollToBottom = false;
            }

            base.Update(gameTime);

            if (!isLoadingHistory)
            {
                UpdateVisibleMessageDrawables();
                RefreshVisibleMessageTimestamps(gameTime);
            }
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Channel.Closed -= OnChannelClosed;
            base.Destroy();
        }

        /// <summary>
        ///     Applies the chat overlay visibility state while preserving this container's viewport culling.
        /// </summary>
        /// <param name="visible"></param>
        public void ApplyVisibility(bool visible)
        {
            if (AppliedVisibility == visible && !visible)
                return;

            AppliedVisibility = visible;
            SetDrawableTreeVisible(this, visible);

            if (Pool == null)
                return;

            if (!visible)
            {
                foreach (var drawable in Pool.OfType<DrawableChatMessage>())
                    drawable.SetScrollVisibility(false);

                return;
            }

            if (IsLoadingMessageHistory())
                return;

            UpdateVisibleMessageDrawables();
        }

        /// <summary>
        ///     Stages messages received while F8 is closed without discarding this tab's cached drawables.
        /// </summary>
        public void StageStoreCatchUpForOverlayClose()
        {
            if (IsOverlayView)
                ShouldStageStoreCatchUp = true;
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
        /// </summary>
        private void CreateLoadingWheel() =>LoadingIcon = new LoadingWheel
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(44, 44),
            Visible = false
        };

        /// <summary>
        ///     Brings this renderer in line with the shared, non-visual message store.
        /// </summary>
        private void SynchronizeMessageStore()
        {
            var snapshot = MessageStore.GetSnapshot();

            if (snapshot.Generation != AppliedStoreGeneration)
            {
                ClearDisplayedMessages();
                HasQueuedInitialHistory = false;
                AppliedStoreGeneration = snapshot.Generation;
                AppliedStoreVersion = -1;
            }

            if (!snapshot.HasLoadedMessageHistory)
                return;

            if (snapshot.Version == AppliedStoreVersion)
            {
                ShouldStageStoreCatchUp = false;
                return;
            }

            lock (AvailableItems)
            lock (MessageHistoryQueue)
            lock (MessageQueue)
            {
                var queue = !HasQueuedInitialHistory || ShouldStageStoreCatchUp ? MessageHistoryQueue : MessageQueue;

                foreach (var message in snapshot.Messages)
                {
                    if (!ContainsMessage(AvailableItems, message) && !ContainsMessage(MessageHistoryQueue, message)
                        && !ContainsMessage(MessageQueue, message))
                    {
                        queue.Add(message);
                    }
                }

                HasQueuedInitialHistory = true;
                AppliedStoreVersion = snapshot.Version;
                ShouldStageStoreCatchUp = false;
            }
        }

        /// <summary>
        ///     Removes stale drawables after the shared store resets on reconnect.
        /// </summary>
        private void ClearDisplayedMessages()
        {
            lock (AvailableItems)
            lock (Pool)
            lock (MessageHistoryQueue)
            lock (MessageQueue)
            {
                foreach (var drawable in Pool)
                    drawable.Destroy();

                AvailableItems.Clear();
                Pool.Clear();
                MessageHistoryQueue.Clear();
                MessageQueue.Clear();
                TotalMessageHeight = 0;
                NeedsReflow = false;
                NeedsScrollToBottom = false;
                ForceScrollToBottom = false;
                UpdateContentContainerSize();
            }
        }

        /// <summary>
        ///     Takes all the messages in the message history queue and displays them
        /// </summary>
        private void FlushMessageHistoryQueue()
        {
            lock (MessageHistoryQueue)
            {
                if (!HasQueuedInitialHistory || MessageHistoryQueue.Count == 0)
                    return;

                var count = Math.Min(MessageHistoryQueue.Count, MaxMessagesFlushedPerUpdate);

                for (var i = 0; i < count; i++)
                    AddMessage(MessageHistoryQueue[i]);

                MessageHistoryQueue.RemoveRange(0, count);
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
                if (!HasQueuedInitialHistory || MessageQueue.Count == 0 || HasPendingHistoryMessages())
                    return;

                var count = Math.Min(MessageQueue.Count, MaxMessagesFlushedPerUpdate);

                for (var i = 0; i < count; i++)
                    AddMessage(MessageQueue[i]);

                MessageQueue.RemoveRange(0, count);
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
        ///     Indicates if messages are still being fetched or handled offscreen.
        /// </summary>
        /// <returns></returns>
        private bool IsLoadingMessageHistory()
            => !HasQueuedInitialHistory || HasPendingHistoryMessages();

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

                AvailableItems.Add(message);

                var drawable = AddObject(AvailableItems.Count - 1);

                TrimRetainedMessages();

                if (ShouldScrollToBottom(message))
                    NeedsScrollToBottom = true;

                NeedsReflow = true;
            }
        }

        /// <summary>
        ///     Removes the oldest message drawables once the retention limit is reached.
        ///     This method is called while <see cref="AvailableItems"/> and <see cref="Pool"/> are locked.
        /// </summary>
        private void TrimRetainedMessages()
        {
            var excess = AvailableItems.Count - MaxRetainedMessages;

            if (excess <= 0)
                return;

            for (var i = 0; i < excess; i++)
            {
                var drawable = Pool[0];

                if (drawable.Parent == ContentContainer)
                    RemoveContainedDrawable(drawable);

                drawable.Destroy();
                Pool.RemoveAt(0);
            }

            AvailableItems.RemoveRange(0, excess);
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

                    if (drawable is DrawableChatMessage message)
                        message.SetScrollVisibility(isVisible);
                }
            }
        }

        /// <summary>
        ///     Refreshes relative timestamps while the chat container is active.
        /// </summary>
        /// <param name="gameTime"></param>
        private void RefreshVisibleMessageTimestamps(GameTime gameTime)
        {
            ElapsedTimeSinceTimestampRefresh += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (ElapsedTimeSinceTimestampRefresh < MessageTimestampRefreshInterval)
                return;

            ElapsedTimeSinceTimestampRefresh = 0;
            var needsReflow = false;

            lock (Pool)
            {
                foreach (var message in Pool.OfType<DrawableChatMessage>())
                {
                    if (message.Parent == ContentContainer)
                        needsReflow |= message.RefreshTimestamp();
                }
            }

            if (needsReflow)
                NeedsReflow = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="visible"></param>
        private static void SetDrawableTreeVisible(Drawable drawable, bool visible)
        {
            drawable.Visible = visible;

            foreach (var child in drawable.Children)
                SetDrawableTreeVisible(child, visible);

            if (drawable is Dropdown dropdown)
                dropdown.ApplyItemVisibilityState();
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

            if (force)
            {
                var y = MathHelper.Clamp(-TotalMessageHeight, -ContentContainer.Height + Height, 0);

                ContentContainer.Animations.Clear();
                TargetY = y;
                PreviousTargetY = y;
                PreviousContentContainerY = y;
                ContentContainer.Y = y;
                return;
            }

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
        ///     When the channel closes, dispose of everything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChannelClosed(object sender, Server.Client.Structures.ChannelClosedEventArgs e)
        {
            Destroy();
            RecalculateContainerHeight();

            OnlineChat.JoinedChatChannels.RemoveAll(x => x.Name == e.Channel.Name);
            OnlineChat.Instance?.MessageContainer.MessageScrollContainers.Remove(Channel);
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

            ActiveRightClickOptions.Close(0);
            ActiveRightClickOptions.Visible = false;
            ActiveRightClickOptions.Parent = null;
            ActiveRightClickOptions.Destroy();
            ActiveRightClickOptions = null;
        }

    }
}
