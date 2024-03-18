using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class StoryboardActionManager
{
    [MoonSharpVisible(false)]
    public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)]
    public StoryboardScript Script { get; set; }

    public void AddCustomSegment(float startTime, float endTime, Closure updater)
    {
        GameplayScreenView.SegmentManager.Add(new Segment
        {
            StartTime = startTime,
            EndTime = endTime,
            Payload = new LuaCustomSegmentPayload(updater, Script.WorkingScript)
        });
    }
    
    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}