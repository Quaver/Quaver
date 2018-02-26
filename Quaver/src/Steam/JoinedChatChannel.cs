﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;

namespace Quaver.Steam
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
        }
    }
}
