using System;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets
{
    public class RandomMapsetSelectedEventArgs : EventArgs
    {
        public Mapset Mapset { get; }

        public int Index { get; }

        public RandomMapsetSelectedEventArgs(Mapset mapset, int index)
        {
            Mapset = mapset;
            Index = index;
        }
    }
}