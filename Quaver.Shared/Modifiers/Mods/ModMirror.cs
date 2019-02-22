using Quaver.API.Enums;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModMirror : IGameplayModifier
    {
        public string Name { get; set; } = "Mirror";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Mirror;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Flips the map horizontally.";

        public bool Ranked { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public void InitializeMod() {}
    }
}