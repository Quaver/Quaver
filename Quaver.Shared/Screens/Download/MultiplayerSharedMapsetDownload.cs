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
    }
}