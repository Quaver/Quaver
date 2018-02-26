using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;

namespace Quaver.Steam
{
    internal static class LeftChatChannel
    {
        /// <summary>
        ///     Called when the server confirms that we've successfully left a chat channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnChatChannelLeft(object sender, LeftChatChannelEventArgs e)
        {
            Logger.LogSuccess($"Successfully left Chat Channel: {e.Channel.Name}", LogType.Network);
        }
    }
}
