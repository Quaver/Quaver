using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public class BannerLoadedEventArgs : EventArgs
    {
        public Mapset Mapset { get; }

        public Texture2D Banner { get; }

        public BannerLoadedEventArgs(Mapset mapset, Texture2D banner)
        {
            Mapset = mapset;
            Banner = banner;
        }
    }
}