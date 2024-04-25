namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

public interface ITriggerPayload
{
    public delegate void TriggerDelegate(ValueVertex<ITriggerPayload> valueVertex);
    void Trigger(ValueVertex<ITriggerPayload> valueVertex);
    void Undo(ValueVertex<ITriggerPayload> valueVertex);
}