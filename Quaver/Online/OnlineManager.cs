using System;
using Quaver.Logging;
using Steamworks;

namespace Quaver.Online
{
    public static class OnlineManager
    {
        /// <summary>
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            Logger.LogImportant($"Attempting to log into the Quaver server...", LogType.Network);

            if (!SteamManager.AuthSessionTicketValidated)
            {
                Logger.LogError($"Could not log in because the steam auth session ticket was not validated.", LogType.Network);
                throw new Exception("Failed to login");
            }


        }
    }
}