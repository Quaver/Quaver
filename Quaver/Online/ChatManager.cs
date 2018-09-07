using System.Collections.Generic;
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
    }
}