using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Patterns
{
    internal struct PatternList
    {
        /// <summary>
        ///     Jack Patterns
        /// </summary>
        internal List<JackPatternInfo> Jacks { get; set; }

        /// <summary>
        ///     Vibro Patterns
        /// </summary>
        internal List<JackPatternInfo> Vibro { get; set; }

        /// <summary>
        ///     Chord Patterns
        /// </summary>
        internal List<ChordPatternInfo> Chords { get; set; }

        /// <summary>
        ///     Stream patterns
        /// </summary>
        internal List<StreamPatternInfo> Streams { get; set; }
    }
}
