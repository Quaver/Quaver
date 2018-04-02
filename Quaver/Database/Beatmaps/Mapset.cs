using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Database.Beatmaps
{
    internal class Mapset
    {
        /// <summary>
        ///     The directory of the mapset.
        /// </summary>
        internal string Directory { get; set; }

        /// <summary>
        ///     The list of beatmaps in this mapset.
        /// </summary>
        internal List<Beatmap> Beatmaps { get; set; }
    }
}
