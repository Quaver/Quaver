using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods.Mania
{
    /// <summary>
    ///     ManiaGameplayModifierChill gameplayModifier. Makes the hit timing windows 
    /// </summary>
    internal class ManiaGameplayModifierChill : IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = "ManiaGameplayModifierChill";

        /// <inheritdoc />
        /// <summary>
        ///     Identifier
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Chill;

        /// <inheritdoc />
        /// <summary>
        ///     Type
        /// </summary>
        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        /// <inheritdoc />
        /// <summary>
        ///     Desc
        /// </summary>
        public string Description { get; set; } = "Make it easier on yourself.";
        
        /// <inheritdoc />
        /// <summary>
        ///     If gameplayModifier is ranked
        /// </summary>
        public bool Ranked { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        ///     Score x
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = -0.5f;

        /// <inheritdoc />
        /// <summary>
        ///     Incompatible Mods
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Strict };

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
            GameBase.ScoreMultiplier += ScoreMultiplierAddition;
        }
    }
}
