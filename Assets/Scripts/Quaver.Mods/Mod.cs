using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
    public class Mod
    {
        /// <summary>
        /// The name of the mod.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of mod
        /// </summary>
        public ModType Type;

        /// <summary>
        /// The unique indentifier of the mod
        /// </summary>
        public ModIdentifier ModIdentifier;

        /// <summary>
        /// A description of the mod.
        /// </summary>
        public string Description;

        /// <summary>
        /// The score multiplier the mod gives.
        /// </summary>
        public float ScoreMultiplier;

        /// <summary>
        /// If the mod triggers score submission or not.
        /// </summary>
        public bool Ranked;

        /// <summary>
        /// A list of incompatible mods that can't be used with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods;

        /// <summary>
        /// If you are allowed to fail the map when using this mod.
        /// </summary>
        public bool FailureAllowed;
    }
}
