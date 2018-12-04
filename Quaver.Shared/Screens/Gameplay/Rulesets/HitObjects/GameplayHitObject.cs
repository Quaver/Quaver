using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class GameplayHitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        public HitObjectInfo Info { get; set; }

        /// <summary>
        ///     Destroys the HitObject
        /// </summary>
        public abstract void Destroy();
    }
}