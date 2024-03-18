using System;
using MoonSharp.Interpreter;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class LuaCustomSegmentPayload : ISegmentPayload
{
    public Closure Updater;
    public Script Script;

    public LuaCustomSegmentPayload(Closure updater, Script script)
    {
        Updater = updater;
        Script = script;
    }

    public void Update(float curTime)
    {
        Script.Call(Updater, curTime);
    }
}