using System;
using System.Linq;
using System.Net;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Objects.Twitch;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.Maps;
using Quaver.Shared.Online.API.Mapsets;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Edit;
using Quaver.Shared.Screens.Importing;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

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
            else if (message.StartsWith("mapset/"))
                HandleMapsetSelection(message);
            else if (message.StartsWith("playlist/"))
                HandlePlaylistImport(message);
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

            var map = MapManager.FindMapFromOnlineId(id);

            if (SelectMapIfImported(map))
                return;

            if (!IsConnected())
                return;

            try
            {
                // Find mapset id & song name.
                var response = new APIRequestMapInformation(id).ExecuteRequest();

                if (response.Status == (int)HttpStatusCode.NotFound)
                {
                    NotificationManager.Show(NotificationLevel.Error, $"That map does not exist on the server.");
                    return;
                }

                if (response.Status != (int)HttpStatusCode.OK)
                    throw new Exception($"Failed map information `{id}` fetch with response: {response.Status}");

                DownloadMapAndImport(response.Map.MapsetId, response.Map.Artist, response.Map.Title, true);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, $"An error occurred while fetching map information.");
                Logger.Error(e, LogType.Network);
            }
        }

        /// <summary>
        ///     Selects a mapset if already imported or downloads it from the server.
        /// </summary>
        /// <param name="message"></param>
        private static void HandleMapsetSelection(string message)
        {
            message = message.Replace("mapset/", "");

            if (!int.TryParse(message, out var id))
            {
                NotificationManager.Show(NotificationLevel.Error, $"The provided mapset id was not a valid number.");
                return;
            }

            var mapset = MapManager.Mapsets.Find(x => x.Maps.First().MapSetId == id);

            if (SelectMapIfImported(mapset?.Maps.First()))
                return;

            if (!IsConnected())
                return;

            try
            {
                // Find mapset id & song name.
                var response = new APIRequestMapsetInformation(id).ExecuteRequest();

                if (response.Status == (int)HttpStatusCode.NotFound)
                {
                    NotificationManager.Show(NotificationLevel.Error, $"That mapset does not exist on the server.");
                    return;
                }

                if (response.Status != (int)HttpStatusCode.OK)
                    throw new Exception($"Failed mapset information `{id}` fetch with response: {response.Status}");

                DownloadMapAndImport(id, response.Mapset.Artist, response.Mapset.Title, false);
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, $"An error occurred while fetching mapset information.");
                Logger.Error(e, LogType.Network);
            }
        }

        /// <summary>
        ///     Imports an online playlist
        /// </summary>
        /// <param name="message"></param>
        private static void HandlePlaylistImport(string message)
        {
            message = message.Replace("playlist/", "");

            if (!int.TryParse(message, out var id))
            {
                NotificationManager.Show(NotificationLevel.Error, $"The provided playlist id was not a valid number.");
                return;
            }

            PlaylistManager.ImportPlaylist(id);
        }

        /// <summary>
        ///     Checks if the user is connected to the server & alerts them if they're not.
        /// </summary>
        /// <returns></returns>
        private static bool IsConnected()
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning, $"You must be logged in to download maps!");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Returns if the user is allowed to select a map/import on specific screens
        /// </summary>
        /// <returns></returns>
        private static bool IsSelectionAllowedOnScreen()
        {
            var game = (QuaverGame)GameBase.Game;

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

        /// <summary>
        ///     Selects a map if it is imported. Returns true if it was successfully selected.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private static bool SelectMapIfImported(Map map)
        {
            if (map == null)
                return false;

            var game = (QuaverGame)GameBase.Game;

            if (!IsSelectionAllowedOnScreen())
            {
                NotificationManager.Show(NotificationLevel.Warning, $"Please finish what you're doing before selecting this map!");
                return false;
            }

            if (game.CurrentScreen.Type == QuaverScreenType.Select)
                MapManager.PlaySongRequest(new SongRequest(), map);
            else
            {
                MapManager.Selected.Value = map;
                AudioEngine.LoadCurrentTrack();
            }

            return true;
        }

        /// <summary>
        ///     Downloads a map from the server.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <param name="isMap"></param>
        private static void DownloadMapAndImport(int id, string artist, string title, bool isMap)
        {
            var game = (QuaverGame)GameBase.Game;

            var dl = MapsetDownloadManager.Download(id, artist, title);
            MapsetDownloadManager.OpenOnlineHub();

            // Automatically import if the user is still in song select after completion.
            dl.Status.ValueChanged += (o, e) =>
            {
                if (!IsSelectionAllowedOnScreen() || e.Value.Status != FileDownloaderStatus.Complete)
                    return;

                var dialog = DialogManager.Dialogs.Find(x => x is OnlineHubDialog) as OnlineHubDialog;
                dialog?.Close();

                if (isMap)
                    game.CurrentScreen.Exit(() => new ImportingScreen(null, true, false, id));
                else
                    game.CurrentScreen.Exit(() => new ImportingScreen(null, true, false));
            };
        }
    }
}