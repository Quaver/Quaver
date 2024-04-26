using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

[MoonSharpUserData]
public class ModChartEventInstance
{
    public ModChartEventType Type { get; private set; }
    public object[] Arguments { get; private set; }

    public ModChartEventInstance(ModChartEventType type, object[] arguments)
    {
        Type = type;
        Arguments = arguments;
    }

    /// <summary>
    ///     Invokes the event immediately
    /// </summary>
    /// <param name="modChartEvents"></param>
    public void Dispatch(ModChartEvents modChartEvents)
    {
        modChartEvents.Invoke(Type, Arguments);
    }
}