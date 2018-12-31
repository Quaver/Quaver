using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModNoLongNotes : IGameplayModifier
    {
        public string Name { get; set; } = "No Long Notes";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoLongNotes;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "I have a variety of taste preferences, but noodles aren't included.";

        public bool Ranked { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public void InitializeMod() {}
    }
}