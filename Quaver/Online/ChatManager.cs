using System;
using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;

namespace Quaver.Online
{
    public static class ChatManager
    {
        /// <summary>
        ///     The list of available public chat channels.
        /// </summary>
        public static List<ChatChannel> AvailableChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        ///     The list of chat channels the user is currently in.
        /// </summary>
        public static List<ChatChannel> JoinedChatChannels { get; } = new List<ChatChannel>();

        /// <summary>
        ///     Sends a chat message to the server.
        /// </summary>
        public static void SendMessage(ChatChannel chan, ChatMessage message)
        {
            // Add the message to the chat channel.
            // Find the channel the message is for.
            var channel = JoinedChatChannels.Find(x => x.Name == chan.Name);

            // Add the message to the appropriate channel.
            channel.Messages.Add(message);

            // Send the message to the server.
            OnlineManager.Client.SendMessage(chan.Name, message.Message);
        }

        /// <summary>
        ///     Called whenever we receive new chat messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            // Find the channel the message is for.
            var channel = JoinedChatChannels.Find(x => x.Name == e.Message.Channel);

            // Add the message to the appropriate channel.
            channel.Messages.Add(e.Message);

            Console.WriteLine($"Received a chat message: [{e.Message.Time}] {e.Message.Channel} | {e.Message.Sender} | {e.Message.Message}");
            Console.WriteLine($"There are now: {channel.Messages.Count} messages in channel: {channel.Name}");
        }
    }
}