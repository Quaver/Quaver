using Quaver.API.Enums;

namespace Quaver.Modifiers.Mods.Gameplay
{
    public class ManiaModPaused : IGameplayModifier
    {
        public string Name { get; set; } = "Paused";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Paused;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Player paused in gameplay";

        public bool Ranked { get; set; } = false;

        public float ScoreMultiplierAddition { get; set; } = 0;

        public ModIdentifier[] IncompatibleMods { get; set; } = {ModIdentifier.NoPause};

        public void InitializeMod() {}
    }
}