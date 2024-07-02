using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventInstance(ModChartEventType EventType)
{
    /// <summary>
    ///     Invokes the event immediately
    /// </summary>
    /// <param name="modChartEvents"></param>
    public void Dispatch(ModChartEvents modChartEvents)
    {
        modChartEvents.Invoke(this);
    }
}