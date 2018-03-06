using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;
using Quaver.Net.Requests.Events;

namespace Quaver.Online.Events
{
    internal static class UserData
    {
        /// <summary>
        ///     Called when we receive new and updated user information from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnResponseUserDataHandler(object sender, ResponseUserDataEventArgs e)
        {
            Logger.LogSuccess($"Received updated user information for {e.User.Username} <{e.User.SteamId}> ({e.User.UserId})", LogType.Network);
        }
    }
}
