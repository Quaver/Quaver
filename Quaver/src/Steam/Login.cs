using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Constants;
using Quaver.Net.Handlers;

namespace Quaver.Steam
{
    public class Login
    {
        /// <summary>
        ///     After logging in, the following event handler will execute
        ///     based on the error code received when logging in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnLoginReplyEvent(object sender, LoginReplyEventArgs e)
        {
            switch (e.LoginErrorCode)
            {
                case LoginErrorCodes.Success:
                    Logger.LogSuccess($"Successfully logged in as {Flamingo.Self.Username} <{Flamingo.Self.SteamId}", LogType.Network);
                    Logger.LogSuccess($"There are currently {Flamingo.Clients.Count + 1} users online.", LogType.Network);
                    break;
                case LoginErrorCodes.Banned:
                    Logger.LogError($"You are banned.", LogType.Runtime);
                    break;
                case LoginErrorCodes.AlreadyConnected:
                    Logger.LogError($"You are already connected to the server in another location.", LogType.Network);
                    break;
                case LoginErrorCodes.ServerError:
                    Logger.LogError($"An internal server error has occurred", LogType.Network);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
