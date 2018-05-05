using System;
using System.Collections.Generic;

namespace Quaver.States.Gameplay
{
    internal class HitObjectPool
    {
        /// <summary>
        ///     All of the objects in the pool.
        /// </summary>
        internal List<HitObject> Objects { get; set; }

        /// <summary>
        ///     The amount of objects in the pool.
        /// </summary>
        internal int Size { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="size"></param>
        internal HitObjectPool(int size)
        {
            Size = size;
            Objects = new List<HitObject>();
        }
    }
}