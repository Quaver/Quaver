using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using static Quaver.Shared.Database.Playlists.Playlist;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class ExportPlaylistDialog : LoadingDialog
    {
        public ExportPlaylistDialog(Playlist playlist, ExportMode exportMode) : base("Export Playlist",
            "Please wait while your playlist is being exported...", () => playlist.Export(exportMode))
        {
        }
    }
}