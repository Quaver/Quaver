using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class LuaCustomTriggerPayload: ITriggerPayload
{
    public Script Script { get; set; }
    public Closure TriggerClosure { get; set; }
    public Closure UndoClosure { get; set; }

    public LuaCustomTriggerPayload(Script script, Closure triggerClosure, Closure undoClosure)
    {
        Script = script;
        TriggerClosure = triggerClosure;
        UndoClosure = undoClosure;
    }

    public void Trigger(int exactTime)
    {
        if (TriggerClosure == null) return;
        Script.Call(TriggerClosure, exactTime);
    }

    public void Undo(int exactTime)
    {
        if (UndoClosure == null) return;
        Script.Call(UndoClosure, exactTime);
    }
}