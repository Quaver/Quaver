using System;
using System.Collections.Generic;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.BlockedUsers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Online.Chat
{
    public static class ChatSession
    {
        public static Bindable<ChatChannel> ActiveChannel { get; } = new Bindable<ChatChannel>(null);

        public static BindableList<ChatChannel> AvailableChannels { get; } = new BindableList<ChatChannel>(new List<ChatChannel>());

        public static BindableList<ChatChannel> JoinedChannels { get; } = new BindableList<ChatChannel>(new List<ChatChannel>());

        public static event Action<ChatChannel> ChannelUpdated;

        public static event Action<ChatChannel, ChatMessage> DirectMessageReceived;

        private static OnlineClient SubscribedClient { get; set; }

        static ChatSession()
        {
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            ActiveChannel.ValueChanged += OnActiveChannelChanged;

            if (OnlineManager.Status.Value == ConnectionStatus.Connected)
                SubscribeToOnlineEvents();
        }

        private static void OnActiveChannelChanged(object sender, BindableValueChangedEventArgs<ChatChannel> e)
        {
            if (e.Value == null)
                return;

            e.Value.IsUnread = false;
            e.Value.IsMentioned = false;
            ChannelUpdated?.Invoke(e.Value);
        }

        private static void OnConnectionStatusChanged(object sender,
            BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value != ConnectionStatus.Connected)
            {
                UnsubscribeFromOnlineEvents();
                RemoveSpecialChannels();
                return;
            }

            SubscribeToOnlineEvents();

            AvailableChannels.Clear();
            Logger.Important("Cleared previously available chat channels", LogType.Runtime);

            foreach (var channel in JoinedChannels.Value)
            {
                if (channel.IsPrivate)
                    continue;

                OnlineManager.Client?.JoinChatChannel(channel.Name);
                Logger.Important($"Requested to rejoin chat channel: {channel.Name}", LogType.Runtime);
            }
        }

        private static void SubscribeToOnlineEvents()
        {
            if (OnlineManager.Client == null || ReferenceEquals(SubscribedClient, OnlineManager.Client))
                return;

            UnsubscribeFromOnlineEvents();
            SubscribedClient = OnlineManager.Client;
            SubscribedClient.OnAvailableChatChannel += OnAvailableChatChannel;
            SubscribedClient.OnFailedToJoinChatChannel += OnFailedToJoinChatChannel;
            SubscribedClient.OnJoinedChatChannel += OnJoinedChatChannel;
            SubscribedClient.OnLeftChatChannel += OnLeftChatChannel;
            SubscribedClient.OnChatMessageReceived += OnChatMessageReceived;
        }

        private static void UnsubscribeFromOnlineEvents()
        {
            if (SubscribedClient == null)
                return;

            SubscribedClient.OnAvailableChatChannel -= OnAvailableChatChannel;
            SubscribedClient.OnFailedToJoinChatChannel -= OnFailedToJoinChatChannel;
            SubscribedClient.OnJoinedChatChannel -= OnJoinedChatChannel;
            SubscribedClient.OnLeftChatChannel -= OnLeftChatChannel;
            SubscribedClient.OnChatMessageReceived -= OnChatMessageReceived;
            SubscribedClient = null;
        }

        private static void OnAvailableChatChannel(object sender, AvailableChatChannelEventArgs e)
        {
            if (AvailableChannels.Value.Contains(e.Channel))
                return;

            AvailableChannels.Add(e.Channel);
            Logger.Important($"Received available chat channel: {e.Channel.Name}", LogType.Runtime);
        }

        private static void OnFailedToJoinChatChannel(object sender, FailedToJoinChatChannelEventArgs e)
        {
            var message = $"Failed to join channel: {e.Channel}";

            NotificationManager.Show(NotificationLevel.Error, message);
            Logger.Important(message, LogType.Runtime);
        }

        private static void OnJoinedChatChannel(object sender, JoinedChatChannelEventArgs e)
        {
            var channel = AvailableChannels.Value.Find(x => x.Name == e.Channel) ?? new ChatChannel
            {
                Name = e.Channel,
                Description = "No Description",
                AllowedUserGroups = UserGroups.Normal
            };

            var existingChannel = JoinedChannels.Value.Find(x => x.Name == e.Channel);
            if (existingChannel != null)
                channel = existingChannel;
            else
                AddChannel(channel);

            Logger.Important($"Joined chat channel: {channel.Name} | {channel.Description}", LogType.Runtime);
        }

        private static void OnLeftChatChannel(object sender, LeftChatChannelEventArgs e)
        {
            var channel = JoinedChannels.Value.Find(x => x.Name == e.ChannelName);
            if (channel == null)
                return;

            channel.Close();
            Logger.Important($"Left chat channel: {channel.Name} | {channel.Description}", LogType.Runtime);
        }

        private static void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (OnlineManager.Self != null && e.Message.SenderId == OnlineManager.Self.OnlineUser.Id)
                return;

            if (BlockedUsers.IsUserBlocked(e.Message.SenderId))
                return;

            if (!OnlineManager.OnlineUsers.TryGetValue(e.Message.SenderId, out var onlineUser))
                return;

            e.Message.Sender = onlineUser;

            var isDirectMessage = !e.Message.Channel.StartsWith("#");
            var channel = isDirectMessage
                ? JoinedChannels.Value.Find(x => x.Name == e.Message.SenderName)
                : JoinedChannels.Value.Find(x => x.Name == e.Message.Channel);

            if (channel == null && isDirectMessage)
            {
                channel = AddChannel(new ChatChannel
                {
                    Name = e.Message.SenderName,
                    Description = "Private Chat",
                    DirectMessageUser = onlineUser
                }, false);
            } 
            else if(isDirectMessage)
            {
                channel.DirectMessageUser ??= onlineUser;
            }

            if (channel == null)
                return;

            if (ActiveChannel.Value != channel)
            {
                channel.IsUnread = true;
                ChannelUpdated?.Invoke(channel);
            }

            channel.QueueMessage(e.Message);

            if (isDirectMessage)
                DirectMessageReceived?.Invoke(channel, e.Message);
        }

        public static ChatChannel AddChannel(ChatChannel channel, bool activate = true)
        {
            var existingChannel = JoinedChannels.Value.Find(x => x.Name == channel.Name);
            if (existingChannel != null)
            {
                ChatMessageStore.GetOrCreate(existingChannel);

                if (activate)
                    ActiveChannel.Value = existingChannel;

                return existingChannel;
            }

            channel.Closed += OnChannelClosed;
            ChatMessageStore.GetOrCreate(channel);
            JoinedChannels.Add(channel);

            if (activate)
                ActiveChannel.Value = channel;

            return channel;
        }

        private static void OnChannelClosed(object sender, ChannelClosedEventArgs e)
        {
            if (!JoinedChannels.Value.Contains(e.Channel))
                return;

            JoinedChannels.Remove(e.Channel);

            if (ActiveChannel.Value == e.Channel)
                ActiveChannel.Value = JoinedChannels.Value.Count == 0 ? null : JoinedChannels.Value[0];
        }

        private static void RemoveSpecialChannels()
        {
            var channels = JoinedChannels.Value.FindAll(x => x.Name.StartsWith("#multi") || x.Name.StartsWith("#spectator"));

            foreach (var channel in channels)
                channel.Close();
        }
    }
}
