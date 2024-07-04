using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

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
    protected override float Add(float left, float right)
    {
        return left + right;
    }

    protected override SetterDelegate<float> SetterDelegate => TweenSetters.CreateFloat(Setter);
}