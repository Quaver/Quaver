using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;

namespace Quaver.Online.Events
{
    internal static class JoinedChatChannel
    {
        /// <summary>
        ///     Called when the server confirms that we've successfully joined a chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnChatChannelJoined(object sender, JoinedChatChannelEventArgs e)
        {
            Logger.LogSuccess($"Successfully joined Chat Channel: {e.Channel.Name}", LogType.Network);

            // TODO: Do something with the old messages you get.
            foreach (var msg in e.PreviousMessages)
                Logger.LogSuccess($"[{msg.DateTime.Hour}:{msg.DateTime.Minute}:{msg.DateTime.Second}] {msg.Channel} - {msg.Sender}: {msg.Text}", LogType.Network);
        }
    }
}