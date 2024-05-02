namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

public record ModChartEventCustomInstance(ModChartEventType EventType, params object[] Arguments)
    : ModChartEventInstance(EventType);