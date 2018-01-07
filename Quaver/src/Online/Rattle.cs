using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Framework.Events.Packets;
using Quaver.Framework.Events.Packets.Structures;
#if !PUBLIC
using Quaver.Framework.Events;
using Quaver.Logging;

namespace Quaver.Online
{
    internal class Rattle
    {
        /// <summary>
        ///     Initialize a new OnlineEvents instance
        /// </summary>
        internal static OnlineEvents OnlineEvents { get; set; } = new OnlineEvents();

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
            Logger.Log("A connection error occurred.", LogColors.GameError);
        }

        /// <summary>
        ///     On Disconnection hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDisconnection(object sender, PacketEventArgs e)
        {
            Console.WriteLine(e.Data);
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

            var log = $"Successfully logged in as {response.CurrentClient.Username} #{response.CurrentClient.UserId} \n" +
                      $"You are logging in from {response.CurrentClient.Country} w/ time offset: {response.CurrentClient.TimeOffset} \n" +
                      $"There are currently: {response.OnlineClients.Count + 1} users online.";

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
    }
}
#endif