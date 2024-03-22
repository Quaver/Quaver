using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

[MoonSharpUserData]
public class LuaCustomSegmentPayload : ISegmentPayload
{
    public Closure Updater { get; }

    public LuaCustomSegmentPayload(Closure updater)
    {
        Updater = updater;
    }

    [MoonSharpHidden]
    public void Update(float progress, Segment segment)
    {
        Updater.Call(progress, segment);
    }
}