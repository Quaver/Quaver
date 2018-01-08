using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !PUBLIC
using Quaver.Framework.Events.Packets;
using Quaver.Framework.Events.Packets.Structures;
using Quaver.Framework.Events;
using Quaver.Logging;

namespace Quaver.Online
{
    internal static class Rattle
    {
        /// <summary>
        ///     Initialize a new OnlineEvents instance
        /// </summary>
        internal static OnlineEvents OnlineEvents { get; set; } = new OnlineEvents();

        /// <summary>
        ///     Keeps track if the user is still logged in
        /// </summary>
        internal static bool IsLoggedIn;

        /// <summary>
        ///     The online client that is logged in.
        /// </summary>
        internal static OnlineClient Client { get; set; }

        /// <summary>
        ///     The list currently online users
        /// </summary>
        internal static List<OnlineClient> OnlineClients { get; set; }

        /// <summary>
        ///     Initializes the online event hooking
        /// </summary>
        internal static void Initialize()
        {
            OnlineEvents.Connecting += OnConnecting;
            OnlineEvents.ConnectionError += OnConnectionError;
            OnlineEvents.Disconnection += OnDisconnection;
            OnlineEvents.RattleLoginReply += OnRattleLoginReply;
            OnlineEvents.RattleUserConnected += OnRattleUserConnected;
            OnlineEvents.RattleUserDisconnected += OnRattleUserDisconnected;
        }

        /// <summary>
        ///     On connection hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConnecting(object sender, EventArgs e)
        {
            Logger.Log("Connecting to the Quaver server.", LogColors.GameInfo);
        }

        /// <summary>
        ///     On connection error hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConnectionError(object sender, PacketEventArgs e)
        {
            // Log out user
            ResetLogin();

            Logger.Log("A connection error occurred.", LogColors.GameError);
        }

        /// <summary>
        ///     On Disconnection hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDisconnection(object sender, PacketEventArgs e)
        {
            // Log out user
            ResetLogin();
            Logger.Log("Disconnected from the Quaver server", LogColors.GameWarning);
        }

        /// <summary>
        ///     On RattleLoginReply Hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRattleLoginReply(object sender, LoginReplyEventArgs e)
        {
            var response = e.Data;

            // Handle incorrect login errors
            if (!response.Success)
            {
                switch (response.Error)
                {
                    case LoginError.None:
                        // This should never happen - server will always return a successful login w/ no error
                        break;
                    case LoginError.AlreadyConnected:
                        break;
                    case LoginError.Banned:
                        break;
                    case LoginError.InvalidCredentials:
                        break;
                    case LoginError.Require2FA:
                        break;
                    default:
                        break;
                }

                Logger.Log($"Login failed", LogColors.GameError);
                return;
            }

            // Set the current client 
            Client = response.CurrentClient;
            IsLoggedIn = true;

            // Add self to list of online clients
            OnlineClients = response.OnlineClients;
            OnlineClients.Add(Client);

            var log = $"Successfully logged in as {Client.Username} #{Client.UserId} \n" +
                      $"You are logging in from {Client.Country} w/ time offset: {Client.TimeOffset} \n" +
                      $"There are currently: {OnlineClients.Count} users online.";

            Logger.Log(log, LogColors.GameInfo);
        }

        /// <summary>
        ///     On RattleUserConnected hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRattleUserConnected(object sender, PacketEventArgs e)
        {
            Console.WriteLine(e.Data);
            Logger.Log("Received RattleUserConnected packet", LogColors.GameInfo);
        }

        /// <summary>
        ///     On RattleUserDisconnected hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRattleUserDisconnected(object sender, PacketEventArgs e)
        {
            Console.WriteLine(e.Data);
            Logger.Log("Received RattleUserDisconnected packet", LogColors.GameInfo);
        }

        /// <summary>
        ///     Logs out the user completely
        /// </summary>
        internal static void ResetLogin(bool tellServer = false)
        {
            IsLoggedIn = false;
            Client = null;
            OnlineClients = null;
        }
    }
}
#endif