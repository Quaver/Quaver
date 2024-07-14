using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyInt : ModChartProperty<int>
{
    public ModChartPropertyInt(Func<int> getter, Action<int> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyInt(Func<int> getter) : base(getter)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int Add(int left, int right) => left + right;

    public override float Dot(int left, int right) => left * right;

    public override int Normalise(int left) => left;

    public override int Negative(int left) => -left;

    public override int Multiply(int left, float right) => (int)(left * right);
    public override int RandomUnit() => RandomHelper.RandomBinary() * 2 - 1;

    public override LerpDelegate<int> Lerp =>
        (start, end, progress) => (int)EasingFunctions.Linear(start, end, progress);
}