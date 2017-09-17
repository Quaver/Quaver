
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
    public class ModNoFail : Mod
    {
        public ModNoFail()
        {
            Name = "No Fail";
            Type = ModType.Special;
            ModIdentifier = ModIdentifier.NoFail;
            Description = "This mod will make you unable to fail maps.";
            ScoreMultiplier = 0.5f;
            IncompatibleMods = new ModIdentifier[] { ModIdentifier.SuddenDeath };
            Ranked = true;
            FailureAllowed = false;
        }
    }
}