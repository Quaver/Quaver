using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModLongNoteAdjust : IGameplayModifier
    {
        public string Name { get; set; } = "Long Note Adjustments";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.LongNoteAdjust;

        public ModType Type { get; set; } = ModType.DifficultyDecrease;

        public string Description { get; set; } = $"Test the new long note release system. Timing windows are slightly easier.\n\n" +
                                                  $"Long note release judgements that drain hp punish 30% less than usual.\n\n" +
                                                  $"Marv: 2.0x\n" +
                                                  $"Perf: 1.8x\n" +
                                                  $"Great: 1.7x\n" +
                                                  $"Good: 1.5x\n" +
                                                  $"Okay: 1.5x";

        public bool Ranked() => false;

        public bool AllowedInMultiplayer { get; set; } = false;

        public bool OnlyMultiplayerHostCanCanChange { get; set; } = false;

        public ModIdentifier[] IncompatibleMods { get; set; } = new ModIdentifier[] {};

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2C94C");

        public void InitializeMod()
        {
        }
    }
}