using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Replays
{
    /// <summary>
    ///     This is the key press state that will be used in replay frames.
    ///     In a replay frame, it will essentially be the bitwise combination of
    ///     the keys pressed so that it can be easily read.
    /// </summary>
    [Flags]
    internal enum KeyPressState
    {
        K1 = 1 << 0,
        K2 = 1 << 1, 
        K3 = 1 << 2, 
        K4 = 1 << 3,
        K5 = 1 << 4,
        K6 = 1 << 5,
        K7 = 1 << 6
    }
}
