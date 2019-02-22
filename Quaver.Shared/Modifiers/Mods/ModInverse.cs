using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModInverse : IGameplayModifier
    {
        public string Name { get; set; } = "Inverse";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Inverse;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Converts regular notes into long notes and long notes into gaps.";

        public bool Ranked { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.NoLongNotes, ModIdentifier.FullLN };

        public void InitializeMod() {}
    }
}