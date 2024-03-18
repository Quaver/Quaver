using System;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class Segment
{
    public float StartTime { get; set; }
    public float EndTime { get; set; }
    public ISegmentPayload Payload { get; set; }

    protected bool Equals(Segment other)
    {
        return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) && EqualityComparer<ISegmentPayload>.Default.Equals(Payload, other.Payload);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Segment)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartTime, EndTime, Payload);
    }

    public (ValueVertex<ISegmentPayload> Start, ValueVertex<ISegmentPayload> End) CreateVertexPair()
    {
        return (new ValueVertex<ISegmentPayload>
            {
                Payload = Payload,
                Time = StartTime
            },
            new ValueVertex<ISegmentPayload>
            {
                Payload = Payload,
                Time = EndTime
            });
    }
}