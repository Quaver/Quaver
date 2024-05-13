/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client.Events.Download;
using Quaver.Server.Client.Helpers;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Download
{
    public class MapsetDownload : IDisposable
    {
        /// <summary>
        /// </summary>
        public bool HasDownloadEverStarted { get; protected set; }

        /// <summary>
        ///     Json containing the mapset.
        /// </summary>
        public JToken Mapset { get; }

        /// <summary>
        /// </summary>
        public int MapsetId { get; }

        /// <summary>
        /// </summary>
        public string Artist { get; }

        /// <summary>
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// </summary>
        public Bindable<FileDownloader> FileDownloader { get; } = new(null);

        /// <summary>
        /// </summary>
        public Bindable<DownloadProgressEventArgs> Progress { get; } = new(null);

        /// <summary>
        /// </summary>
        public Bindable<DownloadStatusChangedEventArgs> Status { get; } = new(null);

        public event EventHandler Removed;

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        public MapsetDownload(JToken mapset, string artist, string title, bool download = true)
        {
            Mapset = mapset;
            MapsetId = (int)Mapset["id"];

            Artist = artist;
            Title = title;

            if (download)
                Download();
        }

        public MapsetDownload(int id, string artist, string title, bool download = true)
        {
            MapsetId = id;
            Artist = artist;
            Title = title;

            if (download)
                Download();
        }

        public virtual void Download()
        {
            if (HasDownloadEverStarted)
                return;

            HasDownloadEverStarted = true;

            Logger.Important($"Downloading mapset {MapsetId}...", LogType.Network);

            var dir = $"{ConfigManager.DataDirectory.Value}/Downloads";
            var path = $"{dir}/{MapsetId}.qp";
            Directory.CreateDirectory(dir);

            try
            {
                FileDownloader.Value = OnlineManager.Client?.DownloadMapset(path, MapsetId);
                if (FileDownloader.Value == null)
                {
                    RemoveDownload();
                    return;
                }

                FileDownloader.Value.DownloadProgressChanged += (o, e) => Progress.Value = e;
                FileDownloader.Value.StatusUpdated += (o, e) =>
                {
                    if (e.CancelledOrComplete)
                    {
                        Logger.Important(
                            $"Finished downloading mapset: {MapsetId}. Cancelled: {e.Cancelled} | Error: {e.Error}",
                            LogType.Network);
                        if (e.Status == FileDownloaderStatus.Complete)
                            MapsetImporter.Queue.Add(path);
                        MapsetDownloadManager.CurrentActiveDownloads.Remove(this);
                    }
                    else if (e.Status == FileDownloaderStatus.Connecting)
                    {
                        MapsetDownloadManager.CurrentActiveDownloads.Add(this);
                    }

                    Status.Value = e;
                };
                FileDownloader.Value.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error,
                    $"There was an error downloading mapset: {Artist} - {Title} ({MapsetId})");
            }
        }

        public void RemoveDownload()
        {
            Logger.Debug($"Removing download", LogType.Runtime);
            FileDownloader.Value?.Cancel();
            Removed?.Invoke(this, EventArgs.Empty);
            MapsetDownloadManager.CurrentDownloads.Remove(this);
            Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Progress?.Dispose();
            Status?.Dispose();
        }
    }
}