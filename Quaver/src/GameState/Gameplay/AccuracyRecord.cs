using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    class AccuracyRecord
    {
        /// <summary>
        ///     Records the player's multiplier percentage
        /// </summary>
        internal double Accuracy { get; set; }

        /// <summary>
        ///     Records note's song position
        /// </summary>
        internal double Position { get; set; }

        /// <summary>
        ///     The current grade of this value
        /// </summary>
        internal int Type { get; set; }
    }
}
