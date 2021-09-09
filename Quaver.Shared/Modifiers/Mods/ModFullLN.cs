using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModFullLN : IGameplayModifier
    {
        public string Name { get; set; } = "Full Long Notes";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.FullLN;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "So I heard you like long notes.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = true;

        public bool ChangesMapObjects { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.NoLongNotes, ModIdentifier.Inverse };

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2994A");

        public void InitializeMod() {}
    }
}