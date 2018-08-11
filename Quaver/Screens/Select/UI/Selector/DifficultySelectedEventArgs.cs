using System;
using Quaver.Database.Maps;

namespace Quaver.Screens.Select.UI.Selector
{
    public class DifficultySelectedEventArgs : EventArgs
    {
        /// <summary>
        ///     The mapset this map belongs to
        /// </summary>
        public Mapset Set { get; }

        /// <summary>
        ///     The index of the mapset.
        /// </summary>
        public int MapsetIndex { get; }

        /// <summary>
        ///     The map that was selected.
        /// </summary>
        public Map Map { get; }

        /// <summary>
        ///     The index of the map in the mapset.
        /// </summary>
        public int MapIndex { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mapsetIndex"></param>
        /// <param name="map"></param>
        /// <param name="set"></param>
        /// <param name="mapIndex"></param>
        public DifficultySelectedEventArgs(Mapset set, int mapsetIndex, Map map, int mapIndex)
        {
            Set = set;
            MapsetIndex = mapsetIndex;
            Map = map;
            MapIndex = mapIndex;
        }
    }
}