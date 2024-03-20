using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Tween;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardActionManager
{
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }
    [MoonSharpVisible(false)] public GameplayScreen GameplayScreen => GameplayScreenView.Screen;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeys GameplayPlayfieldKeys => (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage => GameplayPlayfieldKeys.Stage;

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
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        return GameplayScreenView.SegmentManager.Add(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic))
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
        if (id == -1) id = GameplayScreenView.TriggerManager.GenerateNextId();
        return GameplayScreenView.TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = id,
            Payload = new LuaCustomTriggerPayload(Script.WorkingScript, trigger, undoTrigger),
            IsDynamic = isDynamic,
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
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        return GameplayScreenView.SegmentManager.UpdateSegment(
            new Segment(id, startTime, endTime,
                new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic))
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
        int startValue, int endValue,
        TweenPayload.SetterDelegate setter,
        TweenPayload.EasingDelegate easingFunction = null,
        bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        return GameplayScreenView.SegmentManager.UpdateSegment(
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
        int startValue, int endValue,
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
    public int GenerateTriggerId() => GameplayScreenView.TriggerManager.GenerateNextId();

    /// <summary>
    ///     Generates a new segment ID
    /// </summary>
    /// <returns></returns>
    public int GenerateSegmentId() => GameplayScreenView.SegmentManager.GenerateNextId();

    /// <summary>
    ///     Width of lane (receptor alone)
    /// </summary>
    /// <returns></returns>
    public float LaneSize => GameplayPlayfieldKeys.LaneSize;

    /// <summary>
    ///     Padding of receptor
    /// </summary>
    /// <returns></returns>
    public float ReceptorPadding => GameplayPlayfieldKeys.ReceptorPadding;

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
        var positions = new ScalableVector2[GameplayScreen.Map.GetKeyCount()];
        for (var i = 0; i < GameplayScreen.Map.GetKeyCount(); i++)
        {
            positions[i] = GameplayPlayfieldKeysStage.Receptors[i].Position;
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
        GameplayPlayfieldKeysStage.Receptors[lane - 1].Position = pos;
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