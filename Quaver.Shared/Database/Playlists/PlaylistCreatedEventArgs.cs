using System;

namespace Quaver.Shared.Database.Playlists
{
    public class PlaylistCreatedEventArgs : EventArgs
    {
        public Playlist Playlist { get; }

        public PlaylistCreatedEventArgs(Playlist p) => Playlist = p;
    }
}