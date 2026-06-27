using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModNoMines : IGameplayModifier
    {
        public string Name { get; set; } = "No Mines";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoMines;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Removes mines from the map.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = true;

        public bool ChangesMapObjects { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { };

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2994A");

        public void InitializeMod() { }
    }
}
