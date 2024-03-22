using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

public class LuaCustomTriggerPayload: ITriggerPayload
{
    public Closure TriggerClosure { get; set; }
    public Closure UndoClosure { get; set; }

    public LuaCustomTriggerPayload(Closure triggerClosure, Closure undoClosure)
    {
        TriggerClosure = triggerClosure;
        UndoClosure = undoClosure;
    }

    public void Trigger(int exactTime, ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerClosure?.Call(exactTime, valueVertex);
    }

    public void Undo(int exactTime, ValueVertex<ITriggerPayload> valueVertex)
    {
        UndoClosure?.Call(exactTime, valueVertex);
    }
}