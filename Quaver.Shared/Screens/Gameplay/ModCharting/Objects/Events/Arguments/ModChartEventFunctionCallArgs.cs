using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public class ModChartEventFunctionCallArgs : ModChartEventArgs
{
    public Closure Closure { get; }
    public object[] Arguments { get; }

    public ModChartEventFunctionCallArgs(Closure closure, object[] arguments)
    {
        Closure = closure;
        Arguments = arguments;
    }
}