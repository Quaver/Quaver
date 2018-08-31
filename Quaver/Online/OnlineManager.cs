using System;
using Amib.Threading;
using Quaver.Logging;
using Quaver.Scheduling;
using Quaver.Server.Client;
using Steamworks;

namespace Quaver.Online
{
    public static class OnlineManager
    {
        /// <summary>
        ///    The online client that connects to the Quaver servers.
        /// </summary>
        private static OnlineClient _onlineClient;
        public static OnlineClient Client
        {
            get => _onlineClient;
            set
            {
                if (_onlineClient == null)
                    _onlineClient = value;
                else
                    throw new InvalidOperationException("OnlineClient can only be created once.");
            }
        }

        /// <summary>
        ///     The thread that online actions will take place on.
        /// </summary>
        private static SmartThreadPool Thread { get; } = new SmartThreadPool();

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

            if (Client == null)
                Client = new OnlineClient();
            else
            {
                /*if (Client.IsConnected)
                    return;*/
            }

            // Initiate the connection to the game server.
            Thread.QueueWorkItem(() => Client.Connect(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(), SteamManager.PTicket, SteamManager.PcbTicket));
        }
    }
}