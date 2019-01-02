/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;

namespace Quaver.Shared.Screens.Download
{
    public static class MapsetDownloadManager
    {
        /// <summary>
        ///     All opf the currently downloading mapsets.
        /// </summary>
        public static List<MapsetDownload> CurrentDownloads { get; } = new List<MapsetDownload>();

        /// <summary>
        ///    The amount of mapsets able to be downloaded at once.
        /// </summary>
        public static int MAX_CONCURRENT_DOWNLOADS { get; } = 5;

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

            if (CurrentDownloads.Count >= MAX_CONCURRENT_DOWNLOADS)
            {
                NotificationManager.Show(NotificationLevel.Error, $"Slow down! You can only download {MAX_CONCURRENT_DOWNLOADS} at a time!");
                return null;
            }

            var download = new MapsetDownload(mapset);
            CurrentDownloads.Add(download);

            return download;
        }
    }
}