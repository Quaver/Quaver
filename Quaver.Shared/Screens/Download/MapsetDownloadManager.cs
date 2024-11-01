/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Download
{
    public static class MapsetDownloadManager
    {
        /// <summary>
        ///     All opf the currently downloading mapsets.
        /// </summary>
        public static List<MapsetDownload> CurrentDownloads { get; } = new List<MapsetDownload>();

        public static HashSet<MapsetDownload> CurrentActiveDownloads { get; } = new HashSet<MapsetDownload>();

        /// <summary>
        ///    The amount of mapsets able to be downloaded at once.
        /// </summary>
        public static int MAX_CONCURRENT_DOWNLOADS => OnlineManager.IsDonator ? 6 : 5;

        /// <summary>
        ///     Event invoked when a download has been added to the manager
        /// </summary>
        public static event EventHandler<MapsetDownloadAddedEventArgs> DownloadAdded;

        /// <summary>
        ///     Downloads an individual mapset.
        /// </summary>
        /// <param name="mapset"></param>
        public static MapsetDownload Download(JToken mapset)
        {
            // Require login in order to download.
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download mapsets!");
                return null;
            }

            if (CurrentDownloads.Any(x => x.MapsetId == (int)mapset["id"]))
                return null;

            var download = new MapsetDownload(mapset, mapset["artist"].ToString(), mapset["title"].ToString(),
                CurrentDownloads.Count + 1 <= MAX_CONCURRENT_DOWNLOADS);

            CurrentDownloads.Insert(0, download);

            DownloadAdded?.Invoke(typeof(MapsetDownloadManager), new MapsetDownloadAddedEventArgs(download));
            OpenOnlineHub();

            return download;
        }

        public static MapsetDownload Download(int id, string artist, string title)
        {
            // Require login in order to download.
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download mapsets!");
                return null;
            }

            if (CurrentDownloads.Any(x => x.MapsetId == id))
                return null;

            var download =
                new MapsetDownload(id, artist, title, CurrentActiveDownloads.Count < MAX_CONCURRENT_DOWNLOADS);

            DownloadAdded?.Invoke(typeof(MapsetDownloadManager), new MapsetDownloadAddedEventArgs(download));

            return download;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static MapsetDownload DownloadSharedMultiplayerMapset(string artist, string title)
        {
            // Require login in order to download.
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must be logged in to download mapsets!");
                return null;
            }

            if (OnlineManager.CurrentGame == null)
                return null;

            var download = new MultiplayerSharedMapsetDownload(OnlineManager.CurrentGame.GameId, artist, title,
                CurrentActiveDownloads.Count < MAX_CONCURRENT_DOWNLOADS);

            DownloadAdded?.Invoke(typeof(MapsetDownloadManager), new MapsetDownloadAddedEventArgs(download));

            return download;
        }

        /// <summary>
        /// </summary>
        public static void OpenOnlineHub()
        {
            var game = (QuaverGame)GameBase.Game;

            game.OnlineHub.SelectSection(OnlineHubSectionType.ActiveDownloads);

            if (game.OnlineHub.IsOpen || game.CurrentScreen.Type == QuaverScreenType.Download)
                return;

            DialogManager.Show(new OnlineHubDialog());
        }
    }
}