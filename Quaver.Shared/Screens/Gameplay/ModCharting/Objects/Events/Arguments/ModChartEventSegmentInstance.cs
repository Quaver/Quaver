using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;

[MoonSharpUserData]
public record ModChartEventAddSegmentInstance(Segment Segment)
    : ModChartEventInstance(ModChartEventType.TimelineAddSegment);

[MoonSharpUserData]
public record ModChartEventRemoveSegmentInstance(Segment Segment)
    : ModChartEventInstance(ModChartEventType.TimelineRemoveSegment);

[MoonSharpUserData]
public record ModChartEventUpdateSegmentInstance(Segment Segment)
    : ModChartEventInstance(ModChartEventType.TimelineUpdateSegment);