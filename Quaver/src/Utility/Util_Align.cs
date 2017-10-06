using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace Quaver.Utility
{
    internal static partial class Util
    {
        /// <summary>
        /// Returns a 1-dimensional value for an object's alignment within the provided boundary.
        /// </summary>
        /// <param name="Scale">The value (percentage) which the object will be aligned to (0=min, 0.5 =mid, 1.0 = max)</param>
        /// <param name="ObjectSize">The size of the object</param>
        /// <param name="Boundary"></param>
        /// <returns></returns>
        public static float Align(float Scale, float ObjectSize, Vector2 Boundary, float Offset = 0)
        {
            float BoundaryMin;
            float BoundaryMax;
            //Sets the boundary min/max
            if (Boundary.X < Boundary.Y)
            {
                BoundaryMin = Boundary.X;
                BoundaryMax = Boundary.Y;
            }
            else
            {
                BoundaryMin = Boundary.Y;
                BoundaryMax = Boundary.X;
            }

            //The alignment (Also used as a temporary boundary size value)
            float alignment = BoundaryMax - BoundaryMin;

            //If the object size is bigger than the boundary for some reason
            if (ObjectSize > alignment)
            {
                alignment = (Scale * alignment) + BoundaryMin;
            }

            //If the object size is within the boundary
            else
            {
                alignment = ((alignment - ObjectSize) * Scale) + BoundaryMin;
            }

            return alignment + Offset;
        }
    }
}
