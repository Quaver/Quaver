using System;

namespace Quaver.Shared.Database.Maps
{
    public class MapUpdatedEventArgs : EventArgs
    {
        public Map Original { get; }

        public Map Updated { get; }

        public MapUpdatedEventArgs(Map original, Map updated)
        {
            Original = original;
            Updated = updated;
        }
    }
}