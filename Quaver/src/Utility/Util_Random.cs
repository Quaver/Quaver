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
        internal static Random random = new Random();
        /// <summary>
        /// Generates A random float between 2 numbers.
        /// </summary>
        /// <param name="Min"></param>
        /// <param name="Max"></param>
        /// <returns></returns>
        public static float Random(float Min, float Max)
        {
            //If Min > Max for some reason
            if (Min > Max)
            {
                float temp = Min;
                Max = Min;
                Min = temp;
            }

            //Generate the random number
            float randNum = (float)random.Next(0, 1000)/1000f;

            //Return the random number in the given range
            return (randNum * (Max - Min)) + Min;
        }
    }
}
