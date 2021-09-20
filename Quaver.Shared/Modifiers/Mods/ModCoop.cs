using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModCoop : IGameplayModifier
    {
        public string Name { get; set; } = "Co-op";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Coop;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Grab a friend, and play together. You do have friends... right?";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = false;

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public Color ModColor { get; } = ColorHelper.HexToColor("#44d6f5");

        public void InitializeMod()
        {
        }
    }
}