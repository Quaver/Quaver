using System;
using System.Collections.Generic;
using Quaver.Logging;
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

            // In the event that the chat channel doesn't already exist, we'll want to add a new one in.
            if (channel == null)
            {
                channel = new ChatChannel()
                {
                    Name = e.Message.Channel
                };

                JoinedChatChannels.Add(channel);
                Logger.LogImportant($"Added ChatChannel: {channel.Name}, as we have received a message and it did not exist", LogType.Network);
            }

            // Add the message to the appropriate channel.
            channel.Messages.Add(e.Message);

            Logger.LogInfo($"Received a chat message: [{e.Message.Time}] {e.Message.Channel} | {e.Message.Sender} | {e.Message.Message}", LogType.Network);
        }
    }
}