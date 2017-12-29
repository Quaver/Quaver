using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    class BarObject
    {
        /// <summary>
        ///     Where the bar is in relation with time
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        ///     The bar's object from the receptor
        /// </summary>
        public ulong OffsetFromReceptor { get; set; }
    }
}
