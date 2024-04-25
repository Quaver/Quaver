using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartActionManager
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;

    public ModChartActionManager(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    /// <summary>
    ///     Keeps calling the updater between the specified time range
    /// </summary>
    /// <param name="id">Unique ID for the segment. Leave -1 for automatic ID generation</param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="updater"></param>
    /// <param name="isDynamic">Whether to delete the segment when finished</param>
    /// <returns>The ID for the segment. -1 if there's already a segment with the id specified</returns>
    public int AddCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = Shortcut.GameplayScreenView.SegmentManager.GenerateNextId();
        return Shortcut.GameplayScreenView.SegmentManager.Add(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater), isDynamic))
            ? id
            : -1;
    }

    /// <summary>
    ///     Calls the trigger function when reaching a time, or calls the undo function to revert
    /// </summary>
    /// <param name="id">Unique ID for the segment. Leave -1 for automatic ID generation</param>
    /// <param name="time"></param>
    /// <param name="trigger"></param>
    /// <param name="undoTrigger"></param>
    /// <param name="isDynamic">Whether to delete the trigger when finished</param>
    /// <returns>The ID for the trigger. -1 if there's already a trigger with the id specified</returns>
    public int SetCustomTrigger(int id, int time, Closure trigger, Closure undoTrigger = null,
        bool isDynamic = false)
    {
        if (id == -1) id = Shortcut.GameplayScreenView.TriggerManager.GenerateNextId();
        return Shortcut.GameplayScreenView.TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = id,
            Payload = new LuaCustomTriggerPayload(trigger, undoTrigger),
            IsDynamic = isDynamic,
            Time = time
        })
            ? id
            : -1;
    }

    public int SetIntervalTrigger(int id, int time, float interval, int count, Closure trigger,
        Closure undoTrigger = null)
    {
        if (id == -1) id = Shortcut.GameplayScreenView.TriggerManager.GenerateNextId();
        return Shortcut.GameplayScreenView.TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = id,
            Payload = new IntervalTriggerPayload(Shortcut.GameplayScreenView.TriggerManager,
                id,
                time,
                interval,
                count,
                v => trigger.Call(v),
                v => undoTrigger?.Call(v)),
            IsDynamic = false,
            Time = time
        })
            ? id
            : -1;
    }

    /// <summary>
    ///     Adds the segment. If there is already a segment with the ID, removes the segment to add the new one
    /// </summary>
    /// <param name="id"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="updater"></param>
    /// <param name="isDynamic"></param>
    /// <seealso cref="AddCustomSegment"/>
    /// <returns></returns>
    public int SetCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = Shortcut.GameplayScreenView.SegmentManager.GenerateNextId();
        return Shortcut.GameplayScreenView.SegmentManager.UpdateSegment(
            new Segment(id, startTime, endTime,
                new LuaCustomSegmentPayload(updater), isDynamic))
            ? id
            : 0;
    }

    /// <summary>
    ///     Adds a tween segment that allows smooth transition of a value
    /// </summary>
    /// <param name="id">ID of the segment</param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="setter">A function f(time: float, progress: float) called for updating the value. progress is [0..1] or -1 if weird things happen</param>
    /// <param name="easingFunction">A function f(startValue: float, endValue: float, progress: float) that returns the value at progress</param>
    /// <param name="isDynamic"></param>
    /// <returns></returns>
    /// <seealso cref="Easing"/>
    public int SetTweenSegment(int id,
        int startTime, int endTime,
        float startValue, float endValue,
        TweenPayload.SetterDelegate setter,
        TweenPayload.EasingDelegate easingFunction = null,
        bool isDynamic = false)
    {
        if (id == -1) id = Shortcut.GameplayScreenView.SegmentManager.GenerateNextId();
        return Shortcut.GameplayScreenView.SegmentManager.UpdateSegment(
            new Segment(id, startTime, endTime,
                new TweenPayload
                {
                    EasingFunction = easingFunction ?? EasingWrapperFunctions.Linear,
                    StartValue = startValue,
                    EndValue = endValue,
                    Setter = setter
                }, isDynamic))
            ? id
            : -1;
    }

    /// <summary>
    ///     Adds a tween segment that allows smooth transition of a value
    /// </summary>
    /// <param name="id">ID of the segment</param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="setter">A function f(time: float, progress: float) called for updating the value. progress is [0..1] or -1 if weird things happen</param>
    /// <param name="easing">Easing type</param>
    /// <param name="isDynamic"></param>
    /// <returns></returns>
    /// <seealso cref="Easing"/>
    public int SetTweenSegment(int id,
        int startTime, int endTime,
        float startValue, float endValue,
        TweenPayload.SetterDelegate setter,
        Easing easing,
        bool isDynamic = false)
    {
        return SetTweenSegment(id, startTime, endTime, startValue, endValue, setter,
            EasingWrapperFunctions.FromEasing(easing), isDynamic);
    }


    /// <summary>
    ///     Generates a new trigger ID
    /// </summary>
    /// <returns></returns>
    public int GenerateTriggerId() => Shortcut.GameplayScreenView.TriggerManager.GenerateNextId();

    /// <summary>
    ///     Generates a new segment ID
    /// </summary>
    /// <returns></returns>
    public int GenerateSegmentId() => Shortcut.GameplayScreenView.SegmentManager.GenerateNextId();

    /// <summary>
    ///     Width of lane (receptor alone)
    /// </summary>
    /// <returns></returns>
    public float LaneSize => Shortcut.GameplayPlayfieldKeys.LaneSize;

    /// <summary>
    ///     Padding of receptor
    /// </summary>
    /// <returns></returns>
    public float ReceptorPadding => Shortcut.GameplayPlayfieldKeys.ReceptorPadding;

    /// <summary>
    ///     Separation between lanes
    /// </summary>
    /// <returns></returns>
    public float LaneSeparationWidth => LaneSize + ReceptorPadding;

    /// <summary>
    ///     Positions of each receptor
    /// </summary>
    /// <returns>Scalable vector (x, y, scale_x, scale_y) for each receptor</returns>
    public ScalableVector2[] GetReceptorPositions()
    {
        var positions = new ScalableVector2[Shortcut.GameplayScreen.Map.GetKeyCount()];
        for (var i = 0; i < Shortcut.GameplayScreen.Map.GetKeyCount(); i++)
        {
            positions[i] = Shortcut.GameplayPlayfieldKeysStage.Receptors[i].Position;
        }

        return positions;
    }

    /// <summary>
    ///     Sets the position of a receptor of a particular lane
    /// </summary>
    /// <param name="lane"></param>
    /// <param name="pos"></param>
    public void SetReceptorPosition(int lane, ScalableVector2 pos)
    {
        Shortcut.GameplayPlayfieldKeysStage.Receptors[lane - 1].Position = pos;
    }

    /// <summary>
    ///     Spits a debug message
    /// </summary>
    /// <param name="str"></param>
    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}