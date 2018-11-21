using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods.Mania
{
    public class ModNoFail : IGameplayModifier
    {
        public string Name { get; set; } = "No Fail";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.NoFail;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Failure is not an option.";

        public bool Ranked { get; set; } = true;

        public float ScoreMultiplierAddition { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.Autoplay,
        };

        public void InitializeMod() {}
    }
}