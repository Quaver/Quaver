using System;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class ValueVertex<T> : IEqualityComparer<ValueVertex<T>>
{
    public float Time { get; set; }
    public T Payload { get; set; }

    private sealed class TimeRelationalComparer : IComparer<ValueVertex<T>>
    {
        public int Compare(ValueVertex<T> x, ValueVertex<T> y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.Time.CompareTo(y.Time);
        }
    }
    

    public static IComparer<ValueVertex<T>> TimeComparer { get; } = new TimeRelationalComparer();

    public bool Equals(ValueVertex<T> x, ValueVertex<T> y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Time.Equals(y.Time) && EqualityComparer<T>.Default.Equals(x.Payload, y.Payload);
    }

    public int GetHashCode(ValueVertex<T> obj)
    {
        return HashCode.Combine(obj.Time, obj.Payload);
    }
}