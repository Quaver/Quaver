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
        /// This method is used for animation/tweening.
        /// </summary>
        /// <param name="Target">The target value.</param>
        /// <param name="Current">The current value.</param>
        /// <param name="Scale">Make sure this value is between 0 and 1.</param>
        /// <returns></returns>
        public static float Tween(float Target, float Current, double Scale)
        {
            return (float)(Current + ((Target-Current)*Scale));
        }
    }
}
