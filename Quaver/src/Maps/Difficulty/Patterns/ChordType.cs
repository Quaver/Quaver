using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Maps.Difficulty.Patterns
{
    internal enum ChordType
    {
        Jump, // 2 Note Chord
        Hand, // 3 Note Chord
        Quad, // 4 Note Chord
        FivePlus // 5 Plus note chord (7k, or stacked notes)
    }
}
