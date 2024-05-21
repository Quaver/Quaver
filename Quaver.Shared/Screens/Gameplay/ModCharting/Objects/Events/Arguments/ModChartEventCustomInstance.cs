using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventCustomInstance(ModChartEventType EventType, params object[] Arguments)
    : ModChartEventInstance(EventType);