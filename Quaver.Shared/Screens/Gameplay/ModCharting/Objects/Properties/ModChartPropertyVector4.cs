using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector4 : ModChartProperty<Vector4>
{
    public ModChartPropertyVector4(Func<Vector4> getter, Action<Vector4> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector4(Func<Vector4> getter) : base(getter)
    {
    }

    protected override Vector4 Add(Vector4 left, Vector4 right)
    {
        return left + right;
    }

    protected override SetterDelegate<Vector4> SetterDelegate => TweenSetters.CreateVector4(Setter);
}