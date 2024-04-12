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

    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerClosure?.Call(valueVertex);
    }

    public void Undo(ValueVertex<ITriggerPayload> valueVertex)
    {
        UndoClosure?.Call(valueVertex);
    }
}