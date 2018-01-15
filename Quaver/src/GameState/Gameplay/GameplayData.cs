using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    struct GameplayData
    {
        /// <summary>
        ///     Value of Data
        /// </summary>
        internal double Value { get; set; }

        /// <summary>
        ///     Records note's song position
        /// </summary>
        internal double Position { get; set; }
    }
}
