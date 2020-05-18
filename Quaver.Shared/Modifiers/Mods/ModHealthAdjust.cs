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
                                                  $"Marv: +0.5% -> +0.65%\n" +
                                                  $"Perf: +0.4% -> +0.45%\n" +
                                                  $"Great: +0.2% -> +0.3%\n" +
                                                  $"Good: -0.3% -> +0.1%\n" +
                                                  $"Okay: -4.5% -> -2.5%\n" +
                                                  $"Miss: -6.0% -> -5%\n\n" +
                                                  $"Long Note Changes:\n" +
                                                  $"* Failing to release a long note now results in a 'Good' judgement rather than an 'Okay.'\n" +
                                                  $"* 'Okay' judgements are no longer possible on long note releases. Releasing in the 'Okay' window results in a 'Good.'\n\n" +
                                                  $"Developer Notes:\n" +
                                                  $"* You can use theater mode to view multiple replays at the same time if you would like to compare!\n";

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