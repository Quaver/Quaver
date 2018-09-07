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
        ///     Called whenever we receive new chat messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            Console.WriteLine($"Received a chat message: [{e.Message.Time}] {e.Message.Channel} | {e.Message.Sender} | {e.Message.Message}");
        }
    }
}