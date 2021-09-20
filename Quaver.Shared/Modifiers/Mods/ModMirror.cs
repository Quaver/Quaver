using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModMirror : IGameplayModifier
    {
        public string Name { get; set; } = "Mirror";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Mirror;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Flips the map horizontally.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; }

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public Color ModColor { get; } = ColorHelper.HexToColor($"#5F868F");

        public void InitializeMod() {}
    }
}