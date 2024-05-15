using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

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
        Updater.SafeCall(progress, segment);
    }

    public override string ToString()
    {
        return "Custom";
    }
}