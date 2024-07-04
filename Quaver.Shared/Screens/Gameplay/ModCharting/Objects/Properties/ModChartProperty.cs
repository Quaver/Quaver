using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public abstract class ModChartProperty<T>
{
    protected readonly Action<T> Setter;
    protected readonly Func<T> Getter;

    protected ModChartProperty(Func<T> getter, Action<T> setter)
    {
        Getter = getter;
        Setter = setter;
    }

    protected ModChartProperty(Func<T> getter)
    {
        Getter = getter;
        Setter = _ => { };
    }

    public T Value
    {
        get => Getter();
        set => Setter(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract T Add(T left, T right);

    protected abstract SetterDelegate<T> SetterDelegate { get; }

    public TweenPayload<T> Tween(T start, T end) => Tween(start, end, EasingWrapperFunctions.Linear);

    /// <summary>
    ///     Tweens the property from <see cref="start"/> to <see cref="end"/>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenPayload<T> Tween(T start, T end, EasingDelegate easingDelegate) => new()
    {
        StartValue = start,
        EndValue = end,
        EasingFunction = easingDelegate,
        Setter = SetterDelegate
    };

    public TweenPayload<T> Tween(T end) => Tween(Getter(), end, EasingWrapperFunctions.Linear);

    /// <summary>
    ///     Tweens the property from current value to <see cref="end"/> 
    /// </summary>
    /// <param name="end"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenPayload<T> Tween(T end, EasingDelegate easingDelegate) => Tween(Getter(), end, easingDelegate);

    public TweenPayload<T> TweenAdd(T increment) => TweenAdd(increment, EasingWrapperFunctions.Linear);

    /// <summary>
    ///     Tweens the property from current value to current value + <see cref="increment"/>
    /// </summary>
    /// <param name="increment"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenPayload<T> TweenAdd(T increment, EasingDelegate easingDelegate)
    {
        var startValue = Getter();
        return Tween(startValue, Add(startValue, increment), easingDelegate);
    }

    /// <summary>
    ///     Generates a keyframes payload
    /// </summary>
    /// <param name="keyframes"></param>
    /// <returns></returns>
    public KeyframesPayload<T> Keyframes(Keyframe<T>[] keyframes) => new(SetterDelegate, keyframes);
}