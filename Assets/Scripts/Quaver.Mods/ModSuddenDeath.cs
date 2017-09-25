using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
    public class ModSuddenDeath : Mod
    {
        /// <summary>
        /// Sudden Death Mod.
        /// The user will fail the beatmap if they miss once on the map.
        /// </summary>
        public ModSuddenDeath()
        {
            Name = "Sudden Death";
            Type = ModType.Special;
            ModIdentifier = ModIdentifier.NoFail;
            Description = "Miss once... and you die.";
            ScoreMultiplier = 0.0f;
            IncompatibleMods = new ModIdentifier[] { ModIdentifier.NoFail };
            Ranked = true;
            FailureAllowed = true;
        }
    }
}