using System.Collections.Generic;

namespace Quaver.Database.Maps
{
    internal class Mapset
    {
        /// <summary>
        ///     The directory of the mapset.
        /// </summary>
        internal string Directory { get; set; }

        /// <summary>
        ///     The list of maps in this mapset.
        /// </summary>
        internal List<Map> Maps { get; set; }
    }
}
