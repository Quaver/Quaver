using System;
using Quaver.Logging;
using Quaver.Net.Client;
using Steamworks;

namespace Quaver.Online
{
    public static class OnlineManager
    {
        /// <summary>
        ///     The online client for us (self)
        /// </summary>
        public static OnlineClient Self { get; private set; }

        /// <summary>
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            Self = new OnlineClient();

            Logger.LogImportant($"Attempting to log into the Quaver server...", LogType.Network);

            if (!SteamManager.AuthSessionTicketValidated)
            {
                Logger.LogError($"Could not log in because the steam auth session ticket was not validated.", LogType.Network);
                throw new Exception("Failed to login");
            }

            Self.Login(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(), SteamManager.PTicket, SteamManager.PcbTicket);
        }
    }
}