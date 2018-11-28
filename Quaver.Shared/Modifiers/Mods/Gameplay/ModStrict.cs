using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods.Gameplay
{
    /// <summary>
    ///     ManiaModStrict Mod. Makes the hit timing windows harder during gameplay.
    /// </summary>
    internal class ManiaModStrict : IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = "ModStrict";

        /// <inheritdoc />
        /// <summary>
        ///     ID
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Strict;

        /// <inheritdoc />
        /// <summary>
        ///     ResultsScreenType
        /// </summary>
        public ModType Type { get; set; } = ModType.DifficultyIncrease;

        /// <inheritdoc />
        /// <summary>
        ///     Desc
        /// </summary>
        public string Description { get; set; } = "You'll need to be super accurate.";

        /// <inheritdoc />
        /// <summary>
        ///     Ranked?
        /// </summary>
        public bool Ranked { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        ///     Score +x
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0.1f;

        /// <inheritdoc />
        /// <summary>
        ///     Incompatible Mods
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Chill };

        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
        }
    }
}
