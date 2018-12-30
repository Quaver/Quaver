/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Download
{
    public class MapsetDownload : IDisposable
    {
        /// <summary>
        ///     Json containing the mapset.
        /// </summary>
        public JToken Mapset { get; }

        /// <summary>
        /// </summary>
        public int MapsetId => (int) Mapset["id"];

        /// <summary>
        /// </summary>
        public Bindable<DownloadProgressChangedEventArgs> Progress { get; } = new Bindable<DownloadProgressChangedEventArgs>(null);

        /// <summary>
        /// </summary>
        public Bindable<AsyncCompletedEventArgs> Completed { get; } = new Bindable<AsyncCompletedEventArgs>(null);

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public MapsetDownload(JToken mapset)
        {
            Mapset = mapset;

            Logger.Important($"Downloading mapset {MapsetId}...", LogType.Network);

            var dir = $"{ConfigManager.DataDirectory.Value}/Downloads";
            var path = $"{dir}/{MapsetId - TimeHelper.GetUnixTimestampMilliseconds()}.qp";
            Directory.CreateDirectory(dir);

            OnlineManager.Client?.DownloadMapset(path, MapsetId, (o, e) => Progress.Value = e, (o, e) =>
            {
                Logger.Important($"Finished downloading mapset: {MapsetId}. Cancelled: {e.Cancelled} | Error: {e.Error}", LogType.Network);
                MapsetImporter.Queue.Add(path);

                Completed.Value = e;
                MapsetDownloadManager.CurrentDownloads.Remove(this);
                Dispose();
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Progress?.Dispose();
            Completed?.Dispose();
        }
    }
}