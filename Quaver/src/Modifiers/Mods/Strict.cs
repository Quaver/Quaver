using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Modifiers.Mods
{
    /// <summary>
    ///     Strict Mod. Makes the hit timing windows harder during gameplay.
    /// </summary>
    internal class Strict : IMod
    {
        public string Name { get; set; } = "Strict";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Strict;

        public ModType Type { get; set; } = ModType.DifficultyIncrease;

        public string Description { get; set; } = "You'll need to be super accurate.";

        public bool Ranked { get; set; } = true;

        public float ScoreMultiplierAddition { get; set; } = 0.1f;

        public ModIdentifier[] IncompatibleMods { get; set; } = {ModIdentifier.Chill};

        public void InitializeMod()
        {
            GameBase.ScoreMultiplier += ScoreMultiplierAddition;
        }
    }
}
