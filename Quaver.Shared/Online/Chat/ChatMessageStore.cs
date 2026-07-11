using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quaver.Server.Client;
using Quaver.Server.Client.Events;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Database.BlockedUsers;
using Wobble.Logging;
using Wobble.Scheduling;

namespace Quaver.Shared.Online.Chat
{
    /// <summary>
    ///     Retains the authoritative messages for a joined chat channel independently of any chat UI.
    /// </summary>
    public sealed class ChatMessageStore : IDisposable
    {
        /// <summary>
        ///     The maximum number of messages kept for a channel.
        /// </summary>
        private const int MaxRetainedMessages = 250;

        /// <summary>
        ///     Stores are shared by every chat view for a channel.
        /// </summary>
        private static Dictionary<string, ChatMessageStore> Stores { get; } = new Dictionary<string, ChatMessageStore>();

        /// <summary>
        ///     Synchronizes store creation and removal.
        /// </summary>
        private static object StoresLock { get; } = new object();

        /// <summary>
        ///     Synchronizes the message snapshot and its state.
        /// </summary>
        private object MessageLock { get; } = new object();

        /// <summary>
        ///     The chat channel represented by this store.
        /// </summary>
        public ChatChannel Channel { get; }

        /// <summary>
        ///     The client subscribed when the store was created.
        /// </summary>
        private OnlineClient Client { get; }

        /// <summary>
        ///     Retrieves the initial history without blocking rendering.
        /// </summary>
        private TaskHandler<int, List<ChatMessage>> RequestHistoryTask { get; }

        /// <summary>
        ///     If the current history request has begun.
        /// </summary>
        private bool HasRequestedMessageHistory { get; set; }

        /// <summary>
        ///     If the initial history has completed, including an empty response.
        /// </summary>
        private bool HasLoadedMessageHistory { get; set; }

        /// <summary>
        ///     Changes whenever the retained message list is reset.
        /// </summary>
        private long Generation { get; set; }

        /// <summary>
        ///     Changes whenever the retained message list changes.
        /// </summary>
        private long Version { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static ChatMessageStore GetOrCreate(ChatChannel channel)
        {
            lock (StoresLock)
            {
                var key = channel.Name ?? string.Empty;

                if (Stores.TryGetValue(key, out var store) && ReferenceEquals(store.Channel, channel))
                    return store;

                store?.Dispose();
                store = new ChatMessageStore(channel);
                Stores[key] = store;
                return store;
            }
        }

        /// <summary>
        ///     Removes the store once its channel is closed.
        /// </summary>
        /// <param name="channel"></param>
        public static void Remove(ChatChannel channel)
        {
            lock (StoresLock)
            {
                var key = channel.Name ?? string.Empty;

                if (!Stores.TryGetValue(key, out var store) || !ReferenceEquals(store.Channel, channel))
                    return;

                Stores.Remove(key);
                store.Dispose();
            }
        }

        /// <summary>
        ///     Returns a stable message snapshot for code that does not render a chat view.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static List<ChatMessage> GetMessageSnapshot(ChatChannel channel)
        {
            lock (StoresLock)
            {
                var key = channel.Name ?? string.Empty;

                if (Stores.TryGetValue(key, out var store) && ReferenceEquals(store.Channel, channel))
                    return store.GetSnapshot().Messages;
            }

            lock (channel.Messages)
                return channel.Messages.ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        private ChatMessageStore(ChatChannel channel)
        {
            Channel = channel;
            Client = OnlineManager.Client;
            RequestHistoryTask = new TaskHandler<int, List<ChatMessage>>(RunRequestHistoryTask);
            RequestHistoryTask.OnCompleted += OnHistoryRequestCompleted;

            Channel.MessageQueued += OnMessageQueued;
            Channel.Closed += OnChannelClosed;

            if (Client != null)
            {
                Client.OnChatMessageReceived += OnChatMessageReceived;
                Client.OnConnectionStatusChanged += OnConnectionStatusChanged;
            }

            RequestMessageHistoryIfNecessary();
        }

        /// <summary>
        ///     Returns a stable view of the messages retained by this channel.
        /// </summary>
        /// <returns></returns>
        public ChatMessageStoreSnapshot GetSnapshot()
        {
            lock (MessageLock)
                return new ChatMessageStoreSnapshot(Channel.Messages.ToList(), HasLoadedMessageHistory, Generation, Version);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            RequestHistoryTask.OnCompleted -= OnHistoryRequestCompleted;
            RequestHistoryTask.Dispose();

            Channel.MessageQueued -= OnMessageQueued;
            Channel.Closed -= OnChannelClosed;

            if (Client == null)
                return;

            Client.OnChatMessageReceived -= OnChatMessageReceived;
            Client.OnConnectionStatusChanged -= OnConnectionStatusChanged;
        }

        /// <summary>
        ///     Starts the initial history request once the online client is connected.
        /// </summary>
        private void RequestMessageHistoryIfNecessary(bool connectionAlreadyConfirmed = false)
        {
            lock (MessageLock)
            {
                if (HasRequestedMessageHistory || (!connectionAlreadyConfirmed && !OnlineManager.Connected))
                    return;

                HasRequestedMessageHistory = true;
            }

            Logger.Important($"Fetching message history for channel: {Channel.Name}", LogType.Runtime);
            RequestHistoryTask.Run(0);
        }

        /// <summary>
        ///     Fetches and converts the server history for this channel.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<ChatMessage> RunRequestHistoryTask(int val, CancellationToken token)
        {
            // Visual tests can replace the active client after the store is created.
            var client = OnlineManager.Client ?? Client ?? new OnlineClient();
            var history = client.GetChannelMessageHistory(Channel);

            if (history == null)
                return new List<ChatMessage>();

            return (history.Messages ?? new List<GetChannelMessageHistoryMessageContainer>())
                .Where(x => !BlockedUsers.IsUserBlocked(x.User.Id))
                .Select(x => x.ToChatMessage(x.MakeUser()))
                .ToList();
        }

        /// <summary>
        ///     Merges the initially fetched history with messages received while it was loading.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHistoryRequestCompleted(object sender, TaskCompleteEventArgs<int, List<ChatMessage>> e)
        {
            lock (MessageLock)
            {
                foreach (var message in e.Result)
                    AddMessage(message);

                Channel.Messages.Sort((x, y) => x.Time.CompareTo(y.Time));
                TrimMessages();
                HasLoadedMessageHistory = true;
                Version++;
            }

            Logger.Important($"Finished fetching message history for channel: {Channel.Name} w/ {e.Result.Count} messages!", LogType.Runtime);
        }

        /// <summary>
        ///     Retains a message queued locally or received from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageQueued(object sender, MessageQueuedEventArgs e)
        {
            lock (MessageLock)
                AddMessage(e.Message);
        }

        /// <summary>
        ///     Receives messages for this channel and queues them through the shared channel object.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (BlockedUsers.IsUserBlocked(e.Message.SenderId))
                return;

            if (Channel.Name.StartsWith("#") && Channel.Name != e.Message.Channel)
                return;

            if (!Channel.Name.StartsWith("#"))
            {
                if (Channel.Name != e.Message.SenderName || e.Message.Channel != OnlineManager.Self?.OnlineUser.Username)
                    return;
            }

            if (!OnlineManager.OnlineUsers.TryGetValue(e.Message.SenderId, out var onlineUser))
                return;

            e.Message.Sender = onlineUser;

            if (OnlineManager.Self != null && e.Message.SenderId == OnlineManager.Self.OnlineUser.Id)
                return;

            Channel.QueueMessage(e.Message);
        }

        /// <summary>
        ///     Clears stale retained state and reloads history after a connection transition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            RequestHistoryTask.Cancel();

            lock (MessageLock)
            {
                Channel.Messages.Clear();
                HasRequestedMessageHistory = false;
                HasLoadedMessageHistory = false;
                Generation++;
                Version++;
            }

            if (e.Status == ConnectionStatus.Connected)
                RequestMessageHistoryIfNecessary(true);
        }

        /// <summary>
        ///     Removes this store when the server closes its channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChannelClosed(object sender, ChannelClosedEventArgs e) => Remove(e.Channel);

        /// <summary>
        ///     Adds a unique message to the canonical channel history.
        /// </summary>
        /// <param name="message"></param>
        private void AddMessage(ChatMessage message)
        {
            if (ContainsMessage(Channel.Messages, message))
                return;

            Channel.Messages.Add(message);
            TrimMessages();
            Version++;
        }

        /// <summary>
        ///     Keeps the canonical history bounded even while no chat view is open.
        /// </summary>
        private void TrimMessages()
        {
            if (Channel.Messages.Count > MaxRetainedMessages)
                Channel.Messages.RemoveRange(0, Channel.Messages.Count - MaxRetainedMessages);
        }

        /// <summary>
        ///     Messages can be recreated independently by the history and realtime paths.
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
    }

    /// <summary>
    ///     An immutable message-store view used by chat renderers.
    /// </summary>
    public class ChatMessageStoreSnapshot
    {
        /// <summary>
        /// </summary>
        public List<ChatMessage> Messages { get; }

        /// <summary>
        /// </summary>
        public bool HasLoadedMessageHistory { get; }

        /// <summary>
        /// </summary>
        public long Generation { get; }

        /// <summary>
        /// </summary>
        public long Version { get; }

        /// <summary>
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="hasLoadedMessageHistory"></param>
        /// <param name="generation"></param>
        /// <param name="version"></param>
        public ChatMessageStoreSnapshot(List<ChatMessage> messages, bool hasLoadedMessageHistory, long generation, long version)
        {
            Messages = messages;
            HasLoadedMessageHistory = hasLoadedMessageHistory;
            Generation = generation;
            Version = version;
        }
    }
}
