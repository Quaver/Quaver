using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModNoMiss : IGameplayModifier
    {
        public string Name { get; set; } = "No Miss";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoMiss;

        public ModType Type { get; set; } = ModType.DifficultyIncrease;

        public string Description { get; set; } = "You miss, you die.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = false;

        public bool ChangesMapObjects { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = new[]
        {
            ModIdentifier.Autoplay,
            ModIdentifier.NoFail
        };

        public Color ModColor { get; } = ColorHelper.HexToColor("#EB5757");

        public void InitializeMod()
        {
        }
    }
}