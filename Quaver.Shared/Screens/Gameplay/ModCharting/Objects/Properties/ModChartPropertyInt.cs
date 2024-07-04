using System;
using System.Runtime.CompilerServices;
using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

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
    protected override int Add(int left, int right)
    {
        return left + right;
    }

    protected override SetterDelegate<int> SetterDelegate => TweenSetters.CreateInt(Setter);
}