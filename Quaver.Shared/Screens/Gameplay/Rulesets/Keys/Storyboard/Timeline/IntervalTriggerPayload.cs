using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

[MoonSharpUserData]
public class IntervalTriggerPayload : ITriggerPayload
{
    [MoonSharpHidden]
    public IntervalTriggerPayload(TriggerManager triggerManager, int occupyId, int startTime, float interval,
        int triggerCount = int.MaxValue,
        ITriggerPayload.TriggerDelegate onTrigger = default, ITriggerPayload.TriggerDelegate onUndo = default)
    {
        TriggerManager = triggerManager;
        OccupyId = occupyId;
        StartTime = startTime;
        Interval = interval;
        TriggerCount = triggerCount;
        OnTrigger = onTrigger;
        OnUndo = onUndo;
    }

    private TriggerManager TriggerManager { get; }
    public int OccupyId { get; }
    public int StartTime { get; }
    public float Interval { get; }
    public int TriggerCount { get; }

    public ITriggerPayload.TriggerDelegate OnTrigger { get; }
    public ITriggerPayload.TriggerDelegate OnUndo { get; }

    public int CurrentTriggerCount { get; private set; }

    [MoonSharpHidden]
    public void Trigger(int exactTime, ValueVertex<ITriggerPayload> valueVertex)
    {
        CurrentTriggerCount++;
        if (CurrentTriggerCount >= TriggerCount) return;
        OnTrigger?.Invoke(exactTime, valueVertex);
        UpdateTrigger();
    }

    [MoonSharpHidden]
    public void Undo(int exactTime, ValueVertex<ITriggerPayload> valueVertex)
    {
        CurrentTriggerCount--;
        if (CurrentTriggerCount < 0) return;
        OnUndo?.Invoke(exactTime, valueVertex);
        UpdateTrigger();
    }

    private void UpdateTrigger()
    {
        TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = OccupyId,
            IsDynamic = false,
            Payload = this,
            Time = (int)(StartTime + Interval * CurrentTriggerCount)
        });
    }
}