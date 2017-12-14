using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    class NoteRecord
    {
        /// <summary>
        ///     Records how off the note pressed is from receptor
        /// </summary>
        internal double Offset { get; set; }

        /// <summary>
        ///     Records note's song position
        /// </summary>
        internal double Position { get; set; }

        /// <summary>
        ///     What type of color/judgement the note is. (Perf/Great/ect.)
        /// </summary>
        internal int Type { get; set; }
    }
}
