// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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