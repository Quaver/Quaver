namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

public interface ITriggerPayload
{
    public delegate void TriggerDelegate(int exactTime, ValueVertex<ITriggerPayload> valueVertex);
    void Trigger(int exactTime, ValueVertex<ITriggerPayload> valueVertex);
    void Undo(int exactTime, ValueVertex<ITriggerPayload> valueVertex);
}