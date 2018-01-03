using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics
{
    internal class DrawRectangle
    {
        /// <summary>
        ///     Position X
        /// </summary>
        internal float X { get; set; }

        /// <summary>
        ///     Position Y
        /// </summary>
        internal float Y { get; set; }

        /// <summary>
        ///     Width Size
        /// </summary>
        internal float Width { get; set; }

        /// <summary>
        ///     Height Size
        /// </summary>
        internal float Height { get; set; }

        internal DrawRectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
