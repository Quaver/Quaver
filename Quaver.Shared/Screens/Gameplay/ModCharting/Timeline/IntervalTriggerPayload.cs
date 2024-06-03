using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

[MoonSharpUserData]
public class IntervalTriggerPayload : ITriggerPayload
{
    [MoonSharpHidden]
    public IntervalTriggerPayload(TriggerManager triggerManager, int occupyId, int startTime, float interval,
        int triggerCount = int.MaxValue,
        ITriggerPayload.TriggerDelegate onTrigger = default)
    {
        TriggerManager = triggerManager;
        OccupyId = occupyId;
        StartTime = startTime;
        Interval = interval;
        TriggerCount = triggerCount;
        OnTrigger = onTrigger;
    }

    private TriggerManager TriggerManager { get; }
    public int OccupyId { get; }
    public int StartTime { get; }
    public float Interval { get; }
    public int TriggerCount { get; }

    public ITriggerPayload.TriggerDelegate OnTrigger { get; }

    public int CurrentTriggerCount { get; internal set; }

    [MoonSharpHidden]
    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        CurrentTriggerCount++;
        if (CurrentTriggerCount >= TriggerCount) return;
        OnTrigger?.Invoke(valueVertex);
        UpdateTrigger(valueVertex);
    }

    private void UpdateTrigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = valueVertex.Id,
            IsDynamic = false,
            Payload = this,
            Time = (int)(StartTime + Interval * CurrentTriggerCount)
        });
    }
}