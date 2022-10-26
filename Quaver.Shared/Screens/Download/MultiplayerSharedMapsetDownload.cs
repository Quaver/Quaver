using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Download
{
    public class MultiplayerSharedMapsetDownload : MapsetDownload
    {
        public MultiplayerSharedMapsetDownload(JToken mapset, string artist, string title, bool download = true) : base(mapset, artist, title, download)
        {
        }

        public MultiplayerSharedMapsetDownload(int id, string artist, string title, bool download = true) : base(id, artist, title, download)
        {
        }

        /// <summary>
        ///     Allows the user to download the mapset if shared in multiplayer
        /// </summary>
        public override void Download()
        {
            if (IsDownloading)
                return;

            IsDownloading = true;

            Logger.Important($"Downloading mapset {MapsetId}...", LogType.Network);

            var dir = $"{ConfigManager.DataDirectory.Value}/Downloads";
            var path = $"{dir}/{MapsetId}.qp";
            Directory.CreateDirectory(dir);

            try
            {
                OnlineManager.Client?.DownloadSharedMultiplayerMap(path, (o, e) => Progress.Value = e, (o, e) =>
                {
                    Logger.Important($"Finished downloading mapset: {MapsetId}. Cancelled: {e.Cancelled} | Error: {e.Error}", LogType.Network);
                    MapsetImporter.Queue.Add(path);

                    Completed.Value = e;
                    MapsetDownloadManager.CurrentDownloads.Remove(this);
                    Dispose();
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, $"There was an error downloading mapset: {Artist} - {Title} ({MapsetId})");
            }
        }
    }
}