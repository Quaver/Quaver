using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Editor.UI.Dialogs.GoTo;
using WebSocketSharp;
using Wobble;
using Wobble.Logging;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.IPC
{
    public static class QuaverIpcHandler
    {
        private const string protocolUriStarter = "quaver://";

        /// <summary>
        ///     Handles messages from IPC
        /// </summary>
        /// <param name="message"></param>
        public static void HandleMessage(string message)
        {
            Logger.Important($"Received IPC Message: {message}", LogType.Runtime);

            if (!message.StartsWith(protocolUriStarter))
                return;

            var regex = new Regex(@"quaver:\/\/");
            message = regex.Replace(message, "", 1);

            // Highlighting notes within the editor
            if (message.StartsWith("editor/"))
            {
                message = message.Replace("editor/", "");
                message = message.Replace("%7C", "|");

                var game = GameBase.Game as QuaverGame;

                if (game?.CurrentScreen is EditorScreen)
                    EditorGoToObjectsPanel.HighlightObjects(message);
                else
                    NotificationManager.Show(NotificationLevel.Warning, "You must be in the editor to use this function!");
            }
        }
    }
}