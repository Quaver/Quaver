using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public class ModChartEventNoteEntryArgs : ModChartEventArgs
{
    public GameplayHitObjectKeysInfo HitObject { get; set; }

    public ModChartEventNoteEntryArgs(GameplayHitObjectKeysInfo hitObject)
    {
        HitObject = hitObject;
    }
}