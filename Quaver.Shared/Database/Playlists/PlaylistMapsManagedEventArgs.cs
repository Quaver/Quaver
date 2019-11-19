using System;

namespace Quaver.Shared.Database.Playlists
{
    public class PlaylistMapsManagedEventArgs : EventArgs
    {
        public Playlist Playlist { get; }

        public PlaylistMapsManagedEventArgs(Playlist playlist) => Playlist = playlist;
    }
}