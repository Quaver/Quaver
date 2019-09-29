using System;
using System.IO;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Selection.UI.Playlists
{
    public class ExportPlaylistDialog : LoadingDialog
    {
        public ExportPlaylistDialog(Playlist playlist) : base("Export Playlist",
            "Please wait while your playlist is being exported...", () => playlist.Export())
        {
        }
    }
}