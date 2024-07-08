using System;
using System.Numerics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Logging;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartTimeline : ModChartGlobalVariable
{
    public ModChartTimeline(ElementAccessShortcut shortcut) : base(shortcut)
    {
    }

    public IntervalTriggerPayload IntervalTrigger(int id, int time, float interval, int count, Closure trigger,
        Closure undoTrigger = null) => new(Shortcut.ModChartScript.TriggerManager,
        id,
        time,
        interval,
        count,
        v => trigger.SafeCall(v));

    public Segment Add(int startTime, int endTime, ISegmentPayload payload)
    {
        var segment = new Segment(GenerateSegmentId(), startTime, endTime, payload);
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddSegment, segment);
        return segment;
    }

    public bool Remove(Segment segment)
    {
        if (segment.Id == -1)
            return false;
        if (segment.MarkedToRemove) return false;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineRemoveSegment, segment);
        segment.MarkedToRemove = true;
        return true;
    }

    public bool RemoveSegment(int id)
    {
        return Shortcut.ModChartScript.SegmentManager.TryGetSegment(id, out var segment) && Remove(segment);
    }

    public ValueVertex<ITriggerPayload> Add(int time, ITriggerPayload payload)
    {
        var trigger = new ValueVertex<ITriggerPayload>
        {
            Id = GenerateTriggerId(),
            Payload = payload,
            Time = time
        };
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddTrigger, trigger);
        return trigger;
    }

    public bool Remove(ValueVertex<ITriggerPayload> trigger)
    {
        if (trigger.Id == -1) return false;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineRemoveTrigger, trigger);
        return true;
    }

    public bool RemoveTrigger(int id)
    {
        return Shortcut.ModChartScript.TriggerManager.TryGetVertex(id, out var vertex) && Remove(vertex);
    }

    /// <summary>
    ///     Generates a new trigger ID
    /// </summary>
    /// <returns></returns>
    public int GenerateTriggerId() => Shortcut.ModChartScript.TriggerManager.GenerateNextId();

    /// <summary>
    ///     Generates a new segment ID
    /// </summary>
    /// <returns></returns>
    public int GenerateSegmentId() => Shortcut.ModChartScript.SegmentManager.GenerateNextId();
}