using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

[MoonSharpUserData]
public class LuaCustomSegmentPayload : ISegmentPayload
{
    public Closure Updater { get; set; }

    public LuaCustomSegmentPayload(Closure updater)
    {
        Updater = updater;
    }

    public void Update(float curTime, float progress)
    {
        Updater.Call(curTime, progress);
    }
}