using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventAddTriggerInstance(ValueVertex<ITriggerPayload> Vertex, bool Trigger = true)
    : ModChartEventInstance(ModChartEventType.TimelineAddTrigger);

[MoonSharpUserData]
public record ModChartEventRemoveTriggerInstance(ValueVertex<ITriggerPayload> Vertex, bool Trigger = true)
    : ModChartEventInstance(ModChartEventType.TimelineRemoveTrigger);

[MoonSharpUserData]
public record ModChartEventUpdateTriggerInstance(ValueVertex<ITriggerPayload> Vertex, bool Trigger = true)
    : ModChartEventInstance(ModChartEventType.TimelineUpdateTrigger);