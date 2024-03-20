namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

public interface ITriggerPayload
{
    void Trigger(int exactTime);
    void Undo(int exactTime);
}