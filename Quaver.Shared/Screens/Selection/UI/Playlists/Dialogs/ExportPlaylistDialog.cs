using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class ExportPlaylistDialog : LoadingDialog
    {
        public ExportPlaylistDialog(Playlist playlist) : base("Export Playlist",
            "Please wait while your playlist is being exported...", () => playlist.Export())
        {
        }
    }
}