using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

public abstract class PropertySegmentPayload<T> : ISegmentPayload
{
    /// <summary>
    ///     The property to vary
    /// </summary>
    protected ModChartProperty<T> Property
    {
        get => _property;
        init
        {
            _property = value;
            Lerp ??= value.Lerp;
        }
    }

    /// <summary>
    ///     The modifiers that are supposed to be applied to the property.
    ///     They modify the supposed value of the property to be set.
    ///     The implementor is obligated to use <see cref="Transform"/> to apply the modifiers first.
    /// </summary>
    private readonly List<ModifierDelegate<T>> modifiers = new();

    private readonly ModChartProperty<T> _property;

    protected LerpDelegate<T> Lerp { get; set; }

    public ISegmentPayload Slerp()
    {
        Lerp = Property.Slerp;
        return this;
    }

    public ISegmentPayload Slerp(T origin)
    {
        Lerp = Property.MakeSlerp(origin);
        return this;
    }

    /// <summary>
    ///     Aggregate applying modifiers to the lerped value 
    /// </summary>
    /// <param name="lerpedValue"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    protected T Transform(T lerpedValue, float progress)
    {
        return modifiers.Aggregate(lerpedValue, (current, modifier) => modifier(current, progress));
    }

    /// <summary>
    ///     Adds a modifier to the payload
    /// </summary>
    /// <param name="modifier"></param>
    public void AddModifier(ModifierDelegate<T> modifier) => modifiers.Add(modifier);

    #region Vibration Modifiers

    /// <summary>
    ///     Cycles from value + <see cref="direction"/> to value - <see cref="direction"/> for <see cref="cycles"/> cycles
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cycles"></param>
    /// <returns></returns>
    public ISegmentPayload WithVibrate(T direction, int cycles)
    {
        AddModifier((v, p) =>
        {
            var vibrationProgress = p * cycles % 1;
            return Property.Add(v, Property.Multiply(direction, vibrationProgress));
        });
        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cycles"></param>
    /// <returns></returns>
    public ISegmentPayload WithVibrate(ModChartGeneralProperty<T> direction, int cycles)
    {
        AddModifier((v, p) =>
        {
            var vibrationProgress = p * cycles % 1;
            return Property.Add(v, Property.Multiply(direction.Value, vibrationProgress));
        });
        return this;
    }

    /// <summary>
    ///     Randomly vibrates the value by a value in the range of [-<see cref="direction"/>, +<see cref="direction"/>]
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public ISegmentPayload WithVibrateRandom(T direction)
    {
        AddModifier((v, _) =>
            Property.Add(v, Property.Multiply(direction, RandomHelper.RandomUniform() * 2 - 1)));
        return this;
    }

    public ISegmentPayload WithVibrateRandom(ModChartGeneralProperty<T> direction)
    {
        AddModifier((v, _) =>
            Property.Add(v, Property.Multiply(direction.Value, RandomHelper.RandomUniform() * 2 - 1)));
        return this;
    }

    /// <summary>
    ///     Randomly vibrates the value by <see cref="strength"/> * random unit vector
    /// </summary>
    /// <param name="strength"></param>
    /// <returns></returns>
    public ISegmentPayload WithVibrateCirc(float strength)
    {
        AddModifier((v, _) =>
            Property.Add(v, Property.Multiply(Property.RandomUnit(), strength)));
        return this;
    }

    public ISegmentPayload WithVibrateCirc(ModChartGeneralProperty<float> strength)
    {
        AddModifier((v, _) =>
            Property.Add(v, Property.Multiply(Property.RandomUnit(), strength.Value)));
        return this;
    }

    #endregion

    public abstract void Update(float progress, Segment segment);
}

public delegate T ModifierDelegate<T>(T input, float progress);