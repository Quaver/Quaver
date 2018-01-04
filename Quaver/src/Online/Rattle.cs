using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static void OnRattleLoginReply(object sender, PacketEventArgs e)
        {
            Console.WriteLine(e.Data);
            Logger.Log("Received a login reply from the Quaver server", LogColors.GameInfo);
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
