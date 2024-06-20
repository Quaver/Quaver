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
public class ModChartTimeline
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;

    public ModChartTimeline(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }

    public IntervalTriggerPayload IntervalTrigger(int id, int time, float interval, int count, Closure trigger,
        Closure undoTrigger = null) => new(Shortcut.ModChartScript.TriggerManager,
        id,
        time,
        interval,
        count,
        v => trigger.SafeCall(v));

    /// <summary>
    ///     Creates a keyframes payload.
    /// </summary>
    /// <param name="setter"></param>
    /// <param name="keyframes"></param>
    /// <returns></returns>
    /// <seealso cref="Keyframe{T}"/>
    public KeyframesPayload<float> Keyframes(SetterDelegate<float> setter, Keyframe<float>[] keyframes) =>
        new(setter, keyframes);

    public KeyframesPayload<Vector2> Keyframes(SetterDelegate<Vector2> setter, Keyframe<Vector2>[] keyframes) =>
        new(setter, keyframes);

    public KeyframesPayload<XnaVector2> Keyframes(SetterDelegate<XnaVector2> setter, Keyframe<XnaVector2>[] keyframes) =>
        new(setter, keyframes);

    public KeyframesPayload<Vector3> Keyframes(SetterDelegate<Vector3> setter, Keyframe<Vector3>[] keyframes) =>
        new(setter, keyframes);

    public KeyframesPayload<Vector4> Keyframes(SetterDelegate<Vector4> setter, Keyframe<Vector4>[] keyframes) =>
        new(setter, keyframes);

    /// <summary>
    ///     Adds a tween segment that allows smooth transition of a value
    /// </summary>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="setter">A function f(time: float, progress: float) called for updating the value. progress is [0..1] or -1 if weird things happen</param>
    /// <param name="easingFunction">A function f(startValue: float, endValue: float, progress: float) that returns the value at progress</param>
    /// <returns></returns>
    /// <seealso cref="Easing"/>
    public TweenPayload<float> Tween(float startValue, float endValue, SetterDelegate<float> setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector2> Tween(Vector2 startValue, Vector2 endValue,
        SetterDelegate<Vector2> setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<XnaVector2> Tween(XnaVector2 startValue, XnaVector2 endValue,
        SetterDelegate<XnaVector2> setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector3> Tween(Vector3 startValue, Vector3 endValue,
        SetterDelegate<Vector3> setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public TweenPayload<Vector4> Tween(Vector4 startValue, Vector4 endValue,
        SetterDelegate<Vector4> setter,
        EasingDelegate easingFunction = null) => new()
    {
        EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
        StartValue = startValue,
        EndValue = endValue,
        Setter = setter
    };

    public static Segment Segment(int startTime, int endTime, ISegmentPayload payload) =>
        new(-1, startTime, endTime, payload);

    public static ValueVertex<ITriggerPayload> Trigger(int time, ITriggerPayload payload) =>
        new()
        {
            Id = -1,
            Payload = payload,
            Time = time
        };

    public int Add(Segment segment)
    {
        if (segment.Id == -1) segment.Id = GenerateSegmentId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddSegment, segment);
        return segment.Id;
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

    public int Set(int id, Segment segment)
    {
        segment.Id = id == -1 ? GenerateSegmentId() : id;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineUpdateSegment, segment);
        return segment.Id;
    }

    public int Add(ValueVertex<ITriggerPayload> trigger)
    {
        if (trigger.Id == -1) trigger.Id = GenerateTriggerId();
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineAddTrigger, trigger);
        return trigger.Id;
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

    public int Set(int id, ValueVertex<ITriggerPayload> trigger)
    {
        trigger.Id = id == -1 ? GenerateTriggerId() : id;
        Shortcut.ModChartEvents.Enqueue(ModChartEventType.TimelineUpdateTrigger, trigger);
        return trigger.Id;
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