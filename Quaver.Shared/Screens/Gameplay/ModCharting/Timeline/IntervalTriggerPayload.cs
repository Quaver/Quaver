using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

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
        UndoTriggerId = triggerManager.GenerateNextId();
        IntervalUndoTriggerPayload = new IntervalUndoTriggerPayload(TriggerManager, this);
    }

    private TriggerManager TriggerManager { get; }
    public int OccupyId { get; }
    public int StartTime { get; }
    public float Interval { get; }
    public int TriggerCount { get; }
    public int UndoTriggerId { get; }
    private IntervalUndoTriggerPayload IntervalUndoTriggerPayload { get; set; }

    public ITriggerPayload.TriggerDelegate OnTrigger { get; }
    public ITriggerPayload.TriggerDelegate OnUndo { get; }

    public int CurrentTriggerCount { get; internal set; }

    [MoonSharpHidden]
    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        CurrentTriggerCount++;
        if (CurrentTriggerCount >= TriggerCount) return;
        OnTrigger?.Invoke(valueVertex);
        UpdateTrigger(valueVertex);
    }

    [MoonSharpHidden]
    public void Undo(ValueVertex<ITriggerPayload> valueVertex)
    {
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
        TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = UndoTriggerId,
            IsDynamic = false,
            Payload = IntervalUndoTriggerPayload,
            Time = (int)(StartTime + Interval * (CurrentTriggerCount - 1))
        }, false);
    }
}

class IntervalUndoTriggerPayload : ITriggerPayload
{

    private TriggerManager TriggerManager { get; }
    private IntervalTriggerPayload IntervalTriggerPayload { get; }

    public IntervalUndoTriggerPayload(TriggerManager triggerManager, IntervalTriggerPayload intervalTriggerPayload)
    {
        TriggerManager = triggerManager;
        IntervalTriggerPayload = intervalTriggerPayload;
    }

    public void Trigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        
    }

    public void Undo(ValueVertex<ITriggerPayload> valueVertex)
    {
        IntervalTriggerPayload.CurrentTriggerCount--;
        if (IntervalTriggerPayload.CurrentTriggerCount < 0) return;
        IntervalTriggerPayload.OnUndo?.Invoke(valueVertex);
        UpdateTrigger(valueVertex);
    }
    private void UpdateTrigger(ValueVertex<ITriggerPayload> valueVertex)
    {
        TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = IntervalTriggerPayload.OccupyId,
            IsDynamic = false,
            Payload = IntervalTriggerPayload,
            Time = (int)(IntervalTriggerPayload.StartTime + IntervalTriggerPayload.Interval * IntervalTriggerPayload.CurrentTriggerCount)
        });
        TriggerManager.UpdateVertex(new ValueVertex<ITriggerPayload>
        {
            Id = valueVertex.Id,
            IsDynamic = false,
            Payload = this,
            Time = (int)(IntervalTriggerPayload.StartTime + IntervalTriggerPayload.Interval * (IntervalTriggerPayload.CurrentTriggerCount - 1))
        }, false);
    }
}