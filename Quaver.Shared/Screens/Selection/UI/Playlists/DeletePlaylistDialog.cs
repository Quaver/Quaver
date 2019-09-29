using System;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Playlists
{
    public class DeletePlaylistDialog : YesNoDialog
    {
        public DeletePlaylistDialog(Playlist playlist) : base("Delete Playlist",
            "Are you sure you would like to delete this playlist?", () =>
            {
                PlaylistManager.DeletePlaylist(playlist);
            })
        {
        }
    }
}