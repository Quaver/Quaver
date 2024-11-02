using Newtonsoft.Json.Linq;
using Quaver.Shared.Online;

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

        protected override void CreateFileDownloader(string path)
        {
            FileDownloader.Value = OnlineManager.Client?.DownloadSharedMultiplayerMap(path, OnlineManager.CurrentGame?.GameId ?? -1);
        }
    }
}