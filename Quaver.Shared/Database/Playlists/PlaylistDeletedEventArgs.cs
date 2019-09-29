using System;

namespace Quaver.Shared.Database.Playlists
{
    public class PlaylistDeletedEventArgs : EventArgs
    {
        public Playlist Playlist { get; }

        public PlaylistDeletedEventArgs(Playlist p) => Playlist = p;
    }
}