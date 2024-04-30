using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public class ModChartEventInputKeyArgs : ModChartEventArgs
{
    public GameplayHitObjectKeysInfo HitObject { get; }
    public int Time { get; }
    public Judgement Judgement { get; set; }

    public ModChartEventInputKeyArgs(GameplayHitObjectKeysInfo hitObject, int time, Judgement judgement)
    {
        HitObject = hitObject;
        Time = time;
        Judgement = judgement;
    }
}