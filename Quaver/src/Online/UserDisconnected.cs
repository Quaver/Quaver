using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Handlers;

namespace Quaver.Online
{
    internal static class UserDisconnected
    {
        /// <summary>
        ///     Handles what happens when a new user connects to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnUserDisconnectedEvent(object sender, UserDisconnectedEventArgs e)
        {
            Logger.LogInfo($"{e.User.Username} <{e.User.SteamId}> has disconnected from the server.", LogType.Network);
            Logger.LogInfo($"There are now {Flamingo.Clients.Count + 1} users online", LogType.Network);
        }
    }
}
