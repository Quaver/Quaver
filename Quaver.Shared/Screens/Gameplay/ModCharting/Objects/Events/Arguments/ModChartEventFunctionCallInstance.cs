using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventFunctionCallInstance(Closure Closure, params object[] Arguments)
    : ModChartEventInstance(ModChartEventType.FunctionCall);