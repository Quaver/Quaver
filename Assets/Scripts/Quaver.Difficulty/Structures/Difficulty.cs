// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

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