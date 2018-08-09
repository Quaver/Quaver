using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;

namespace Quaver.Screens.Select.UI.Selector
{
    public class BackgroundLoadedEventArgs : EventArgs
    {
        public Mapset Set { get; }
        public int Index { get; }
        public Texture2D Texture { get; }

        public BackgroundLoadedEventArgs(Mapset set, int mapsetIndex, Texture2D tex)
        {
            Set = set;
            Index = mapsetIndex;
            Texture = tex;
        }
    }
}