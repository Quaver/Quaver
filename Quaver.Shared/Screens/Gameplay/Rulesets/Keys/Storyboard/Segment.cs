using System;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class Segment
{
    public int Id { get; set; }
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public bool IsDynamic { get; set; }
    public ISegmentPayload Payload { get; set; }
    
    public ValueVertex<ISegmentPayload> StartVertex { get; }
    public ValueVertex<ISegmentPayload> EndVertex { get; }

    public Segment(int id, int startTime, int endTime, ISegmentPayload payload, bool isDynamic = false)
    {
        Id = id;
        StartTime = startTime;
        EndTime = endTime;
        Payload = payload;
        IsDynamic = isDynamic;
        StartVertex = new ValueVertex<ISegmentPayload>
        {
            Payload = Payload,
            Time = StartTime,
            Id = Id,
            IsDynamic = IsDynamic
        };
        EndVertex = new ValueVertex<ISegmentPayload>
        {
            Payload = Payload,
            Time = EndTime,
            Id = Id,
            IsDynamic = IsDynamic
        };
    }

    protected bool Equals(Segment other)
    {
        return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) && EqualityComparer<ISegmentPayload>.Default.Equals(Payload, other.Payload);
    }


    private sealed class IdEqualityComparer : IEqualityComparer<Segment>
    {
        public bool Equals(Segment x, Segment y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Segment obj)
        {
            return obj.Id;
        }
    }

    public static IEqualityComparer<Segment> IdComparer { get; } = new IdEqualityComparer();
}