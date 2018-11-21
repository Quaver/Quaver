using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods.Mania
{
    public class ManiaModAutoplay : IGameplayModifier
    {
        public string Name { get; set; } = "Autoplay";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Autoplay;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Take a break and watch something magical.";

        public bool Ranked { get; set; } = false;

        public float ScoreMultiplierAddition { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.NoPause,
            ModIdentifier.NoFail
        };

        public void InitializeMod()
        {
        }
    }
}