using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
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