using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Mods
{
    public class ModNoFail : Mod
    {
        /// <summary>
        /// The No Fail mod
        /// The user can play the beatmap without failing, even if their health reaches 0.
        /// </summary>
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