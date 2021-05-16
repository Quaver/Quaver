using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Online.API.Mapsets;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Editor.UI.Dialogs.GoTo;
using Quaver.Shared.Screens.Importing;
using Quaver.Shared.Screens.Selection;
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

        private static Dictionary<string, Action<string>> messageHandlers = new Dictionary<string, Action<string>>()
        {
            {"editor", HandleEditorMessage},
            {"map", HandleMapMessage},
            {"mapset", HandleMapsetMessage},
        };

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
            foreach (var (key, handler) in messageHandlers)
            {
                if (!message.StartsWith(key + "/")) continue;
                message = message.Replace(key + "/", "");
                handler(message);
                break;
            }
        }

        /// <summary>
        ///     Highlights notes within the editor
        /// </summary>
        /// <param name="message"></param>
        private static void HandleEditorMessage(string message)
        {
            message = message.Replace("%7C", "|");

            var game = GameBase.Game as QuaverGame;

            if (game?.CurrentScreen is EditScreen screen)
                screen.GoToObjects(message);
            else
                NotificationManager.Show(NotificationLevel.Warning,
                    "You must be in the editor to use this function!");
        }

        /// <summary>
        ///     Selects a map in song select and downloads it if not present
        /// </summary>
        /// <param name="message"></param>
        private static void HandleMapMessage(string message)
        {
            int mapId;
            if (!int.TryParse(message, out mapId))
            {
                NotificationManager.Show(NotificationLevel.Warning, "The provided ID was not a number!");
                return;
            }

            var game = (QuaverGame) GameBase.Game;

            // Check if we have the map installed
            var map = MapManager.FindMapFromOnlineId(mapId);
            if (map != null)
            {
                if (!CanGoToSongSelect(game.CurrentScreen.Type)) return;
                game.CurrentScreen.Exit(() => new SelectionScreen());
                var screen = game.CurrentScreen as SelectionScreen;
                MapManager.Selected.Value = map;

                lock (screen.AvailableMapsets.Value)
                    screen.AvailableMapsets.Value = MapsetHelper.FilterMapsets(screen.CurrentSearchQuery);
                return;
            }

            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You must be logged in to download maps!");
                return;
            }

            // Make sure they can visit the song select before making API requests
            // It's possible that something triggers an IPC during gameplay and download/import would interrupt gameplay
            if (!CanGoToSongSelect(game.CurrentScreen.Type)) return;

            try
            {
                // Check if the map exists online
                var request = new APIRequestMapInformation(mapId);
                var response = request.ExecuteRequest();
                if (response.Status == 400 || response.Map == null || response.Map.MapsetId == -1)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "The map does not exist on the server!");
                    return;
                }
                else if (response.Status != 200)
                {
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while requesting mapset data from the server");
                    return;
                }

                DownloadAndImport(response.Map.MapsetId, response.Map.Artist, response.Map.Title, game);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error,
                    "There was an issue while requesting mapset data from the server or downloading!");
            }
        }

        /// <summary>
        ///     Selects a mapset in song select and downloads it if not present
        /// </summary>
        /// <param name="message"></param>
        private static void HandleMapsetMessage(string message)
        {
            int mapsetId;
            if (!int.TryParse(message, out mapsetId))
            {
                NotificationManager.Show(NotificationLevel.Warning, "The provided ID was not a number!");
                return;
            }

            var game = (QuaverGame) GameBase.Game;

            // Check if we have the mapset installed
            if (MapManager.Mapsets.Count != 0)
            {
                var mapset = MapManager.Mapsets.Find(x => x.Maps.First().Game == MapGame.Quaver
                                                          && x.Maps.First().MapSetId == mapsetId);

                if (mapset != null)
                {
                    if (!CanGoToSongSelect(game.CurrentScreen.Type)) return;
                    game.CurrentScreen.Exit(() => new SelectionScreen());
                    var screen = game.CurrentScreen as SelectionScreen;

                    MapManager.Selected.Value = mapset.Maps.First();
                    lock (screen.AvailableMapsets.Value)
                        screen.AvailableMapsets.Value = MapsetHelper.FilterMapsets(screen.CurrentSearchQuery);
                    return;
                }
            }

            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You must be logged in to download mapsets!");
                return;
            }

            // Make sure they can visit the song select before making API requests
            // It's possible that something triggers an IPC during gameplay and download/import would interrupt gameplay
            if (!CanGoToSongSelect(game.CurrentScreen.Type)) return;

            try
            {
                // Check if the mapset exists online
                var request = new APIRequestMapsetInformation(mapsetId);
                var response = request.ExecuteRequest();
                if (response.Status == 400 || response.Mapset == null || response.Mapset.Id == -1)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "The map does not exist on the server!");
                    return;
                }
                else if (response.Status != 200)
                {
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while requesting mapset data from the server!");
                    return;
                }

                DownloadAndImport(response.Mapset.Id, response.Mapset.Artist, response.Mapset.Title, game);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error,
                    "There was an issue while requesting mapset data from the server or downloading!");
            }
        }

        private static bool CanGoToSongSelect(QuaverScreenType type)
        {
            var allowed = SelectionScreen.CanGoToSongSelect(type);
            if (!allowed)
                NotificationManager.Show(NotificationLevel.Warning,
                    "This screen does not support map IPC messages, please visit the song select screen!");
            return allowed;
        }

        private static void DownloadAndImport(int mapsetId, string artist, string title, QuaverGame game)
        {
            // User doesn't have the map, so download it for them
            if (MapsetDownloadManager.CurrentDownloads.All(x => x.MapsetId != mapsetId))
            {
                var download =
                    MapsetDownloadManager.Download(mapsetId, artist, title);
                MapsetDownloadManager.OpenOnlineHub();

                // Auto import
                download.Completed.ValueChanged += (sender, args) =>
                {
                    game.CurrentScreen.Exit(() => new ImportingScreen(null, true));
                    var dialog = DialogManager.Dialogs.Find(x => x is OnlineHubDialog) as OnlineHubDialog;
                    dialog?.Close();
                };
            }
        }
    }
}