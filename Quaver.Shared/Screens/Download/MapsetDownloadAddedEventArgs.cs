using System;

namespace Quaver.Shared.Screens.Download
{
    public class MapsetDownloadAddedEventArgs : EventArgs
    {
        public MapsetDownload Download { get; }

        public MapsetDownloadAddedEventArgs(MapsetDownload download) => Download = download;
    }
}