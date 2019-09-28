using System;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.Selection.UI.Maps
{
    public class MapDeletedEventArgs : EventArgs
    {
        public Map Map { get; }

        public int Index { get; }

        public MapDeletedEventArgs(Map map, int index)
        {
            Map = map;
            Index = index;
        }
    }
}