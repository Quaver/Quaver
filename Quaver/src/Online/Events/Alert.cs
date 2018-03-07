using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Constants;
using Quaver.Net.Handlers;

namespace Quaver.Online.Events
{
    internal static class Alert
    {
        /// <summary>
        ///     Called when we receieve an alert from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnServerAlertReceived(object sender, ServerAlertEventArgs e)
        {
            // TODO: Display some sort of notification here
            switch (e.Alert.Type)
            {
                case AlertType.Error:
                    Logger.LogError(e.Alert.Message, LogType.Network);
                    break;
                case AlertType.Notification:
                    Logger.LogInfo(e.Alert.Message, LogType.Network);
                    break;
                default:
                    break;
            }
        }
    }
}
