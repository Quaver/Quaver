using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods.Gameplay
{
    internal class ManiaModNoSliderVelocities: IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     The name of the gameplayModifier.
        /// </summary>
        public string Name { get; set; } = "No Slider Velocities";

        /// <inheritdoc />
        /// <summary>
        ///     The identifier of the mod.
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoSliderVelocity;

        /// <inheritdoc />
        /// <summary>
        ///     The type of gameplayModifier as defined in the enum
        /// </summary>
        public ModType Type { get; set; } = ModType.Special;

        /// <inheritdoc />
        /// <summary>
        ///     The description of the Mod
        /// </summary>
        public string Description { get; set; } = "Hate scroll speed changes? Say no more.";

        /// <inheritdoc />
        /// <summary>
        ///     Is the gameplayModifier ranked?
        /// </summary>
        public bool Ranked { get; set; } = false;

        /// <inheritdoc />
        /// <summary>
        ///     The addition to the score multiplier this gameplayModifier will have
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        /// <summary>
        ///     The speed alteration rate the game's clock will be set to.
        /// </summary>
        public float SpeedAlterationRate { get; set; } = 1.0f;

        /// <inheritdoc />
        /// <summary>
        ///     All the gameplayModifier logic should go here, setting unique variables. NEVER call this directly. Always use
        ///     ModManager.AddMod();
        /// </summary>
        public void InitializeMod() {}
    }
}
