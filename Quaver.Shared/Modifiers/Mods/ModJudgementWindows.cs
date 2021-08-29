using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Shared.Helpers;

namespace Quaver.Shared.Modifiers.Mods
{
    public class ModJudgementWindows : IGameplayModifier
    {
        public string Name { get; set; } = "Judgement Windows";

        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.None;

        public ModType Type { get; set; } = ModType.Special;

        public string Description { get; set; } = "Fully customize the timing windows.";

        public bool Ranked() => true;

        public bool AllowedInMultiplayer { get; set; }

        public bool OnlyMultiplayerHostCanCanChange { get; set; }

        public bool ChangesMapObjects { get; set; }

        public ModIdentifier[] IncompatibleMods { get; set; }

        public Color ModColor { get; } = ColorHelper.HexToColor("#F2C94C");

        public void InitializeMod() => throw new System.NotImplementedException();
    }
}