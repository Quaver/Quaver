using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.V2.UI;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatMessageList : ScrollContainer
    {
        private const int MaxMessagesFlushedPerUpdate = 8;

        private const int MaxRetainedMessages = 250;

        private LoadingWheel LoadingWheel { get; }

        private Bindable<ChatChannel> ActiveChannel { get; }

        private ChatMessageStore MessageStore { get; set; }

        private List<ChatMessage> Messages { get; } = new List<ChatMessage>();

        private List<ChatMessageRow> Rows { get; } = new List<ChatMessageRow>();

        private List<ChatMessage> MessageHistoryQueue { get; } = new List<ChatMessage>();

        private List<ChatMessage> MessageQueue { get; } = new List<ChatMessage>();

        private bool HasQueuedInitialHistory { get; set; }

        private bool NeedsReflow { get; set; }

        private bool NeedsScrollToBottom { get; set; }

        private bool ForceScrollToBottom { get; set; }

        private long AppliedStoreGeneration { get; set; } = -1;

        private long AppliedStoreVersion { get; set; } = -1;

        private float PreviousWidth { get; set; } = -1;

        private float PreviousHeight { get; set; } = -1;

        public event Action<User> SenderMenuRequested;

        public ChatMessageList(Bindable<ChatChannel> activeChannel, ScalableVector2 size) : base(size, size)
        {
            ActiveChannel = activeChannel;

            ScrollSpeed = 220;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            AllowScrollbarDragging = true;
            CapturesMouseWheelInput = true;

            V2Scrollbar.Apply(this);

            LoadingWheel = new LoadingWheel
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(50, 50),
                Visible = false
            };

            ActiveChannel.ValueChanged += OnActiveChannelChanged;
            ConfigManager.ChatCensorEnabled.ValueChanged += OnChatCensorToggled;
            ChangeChannel(ActiveChannel.Value);
        }

        public override void Update(GameTime gameTime)
        {
            var widthChanged = Math.Abs(PreviousWidth - Width) > 0.001f;
            var heightChanged = Math.Abs(PreviousHeight - Height) > 0.001f;
            if (widthChanged || heightChanged)
            {
                PreviousWidth = Width;
                PreviousHeight = Height;

                if (widthChanged)
                    ReflowRows(true);
                else
                    UpdateContentHeight();
            }

            SynchronizeMessageStore();
            FlushMessageHistoryQueue();
            FlushMessageQueue();

            var isLoadingHistory = MessageStore != null && IsLoadingMessageHistory();
            LoadingWheel.Visible = isLoadingHistory;
            ContentViewport.Visible = !isLoadingHistory;
            Scrollbar.Visible = !isLoadingHistory;
            InputEnabled = !isLoadingHistory && IsHovered();

            if (NeedsReflow)
            {
                ReflowRows(false);

                if (NeedsScrollToBottom)
                    ScrollToBottom(ForceScrollToBottom ? 0 : 150);

                NeedsScrollToBottom = false;
                ForceScrollToBottom = false;
            }

            base.Update(gameTime);

            if (!isLoadingHistory)
                UpdateVisibleRows(this, Rows);
        }

        private static void UpdateVisibleRows(ScrollContainer scroll, IEnumerable<Drawable> rows)
        {
            var visibleTop = -scroll.ContentContainer.Y - 40;
            var visibleBottom = -scroll.ContentContainer.Y+ scroll.Height + 40;

            foreach (var row in rows)
            {
                row.Visible = row.Y + row.Height >= visibleTop && row.Y <= visibleBottom;
            }
        }

        private void ChangeChannel(ChatChannel channel)
        {
            MessageStore = channel == null ? null : ChatMessageStore.GetOrCreate(channel);
            LoadingWheel.Visible = false;
            AppliedStoreGeneration = -1;
            AppliedStoreVersion = -1;
            ClearDisplayedMessages();

            ContentContainer.Animations.Clear();
            ContentContainer.Height = Height;
            ContentContainer.Y = 0;
            TargetY = 0;
            PreviousTargetY = 0;
        }

        private void SynchronizeMessageStore()
        {
            if (MessageStore == null)
                return;

            // Snapshots makes sure that we handle an already network-managed version of the chat rows
            // This helps handling skipped or duplicated messages (*flashbacks to v1 row stacking*)
            var snapshot = MessageStore.GetSnapshot();

            if (snapshot.Generation != AppliedStoreGeneration)
            {
                ClearDisplayedMessages();
                AppliedStoreGeneration = snapshot.Generation;
                AppliedStoreVersion = -1;
            }

            if (!snapshot.HasLoadedMessageHistory || snapshot.Version == AppliedStoreVersion)
                return;

            var queue = HasQueuedInitialHistory ? MessageQueue : MessageHistoryQueue;
            foreach (var message in snapshot.Messages)
            {
                if (!ContainsMessage(Messages, message) && !ContainsMessage(MessageHistoryQueue, message) && !ContainsMessage(MessageQueue, message))
                    queue.Add(message);
            }

            HasQueuedInitialHistory = true;
            AppliedStoreVersion = snapshot.Version;
        }

        private void FlushMessageHistoryQueue()
        {
            if (!HasQueuedInitialHistory || MessageHistoryQueue.Count == 0)
                return;

            var count = Math.Min(MessageHistoryQueue.Count, MaxMessagesFlushedPerUpdate);
            for (var i = 0; i < count; i++)
                AddRow(MessageHistoryQueue[i]);

            
            MessageHistoryQueue.RemoveRange(0, count);
            if (MessageHistoryQueue.Count != 0)
                return;

            NeedsScrollToBottom = true;
            ForceScrollToBottom = true;
        }

        private void FlushMessageQueue()
        {
            if (!HasQueuedInitialHistory || MessageQueue.Count == 0 || HasPendingHistoryMessages())
                return;

            var count = Math.Min(MessageQueue.Count, MaxMessagesFlushedPerUpdate);
            for (var i = 0; i < count; i++)
                AddRow(MessageQueue[i]);


            MessageQueue.RemoveRange(0, count);
        }

        private bool HasPendingHistoryMessages() => MessageHistoryQueue.Count != 0;

        private bool IsLoadingMessageHistory() => !HasQueuedInitialHistory || HasPendingHistoryMessages();

        private void ReflowRows(bool updateContent)
        {
            var y = 0f;
            var rowWidth = GetRowWidth();

            foreach (var row in Rows)
            {
                if (updateContent)
                    row.UpdateContent(row.Item, rowWidth);

                row.Y = y;
                y += row.Height;
            }

            ContentContainer.Height = Math.Max(Height, y);
            NeedsReflow = false;
        }

        private void UpdateContentHeight()
        {
            var rowsHeight = Rows.Count == 0 ? 0 : Rows[^1].Y + Rows[^1].Height;
            ContentContainer.Height = Math.Max(Height, rowsHeight);
        }

        private void AddRow(ChatMessage message)
        {
            if (ContainsMessage(Messages, message))
                return;

            var shouldRemainAtBottom = IsNearBottom() || message.IsFromSelf;
            var row = new ChatMessageRow(message, GetRowWidth(), this);
            row.UpdateWhenInvisible = false;
            row.SenderMenuRequested += OnSenderMenuRequested;

            AddContainedDrawable(row);

            Messages.Add(message);
            Rows.Add(row);
            TrimRetainedMessages();

            NeedsReflow = true;
            if (shouldRemainAtBottom)
                NeedsScrollToBottom = true;
        }

        private void TrimRetainedMessages()
        {
            var excess = Messages.Count - MaxRetainedMessages;
            for (var i = 0; i < excess; i++)
            {
                var row = Rows[0];
                row.SenderMenuRequested -= OnSenderMenuRequested;
                row.Destroy();
                Rows.RemoveAt(0);
            }

            if (excess > 0)
                Messages.RemoveRange(0, excess);
        }

        private void ClearDisplayedMessages()
        {
            foreach (var row in Rows)
            {
                row.SenderMenuRequested -= OnSenderMenuRequested;
                row.Destroy();
            }

            Rows.Clear();
            Messages.Clear();
            MessageHistoryQueue.Clear();
            MessageQueue.Clear();
            HasQueuedInitialHistory = false;
            NeedsReflow = false;
            NeedsScrollToBottom = false;
            ForceScrollToBottom = false;
            UpdateContentHeight();
        }

        private float GetRowWidth() => V2Scrollbar.GetContentWidth(this);

        private bool IsNearBottom() => ContentContainer.Height <= Height || ContentContainer.Height - Height + ContentContainer.Y <= 40;

        public void ScrollToBottom(int duration)
        {
            var y = Math.Min(0, Height - ContentContainer.Height);

            if (duration > 0)
            {
                ScrollTo(y, duration);
                return;
            }

            ContentContainer.Animations.Clear();
            ContentContainer.Y = y;
            TargetY = y;
            PreviousTargetY = y;
        }

        private static bool MessagesMatch(ChatMessage first, ChatMessage second) =>
            ReferenceEquals(first, second) || first.SenderId == second.SenderId &&
            first.Channel == second.Channel && first.Message == second.Message && first.Time == second.Time;

        private static bool ContainsMessage(List<ChatMessage> messages, ChatMessage message)
        {
            foreach (var existing in messages)
            {
                if (MessagesMatch(existing, message))
                    return true;
            }

            return false;
        }

        private void OnSenderMenuRequested(User user) => SenderMenuRequested?.Invoke(user);

        private void OnActiveChannelChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e) => ChangeChannel(e.Value);
        private void OnChatCensorToggled(object sender, BindableValueChangedEventArgs<bool> e) => ReflowRows(true);

        public override void Destroy()
        {
            ActiveChannel.ValueChanged -= OnActiveChannelChanged;
            ConfigManager.ChatCensorEnabled.ValueChanged -= OnChatCensorToggled;
            ClearDisplayedMessages();
            SenderMenuRequested = null;
            base.Destroy();
        }
    }
}
