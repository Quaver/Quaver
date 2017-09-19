using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
    public class ModSuddenDeath : Mod
    {
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