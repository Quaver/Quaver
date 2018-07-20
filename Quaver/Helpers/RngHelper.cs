using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Helpers
{
    internal static class RngHelper
    {
        /// <summary>
        /// Generates A random float between 2 numbers.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static float Random(float min, float max)
        {
            var random = new Random();

            // If min > max for some reason
            if (min > max)
            {
                var temp = min;
                max = min;
                min = temp;
            }

            //Generate the random number
            var randNum = random.Next(0, 1000) / 1000f;

            //Return the random number in the given range
            return (randNum * (max - min)) + min;
        }
    }
}
