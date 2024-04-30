using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartEventInstance
{
    public ModChartEventType Type { get; private set; }
    public ModChartEventArgs Args { get; private set; }

    public ModChartEventInstance(ModChartEventType type, ModChartEventArgs args)
    {
        Type = type;
        Args = args;
    }

    /// <summary>
    ///     Invokes the event immediately
    /// </summary>
    /// <param name="modChartEvents"></param>
    public void Dispatch(ModChartEvents modChartEvents)
    {
        modChartEvents.Invoke(Type, Args);
    }
}