using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;

namespace Quaver.Screens.Select.UI.Selector
{
    public class BackgroundLoadedEventArgs : EventArgs
    {
        public Map Map { get; }
        public int Index { get; }
        public Texture2D Texture { get; }

        public BackgroundLoadedEventArgs(Map map, int mapsetIndex, Texture2D tex)
        {
            Map = map;
            Index = mapsetIndex;
            Texture = tex;
        }
    }
}