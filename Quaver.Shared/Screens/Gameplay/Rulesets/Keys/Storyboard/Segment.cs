using System;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class Segment<T> where T : ISegmentPayload
{
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public T Payload { get; set; }

    protected bool Equals(Segment<T> other)
    {
        return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) && EqualityComparer<T>.Default.Equals(Payload, other.Payload);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Segment<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartTime, EndTime, Payload);
    }

    public (ValueVertex<T> Start, ValueVertex<T> End) CreateVertexPair()
    {
        return (new ValueVertex<T>
            {
                Payload = Payload,
                Time = StartTime
            },
            new ValueVertex<T>
            {
                Payload = Payload,
                Time = EndTime
            });
    }
}