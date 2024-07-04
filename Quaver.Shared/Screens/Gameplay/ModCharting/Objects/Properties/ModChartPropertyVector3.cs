using System;
using System.Numerics;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;

[MoonSharpUserData]
public class ModChartPropertyVector3 : ModChartProperty<Vector3>
{
    public ModChartPropertyVector3(Func<Vector3> getter, Action<Vector3> setter) : base(getter, setter)
    {
    }

    public ModChartPropertyVector3(Func<Vector3> getter) : base(getter)
    {
    }

    protected override Vector3 Add(Vector3 left, Vector3 right)
    {
        return left + right;
    }

    protected override SetterDelegate<Vector3> SetterDelegate => TweenSetters.CreateVector3(Setter);
}