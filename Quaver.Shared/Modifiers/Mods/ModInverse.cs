using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModInverse : IGameplayModifier
    {
        public string Name { get; set; } = "Inverse";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Inverse;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Converts regular notes into long notes and long notes into gaps.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = true;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = true;

        public bool ChangesMapObjects { get; set; } = true;

        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.NoLongNotes, ModIdentifier.FullLN };

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2994A");

        public void InitializeMod() {}
    }
}