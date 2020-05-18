using System;

namespace Quaver.Shared.Database.Playlists
{
    public class PlaylistSyncedEventArgs : EventArgs
    {
        public Playlist Playlist { get; }

        public PlaylistSyncedEventArgs(Playlist p) => Playlist = p;
    }
}