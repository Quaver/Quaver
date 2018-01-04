using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics
{
    /// <summary>
    ///     Universal Dimension Value used for UI and other scalars
    /// </summary>
    internal class UDim
    {
        /// <summary>
        ///     Scale of this value
        /// </summary>
        internal float Scale { get; set; }

        /// <summary>
        ///     Offset of this value
        /// </summary>
        internal float Offset { get; set; }

        internal UDim(float offset = 0, float scale = 0)
        {
            Offset = offset;
            Scale = scale;
        }
    }
}
