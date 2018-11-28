using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers
{
    public interface IGameplayModifier
    {
        /// <summary>
        ///     The name of the Mod
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     The identifier of the the gameplayModifier.
        /// </summary>
        ModIdentifier ModIdentifier { get; set; }

        /// <summary>
        ///     The type of gameplayModifier as defined in the enum
        /// </summary>
        ModType Type { get; set; }

        /// <summary>
        ///     The description of the Mod
        /// </summary>
        string Description { get; set; }

        /// <summary>
        ///     Is the gameplayModifier ranked?
        /// </summary>
        bool Ranked { get; set; }

        /// <summary>
        ///     The addition to the score multiplier this gameplayModifier will have
        /// </summary>
        float ScoreMultiplierAddition { get; set; }

        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        ModIdentifier[] IncompatibleMods { get; set; }

        /// <summary>
        ///     All the gameplayModifier logic should go here.
        /// </summary>
        void InitializeMod();
    }
}
