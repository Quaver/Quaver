using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

[MoonSharpUserData]
public class Keyframe<T>
{
    public readonly double Time;
    public readonly T Value;
    public readonly EasingDelegate EasingFunction;

    public Keyframe(double time, T value, EasingDelegate easingFunction)
    {
        Time = time;
        Value = value;
        EasingFunction = easingFunction;
    }

    public float GetProgress(Keyframe<T> nextKeyframe, double currentTime)
    {
        return EasingFunction((float)((currentTime - Time) / (nextKeyframe.Time - Time)));
    }

    private sealed class TimeRelationalComparer : IComparer<Keyframe<T>>
    {
        public int Compare(Keyframe<T> x, Keyframe<T> y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.Time.CompareTo(y.Time);
        }
    }

    public static IComparer<Keyframe<T>> TimeComparer { get; } = new TimeRelationalComparer();
}