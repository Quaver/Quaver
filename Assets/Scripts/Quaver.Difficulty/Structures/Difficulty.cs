// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Difficulty
{
    public struct Difficulty
    {
        public int[] npsInterval;
        public float AverageNPS;

        public float StarDifficulty;
        public float StaminaStrain;
        public float JackStrain;
        public float SpeedStrain;
        public float ControlStrain;
        public float TechStrain;
    }
}