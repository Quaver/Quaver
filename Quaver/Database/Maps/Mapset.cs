using System.Collections.Generic;

namespace Quaver.Database.Maps
{
    public class Mapset
    {
        /// <summary>
        ///     The directory of the mapset.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        ///     The list of maps in this mapset.
        /// </summary>
        public List<Map> Maps { get; set; }
    }
}