using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public abstract class ModChartProperty<T> : ModChartGeneralProperty<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract T Add(T left, T right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract T Multiply(T left, float right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract T RandomUnit();

    public abstract LerpDelegate<T> Lerp { get; }

    /// <summary>
    ///     Tweens the property from <see cref="start"/> to <see cref="end"/>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public ISegmentPayload Tween(T start, T end, EasingDelegate easingDelegate = null) => new TweenPayload<T>(this)
    {
        StartValue = start,
        EndValue = end,
        EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear
    };

    /// <summary>
    ///     Tweens the property from current value to <see cref="end"/> 
    /// </summary>
    /// <param name="end"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenTowardsPayload<T> Tween(T end, EasingDelegate easingDelegate = null) => new(this)
    {
        EndValue = end,
        EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear
    };

    /// <summary>
    ///     Tweens the property from current value to current value + <see cref="increment"/>
    /// </summary>
    /// <param name="increment"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenAddPayload<T> TweenAdd(T increment, EasingDelegate easingDelegate = null) => new(this, increment)
    {
        EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear
    };

    /// <summary>
    ///     Tween the two properties towards each other.
    ///     This property will be tweened towards <see cref="other"/> and <see cref="other"/> will be tweened towards this.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="easingDelegate"></param>
    /// <returns></returns>
    public TweenSwapPayload<T> TweenSwap(ModChartProperty<T> other, EasingDelegate easingDelegate) => new(this, other)
    {
        EasingFunction = easingDelegate ?? EasingWrapperFunctions.Linear
    };

    #region Vibrate

    /// <summary>
    /// </summary>
    /// <param name="strength"></param>
    /// <returns></returns>
    public ISegmentPayload VibrateCirc(ModChartPropertyFloat strength) =>
        new TweenIdlePayload<T>(this).WithVibrateCirc(strength);

    /// <summary>
    /// </summary>
    /// <param name="strength"></param>
    /// <returns></returns>
    public ISegmentPayload VibrateCirc(float strength) =>
        new TweenIdlePayload<T>(this).WithVibrateCirc(strength);

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public ISegmentPayload VibrateRandom(T direction) =>
        new TweenIdlePayload<T>(this).WithVibrateRandom(direction);

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public ISegmentPayload VibrateRandom(ModChartProperty<T> direction) =>
        new TweenIdlePayload<T>(this).WithVibrateRandom(direction);

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cycles"></param>
    /// <returns></returns>
    public ISegmentPayload Vibrate(T direction, int cycles) =>
        new TweenIdlePayload<T>(this).WithVibrate(direction, cycles);

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cycles"></param>
    /// <returns></returns>
    public ISegmentPayload Vibrate(ModChartGeneralProperty<T> direction, int cycles) =>
        new TweenIdlePayload<T>(this).WithVibrate(direction, cycles);

    #endregion


    /// <summary>
    ///     Generates a keyframes payload
    /// </summary>
    /// <param name="keyframes"></param>
    /// <returns></returns>
    public KeyframesPayload<T> Keyframes(Keyframe<T>[] keyframes) => new(this, keyframes);

    protected ModChartProperty(Func<T> getter, Action<T> setter) : base(getter, setter)
    {
    }

    protected ModChartProperty(Func<T> getter) : base(getter)
    {
    }
}