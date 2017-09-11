using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quaver.Qua
{

    public static class DifficultyCalculator
    {
        public static Difficulty CalculateDifficulty(List<HitObject> ObjectList)
        {
            Difficulty newDifficulty = new Difficulty();
            newDifficulty.StarDifficulty = 12.23f;

            return newDifficulty;
        }

    }
}