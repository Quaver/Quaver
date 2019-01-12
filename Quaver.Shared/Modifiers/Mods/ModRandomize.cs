using System;
using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    /// <summary>
    ///     Randomize gameplayModifier. Randomizes the lanes.
    /// </summary>
    internal class ModRandomize : IGameplayModifier
    {
        /// <summary>
        ///     Integer based seed for use with the Shuffle(Random) function.
        ///
        ///     Used in shuffling the order of the playfields lanes.
        /// </summary>
        public int Seed;
            
        public string Name { get; set; } = "Randomize";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Randomize;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Swap up the lanes.";

        public bool Ranked { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public void InitializeMod()
        {
        }

        public void GenerateSeed() => Seed = new Random().Next(int.MaxValue);
    }
}