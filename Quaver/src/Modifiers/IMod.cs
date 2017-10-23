using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Modifiers
{
    internal interface IMod
    {
        /// <summary>
        ///     The name of the Mod
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     The identifier of the the mod.
        /// </summary>
        ModIdentifier ModIdentifier { get; set; }

        /// <summary>
        ///     The type of mod as defined in the enum
        /// </summary>
        ModType Type { get; set; }

        /// <summary>
        ///     The description of the Mod
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     Is the mod ranked?
        /// </summary>
        bool Ranked { get; set; }

        /// <summary>
        ///     The addition to the score multiplier this mod will have
        /// </summary>
        float ScoreMultiplierAddition { get; set; }

        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        ModIdentifier[] IncompatibleMods { get; set; }

        /// <summary>
        ///     The speed aleration rate the game will be set to.
        ///     If the mod doesnt require a speed alteration, it should be set to 1.0f;
        /// </summary>
        float SpeedAlterationRate { get; set; }

        /// <summary>
        ///     All the mod logic should go here.
        /// </summary>
        void InitializeMod();
    }
}
