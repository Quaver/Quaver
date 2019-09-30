using System;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class UploadPlaylistConfirmationDialog : YesNoDialog
    {
        public UploadPlaylistConfirmationDialog(Playlist playlist) : base("Upload Playlist",
            "Are you sure you would like to upload your playlist online?",
            () => DialogManager.Show(new UploadPlaylistDialog(playlist, true, false)))
        {
        }
    }
}