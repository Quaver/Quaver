using System;

namespace Quaver.Shared.Database.Maps
{
    public class MapsetDeletedEventArgs : EventArgs
    {
        public Mapset Mapset { get; }

        public int Index { get; }

        public MapsetDeletedEventArgs(Mapset m, int index)
        {
            Mapset = m;
            Index = index;
        }
    }
}