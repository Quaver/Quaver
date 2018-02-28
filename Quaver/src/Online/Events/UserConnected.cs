using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Handlers;

namespace Quaver.Online.Events
{
    internal static class UserConnected
    {
        /// <summary>
        ///     Handles what happens when a new user connects to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnUserConnectedEvent(object sender, UserConnectedEventArgs e)
        {
            Logger.LogInfo($"{e.User.Username} <{e.User.SteamId}> has logged into the server.", LogType.Network);
            Logger.LogInfo($"There are now {Flamingo.Clients.Count + 1} users online", LogType.Network);
        }
    }
}
