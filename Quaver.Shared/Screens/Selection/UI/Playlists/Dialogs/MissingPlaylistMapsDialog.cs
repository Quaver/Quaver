using System;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class MissingPlaylistMapsDialog : YesNoDialog
    {
        public MissingPlaylistMapsDialog(Playlist playlist, int missingMaps) : base("Upload Playlist",
            $"You are missing {missingMaps} map(s) from the online map pool. Remove them?", () =>
            {
                DialogManager.Show(new UploadPlaylistDialog(playlist, false, true));
            }, () =>
            {
                DialogManager.Show(new UploadPlaylistDialog(playlist, false, false));
            })
        {
        }
    }
}