using System.Numerics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartTimeline
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;

    public ModChartTimeline(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }

    /// <summary>
    ///     Calls the trigger function when reaching a time, or calls the undo function to revert
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="undoTrigger"></param>
    public LuaCustomTriggerPayload Trigger(Closure trigger, Closure undoTrigger = null) =>
        new(trigger, undoTrigger);


    /// <summary>
    ///     Keeps calling the updater between the specified time range
    /// </summary>
    /// <param name="updater"></param>
    public LuaCustomSegmentPayload Segment(Closure updater) => new(updater);

    public IntervalTriggerPayload IntervalTrigger(int id, int time, float interval, int count, Closure trigger,
        Closure undoTrigger = null) => new(Shortcut.ModChartScript.TriggerManager,
        id,
        time,
        interval,
        count,
        v => trigger.SafeCall(v),
        v => undoTrigger?.SafeCall(v));

    /// <summary>
    ///     Adds a tween segment that allows smooth transition of a value
    /// </summary>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="setter">A function f(time: float, progress: float) called for updating the value. progress is [0..1] or -1 if weird things happen</param>
    /// <param name="easingFunction">A function f(startValue: float, endValue: float, progress: float) that returns the value at progress</param>
    /// <returns></returns>
    /// <seealso cref="Easing"/>
    public TweenPayload<float> Tween(float startValue, float endValue, TweenPayload<float>.SetterDelegate setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector2> Tween(Vector2 startValue, Vector2 endValue,
        TweenPayload<Vector2>.SetterDelegate setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector3> Tween(Vector3 startValue, Vector3 endValue,
        TweenPayload<Vector3>.SetterDelegate setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector4> Tween(Vector4 startValue, Vector4 endValue,
        TweenPayload<Vector4>.SetterDelegate setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public int Add(int id, int startTime, int endTime, ISegmentPayload payload, bool isDynamic = false)
    {
        if (id == -1) id = GenerateSegmentId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddSegment, new Segment(id, startTime, endTime, payload, isDynamic));
        return id;
    }
    public bool RemoveSegment(int id)
    {
        if (!Shortcut.ModChartScript.SegmentManager.TryGetSegment(id, out var segment)) return false;
        if (segment.MarkedToRemove) return false;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineRemoveSegment, segment);
        segment.MarkedToRemove = true;
        return true;
    }
    public int Update(int id, int startTime, int endTime, ISegmentPayload payload, bool isDynamic = false)
    {
        if (id == -1) id = Shortcut.ModChartScript.SegmentManager.GenerateNextId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineUpdateSegment, new Segment(id, startTime, endTime, payload, isDynamic));
        return id;
    }

    public int Add(int id, int time, ITriggerPayload payload, bool isDynamic = false)
    {
        if (id == -1) id = GenerateTriggerId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddTrigger, new ValueVertex<ITriggerPayload>()
        {
            Id = id,
            Time = time,
            Payload = payload,
            IsDynamic = isDynamic
        });
        return id;
    }
    public bool RemoveTrigger(int id)
    {
        if (!Shortcut.ModChartScript.TriggerManager.TryGetVertex(id, out var vertex)) return false;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineRemoveTrigger, vertex);
        return true;
    }
    public int Update(int id, int time, ITriggerPayload payload, bool isDynamic = false)
    {
        if (id == -1) id = GenerateTriggerId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineUpdateTrigger, new ValueVertex<ITriggerPayload>()
        {
            Id = id,
            Time = time,
            Payload = payload,
            IsDynamic = isDynamic
        });
        return id;
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


    /// <summary>
    ///     Spits a debug message
    /// </summary>
    /// <param name="str"></param>
    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}