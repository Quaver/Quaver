using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class StoryboardActionManager
{
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }

    public int AddCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        GameplayScreenView.SegmentManager.Add(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic));
        return id;
    }
    
    public int SetCustomSegment(int id, int startTime, int endTime, Closure updater, bool isDynamic = false)
    {
        if (id == -1) id = GameplayScreenView.SegmentManager.GenerateNextId();
        GameplayScreenView.SegmentManager.UpdateSegment(new Segment(id, startTime, endTime,
            new LuaCustomSegmentPayload(updater, Script.WorkingScript), isDynamic));
        return id;
    }

    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}