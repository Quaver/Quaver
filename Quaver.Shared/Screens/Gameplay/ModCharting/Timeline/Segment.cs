using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

[MoonSharpUserData]
public class Segment
{
    private int _id;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            StartVertex.Id = value;
            EndVertex.Id = value;
        }
    }

    public int StartTime { get; }
    public int EndTime { get; }
    public bool IsDynamic { get; }
    
    [MoonSharpHidden]
    public bool MarkedToRemove { get; set; }
    public ISegmentPayload Payload { get; }

    public ValueVertex<ISegmentPayload> StartVertex { get; }
    public ValueVertex<ISegmentPayload> EndVertex { get; }

    public Segment(int id, int startTime, int endTime, ISegmentPayload payload, bool isDynamic = false)
    {
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
        Id = id;
    }

    protected bool Equals(Segment other)
    {
        return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime) &&
               EqualityComparer<ISegmentPayload>.Default.Equals(Payload, other.Payload);
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

    public float Progress(int curTime) => (float)(curTime - StartTime) / Length;

    public int Length => EndTime - StartTime;

    public override string ToString()
    {
        return $"[Segment {(IsDynamic ? "$" : "")}{Id} {StartTime} - {EndTime} {Payload}]";
    }
}