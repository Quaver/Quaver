using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.src.Gameplay
{
    class NoteDevianceData
    {
        /// <summary>
        ///     Records how off the note pressed is from receptor
        /// </summary>
        internal float Offset { get; set; }

        /// <summary>
        ///     Records note's song position
        /// </summary>
        internal float Position { get; set; }

        /// <summary>
        ///     What type of color/judgement the note is. (Perf/Great/ect.)
        /// </summary>
        internal int Type { get; set; }
    }
}
