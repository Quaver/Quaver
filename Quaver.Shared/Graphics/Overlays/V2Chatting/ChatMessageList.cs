using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatMessageList : ScrollContainer
    {
        private Bindable<ChatChannel> ActiveChannel { get; }

        private ChatMessageStore MessageStore { get; set; }

        private List<ChatMessage> Messages { get; } = new List<ChatMessage>();

        private List<ChatMessageRow> Rows { get; } = new List<ChatMessageRow>();

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

            Scrollbar.Width = 4;
            Scrollbar.Tint = ColorHelper.HexToColor("#D9E3F4");

            ActiveChannel.ValueChanged += OnActiveChannelChanged;
            ChangeChannel(ActiveChannel.Value);
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = IsHovered();

            if (Math.Abs(PreviousWidth - Width) > 0.001f || Math.Abs(PreviousHeight - Height) > 0.001f)
            {
                PreviousWidth = Width;
                PreviousHeight = Height;
                RebuildRows();
            }

            SynchronizeMessageStore();
            base.Update(gameTime);

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
            AppliedStoreGeneration = -1;
            AppliedStoreVersion = -1;
            ClearRows();

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
            if (snapshot.Generation == AppliedStoreGeneration && snapshot.Version == AppliedStoreVersion)
                return;

            // Rebuild can happen after the Message store becomes invalid,
            // which usually is after changing channel or loosing connexion and reconnecting to the game
            var rebuild = snapshot.Generation != AppliedStoreGeneration;
            var shouldRemainAtBottom = rebuild || IsNearBottom();

            ApplySnapshot(snapshot.Messages, rebuild);
            AppliedStoreGeneration = snapshot.Generation;
            AppliedStoreVersion = snapshot.Version;

            if (shouldRemainAtBottom)
                ScrollToBottom(rebuild ? 0 : 150);
        }

        private void ApplySnapshot(List<ChatMessage> messages, bool rebuild)
        {
            if (rebuild)
            {
                RebuildRows(messages);
                return;
            }

            var overlap = FindOverlap(messages);

            if (Messages.Count != 0 && messages.Count != 0 && overlap == 0)
            {
                RebuildRows(messages);
                return;
            }

            var removedCount = Messages.Count - overlap;
            for (var i = 0; i < removedCount; i++)
            {
                Rows[0].Destroy();
                Rows.RemoveAt(0);
                Messages.RemoveAt(0);
            }

            for (var i = overlap; i < messages.Count; i++)
                AddRow(messages[i]);

            RebuildRows();
        }

        private int FindOverlap(List<ChatMessage> messages)
        {
            var maximumOverlap = Math.Min(Messages.Count, messages.Count);
            for (var overlap = maximumOverlap; overlap > 0; overlap--)
            {
                var currentStart = Messages.Count - overlap;
                var matches = true;

                for (var i = 0; i < overlap; i++)
                {
                    if (MessagesMatch(Messages[currentStart + i], messages[i]))
                        continue;

                    matches = false;
                    break;
                }

                if (matches)
                    return overlap;
            }

            return 0;
        }

        private void RebuildRows(List<ChatMessage> messages)
        {
            ClearRows();

            foreach (var message in messages)
                AddRow(message);

            RebuildRows();
        }
        private void RebuildRows()
        {
            var y = 0f;
            var rowWidth = GetRowWidth();

            foreach (var row in Rows)
            {
                row.UpdateContent(row.Item, rowWidth);
                row.Y = y;
                y += row.Height;
            }

            ContentContainer.Height = Math.Max(Height, y);
        }

        private void AddRow(ChatMessage message)
        {
            var row = new ChatMessageRow(message, GetRowWidth(), this);
            row.UpdateWhenInvisible = false;
            row.SenderMenuRequested += OnSenderMenuRequested;

            AddContainedDrawable(row);

            Messages.Add(message);
            Rows.Add(row);
        }

        private void ClearRows()
        {
            foreach (var row in Rows)
            {
                row.SenderMenuRequested -= OnSenderMenuRequested;
                row.Destroy();
            }

            Rows.Clear();
            Messages.Clear();
        }

        private float GetRowWidth() => Math.Max(1, Width - Scrollbar.Width - 10);

        private bool IsNearBottom() => ContentContainer.Height <= Height || ContentContainer.Height - Height + ContentContainer.Y <= 40;

        private void ScrollToBottom(int duration)
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

        private void OnSenderMenuRequested(User user) => SenderMenuRequested?.Invoke(user);

        private void OnActiveChannelChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e) => ChangeChannel(e.Value);

        public override void Destroy()
        {
            ActiveChannel.ValueChanged -= OnActiveChannelChanged;
            ClearRows();
            SenderMenuRequested = null;
            base.Destroy();
        }
    }
}
