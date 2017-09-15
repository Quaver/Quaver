// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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