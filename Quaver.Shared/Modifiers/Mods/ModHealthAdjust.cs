using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModLongNoteAdjust : IGameplayModifier
    {
        public string Name { get; set; } = "Health Adjustments";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.HeatlthAdjust;

        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        public string Description { get; set; } = $"Test the new health system. This is a work in progress!\n\n" +
                                                  $"Goals:\n" +
                                                  $"* Low 'B' and 'C' Ranks should now be possible. \n" +
                                                  $"* The game should be more suitable for low level players.\n" +
                                                  $"* Playing the game incorrectly should now result in extremely low accuracy (<=89%) and not immediate fails.\n\n" +
                                                  $"Health Weighting Changes:\n" +
                                                  $"- Between difficulty levels 1-20, HP will scale and progressively get harder.\n" +
                                                  $"- Any difficulty level 20 or above (Aprox. Insane+) uses the normal HP system.";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = false;

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } = new ModIdentifier[] {};

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2C94C");

        public void InitializeMod()
        {
        }
    }
}