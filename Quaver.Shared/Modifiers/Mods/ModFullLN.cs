using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModFullLN : IGameplayModifier
    {
        public string Name { get; set; } = "Full LN";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.FullLN;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "So I heard you like long notes.";

        public bool Ranked { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.NoLongNotes, ModIdentifier.Inverse };

        public void InitializeMod() {}
    }
}