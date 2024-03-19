using System;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class StoryboardActionManager
{
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }

    public GameplayScreen GameplayScreen => GameplayScreenView.Screen;

    public GameplayPlayfieldKeys GameplayPlayfieldKeys => (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;

    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage => GameplayPlayfieldKeys.Stage;
    
    public int AddCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        return GameplayScreenView.SegmentManager.Add(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic))
            ? id
            : -1;
    }

    public int AddCustomTrigger(int id, int time, Closure trigger, Closure undoTrigger = null,
        bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.TriggerManager.GenerateNextId();
        return GameplayScreenView.TriggerManager.AddVertex(new ValueVertex<ITriggerPayload>
        {
            Id = id,
            Payload = new LuaCustomTriggerPayload(Script.WorkingScript, trigger, undoTrigger),
            IsDynamic = isDynamic,
            Time = time
        })
            ? id
            : -1;
    }

    public int SetCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        GameplayScreenView.SegmentManager.UpdateSegment(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic));
        return id;
    }

    public int GenerateTriggerId() => GameplayScreenView.TriggerManager.GenerateNextId();

    public int GenerateSegmentId() => GameplayScreenView.SegmentManager.GenerateNextId();

    public int GetKeyCount() => GameplayScreen.Map.GetKeyCount();

    public float GetReceptorLaneSize()
    {
        return GameplayPlayfieldKeys.LaneSize;
    }

    public float GetReceptorPadding()
    {
        return GameplayPlayfieldKeys.ReceptorPadding;
    }

    public ScalableVector2[] GetReceptorPositions()
    {
        var positions = new ScalableVector2[GameplayScreen.Map.GetKeyCount()];
        for (var i = 0; i < GameplayScreen.Map.GetKeyCount(); i++)
        {
            positions[i] = GameplayPlayfieldKeysStage.Receptors[i].Position;
        }

        return positions;
    }

    public void SetReceptorPosition(int lane, ScalableVector2 pos)
    {
        GameplayPlayfieldKeysStage.Receptors[lane - 1].Position = pos;
    }

    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}