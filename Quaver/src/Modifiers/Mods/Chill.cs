using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods
{
    /// <summary>
    ///     Chill mod. Makes the hit timing windows 
    /// </summary>
    internal class Chill : IMod
    {
        public string Name { get; set; } = "Chill";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Chill;

        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        public string Description { get; set; } = "Make it easier on yourself.";

        public bool Ranked { get; set; } = true;

        public float ScoreMultiplierAddition { get; set; } = -0.5f;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Strict };

        public void InitializeMod()
        {
            GameBase.ScoreMultiplier += ScoreMultiplierAddition;
        }
    }
}
