using System;
using System.Threading;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Dialogs
{
    public class UploadPlaylistDialog : LoadingDialog
    {
        public UploadPlaylistDialog(Playlist playlist, bool checkMissingMaps, bool removeMissingMaps) : base("Upload Playlist",
            "Please wait while your playlist is being uploaded...", () =>
            {
                if (checkMissingMaps)
                {
                    var missing = playlist.GetMissingMapPoolMaps();

                    // User is missing maps in the pool, so ask them if they want to remove the missing ones.
                    if (missing.Count != 0)
                    {
                        DialogManager.Show(new MissingPlaylistMapsDialog(playlist, missing.Count));
                        Logger.Important($"Missing {missing.Count} maps in playlist: {playlist.Name} prior to uploading.", LogType.Runtime);
                        return;
                    }
                }

                if (playlist.OnlineMapPoolId == -1)
                    PlaylistManager.UploadPlaylist(playlist);
                else
                    PlaylistManager.UpdatePlaylist(playlist, removeMissingMaps);

                NotificationManager.Show(NotificationLevel.Success, "Successfully uploaded playlist as an online map pool");
            })
        {
        }
    }
}