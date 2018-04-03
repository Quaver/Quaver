using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;

namespace Quaver.Online.Events
{
    internal class ChatMessageReceived
    {
        /// <summary>
        ///     Called when we receive a chat message from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnChatMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            // Chat channel message
            if (e.Message.Channel.StartsWith("#"))
            {
                Logger.LogInfo($"[{e.Message.DateTime.Hour}:{e.Message.DateTime.Minute}:{e.Message.DateTime.Second}] Channel: {e.Message.Channel} - {e.Message.Sender} (#{e.Message.SenderId}): {e.Message.Text}", LogType.Network);
                return;
            }

            // Private Message
            Logger.LogInfo($"[{e.Message.DateTime.Hour}:{e.Message.DateTime.Minute}:{e.Message.DateTime.Second}] From: {e.Message.Sender} (#{e.Message.SenderId}): {e.Message.Text}", LogType.Network);
        }
    }
}