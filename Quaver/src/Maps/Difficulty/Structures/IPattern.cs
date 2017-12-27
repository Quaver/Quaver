using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Structures
{
    internal interface IPattern
    {
        /// <summary>
        ///     The total time this pattern takes
        /// </summary>
        int TotalTime { get; set; }

        /// <summary>
        ///     The list of hitobjects in this pattern
        /// </summary>
        List<HitObjectInfo> HitObjects { get; set; }
    }
}
