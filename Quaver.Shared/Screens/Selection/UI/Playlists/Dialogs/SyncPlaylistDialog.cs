using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class SyncPlaylistDialog : LoadingDialog
    {
        public SyncPlaylistDialog(Playlist playlist) : base("Syncing Map Pool",
            "Please wait while we sync your playlist to its online map pool.", () => PlaylistManager.SyncPlaylistToMapPool(playlist))
        {
        }
    }
}