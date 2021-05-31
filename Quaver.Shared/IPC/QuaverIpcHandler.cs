using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Editor.UI.Dialogs.GoTo;
using Quaver.Shared.Screens.Importing;
using WebSocketSharp;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
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

            if (message.StartsWith(protocolUriStarter))
                HandleProtocolMessage(message.Substring(protocolUriStarter.Length));
            else
            {
                // Quaver was launched with a file path, try to import it.
                MapsetImporter.ImportFile(message);
            }
        }

        /// <summary>
        ///     Handles a quaver:// message.
        ///     <param name="message">the IPC message with quaver:// stripped</param>
        /// </summary>
        public static void HandleProtocolMessage(string message)
        {
            if (message.StartsWith("editor/"))
                HandleEditorNoteHighlighting(message);
            else if (message.StartsWith("map/"))
                HandleMapSelection(message);
        }

        /// <summary>
        ///     Highlights notes within the editor
        /// </summary>
        /// <param name="message"></param>
        private static void HandleEditorNoteHighlighting(string message)
        {
            message = message.Replace("editor/", "");
            message = message.Replace("%7C", "|");

            var game = GameBase.Game as QuaverGame;

            if (game?.CurrentScreen is EditScreen screen)
                screen.GoToObjects(message);
            else
                NotificationManager.Show(NotificationLevel.Warning, "You must be in the editor to use this function!");
        }

        /// <summary>
        ///     Selects a map if already imported or downloads it from the server.
        /// </summary>
        /// <param name="message"></param>
        private static void HandleMapSelection(string message)
        {
            message = message.Replace("map/", "");

            if (!int.TryParse(message, out var id))
            {
                NotificationManager.Show(NotificationLevel.Error, $"The provided map id was not a valid number.");
                return;
            }

            var game = (QuaverGame) GameBase.Game;
            var map = MapManager.FindMapFromOnlineId(id);

            if (map != null)
            {
                if (!IsSelectionAllowedOnScreen())
                {
                    NotificationManager.Show(NotificationLevel.Warning, $"Please finish what you're doing before selecting this map!");
                    return;
                }

                if (game.CurrentScreen.Type == QuaverScreenType.Select)
                    MapManager.PlaySongRequest(new SongRequest(), map);
                else
                {
                    MapManager.Selected.Value = map;
                    AudioEngine.LoadCurrentTrack();
                }

                return;
            }

            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, $"You must be logged in to download maps!");
                return;
            }

            try
            {
                // Find mapset id & song name.
                var response = new APIRequestMapInformation(id).ExecuteRequest();

                if (response.Status == (int) HttpStatusCode.NotFound)
                {
                    NotificationManager.Show(NotificationLevel.Error, $"That map does not exist on the server.");
                    return;
                }

                if (response.Status != (int) HttpStatusCode.OK)
                    throw new Exception($"Failed map information `{id}` fetch with response: {response.Status}");

                var dl = MapsetDownloadManager.Download(response.Map.MapsetId, response.Map.Artist, response.Map.Title);
                MapsetDownloadManager.OpenOnlineHub();

                // Automatically import if the user is still in song select after completion.
                dl.Completed.ValueChanged += (o, e) =>
                {
                    if (!IsSelectionAllowedOnScreen())
                        return;

                    var dialog = DialogManager.Dialogs.Find(x => x is OnlineHubDialog) as OnlineHubDialog;
                    dialog?.Close();

                    game.CurrentScreen.Exit(() => new ImportingScreen(null, true, false, id));
                };
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, $"An error occurred while fetching map information.");
                Logger.Error(e, LogType.Network);
            }
        }

        /// <summary>
        ///     Returns if the user is allowed to select a map/import on specific screens
        /// </summary>
        /// <returns></returns>
        private static bool IsSelectionAllowedOnScreen()
        {
            var game = (QuaverGame) GameBase.Game;

            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Select:
                case QuaverScreenType.Menu:
                case QuaverScreenType.Lobby:
                case QuaverScreenType.Download:
                case QuaverScreenType.Music:
                    return true;
                default:
                    return false;
            }
        }
    }
}