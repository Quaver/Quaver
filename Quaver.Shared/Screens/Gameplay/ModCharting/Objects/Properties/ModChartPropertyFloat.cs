using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyFloat : ModChartProperty<float>
{
    public ModChartPropertyFloat(Func<float> getter, Action<float> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyFloat(Func<float> getter) : base(getter)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Add(float left, float right) => left + right;

    public override float Dot(float left, float right) => left * right;

    public override float Normalise(float left) => left;

    public override float Negative(float left) => -left;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Multiply(float left, float right) => left * right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float RandomUnit() => RandomHelper.RandomUniform() * 2 - 1;

    public override LerpDelegate<float> Lerp => EasingFunctions.Linear;
}