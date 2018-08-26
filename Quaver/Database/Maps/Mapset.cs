using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        ///     The last selected/preferred map in this set
        /// </summary>
        public Map PreferredMap { get; set; }

        public string Artist => Maps.First().Artist;
        public string Title => Maps.First().Title;
        public string Creator => Maps.First().Creator;
        public string Background => MapManager.GetBackgroundPath(Maps.First());
    }
}