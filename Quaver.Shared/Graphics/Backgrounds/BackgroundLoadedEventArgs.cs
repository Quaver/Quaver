using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Graphics.Backgrounds
{
    public class BackgroundLoadedEventArgs : EventArgs
    {
        public Map Map { get; }
        public Texture2D Texture { get; }

        public BackgroundLoadedEventArgs(Map map, Texture2D tex)
        {
            Map = map;
            Texture = tex;
        }
    }
}