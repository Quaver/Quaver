using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Main;

namespace Quaver.Modifiers
{
    internal class Speed75X : IMod
    {
        /// <summary>
        ///     The name of the mod.
        /// </summary>
        public string Name { get; set; } = "Speed 0.75x";

        /// <summary>
        ///     The identifier of the mod.
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Speed75X;

        /// <summary>
        ///     The type of mod as defined in the enum
        /// </summary>
        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        /// <summary>
        ///     The description of the Mod
        /// </summary>
        public string Description { get; set; } = "Sets the song speed to 0.75x";

        /// <summary>
        ///     Is the mod ranked?
        /// </summary>
        public bool Ranked { get; set; } = false;

        /// <summary>
        ///     The addition to the score multiplier this mod will have
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0;

        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Speed15X };

        /// <summary>
        ///     All the mod logic should go here, setting unique variables. NEVER call this directly. Always use
        ///     ModManager.AddMod();
        /// </summary>
        public void InitializeMod()
        {
            // Set the GameClock (Audio Speed) to 0.75x here.
            GameBase.GameClock = 0.75f;
        }
    }
}
