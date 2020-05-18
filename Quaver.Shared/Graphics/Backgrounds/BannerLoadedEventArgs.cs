using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public class BannerLoadedEventArgs : EventArgs
    {
        public Mapset Mapset { get; }

        public Playlist Playlist { get; }

        public Texture2D Banner { get; }

        public BannerLoadedEventArgs(Mapset mapset, Texture2D banner)
        {
            Mapset = mapset;
            Banner = banner;
        }

        public BannerLoadedEventArgs(Playlist playlist, Texture2D banner)
        {
            Playlist = playlist;
            Banner = Banner;
        }
    }
}