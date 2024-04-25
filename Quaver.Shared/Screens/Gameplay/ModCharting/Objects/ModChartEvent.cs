using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartEvent
{
    protected readonly HashSet<Closure> Closures = new();

    public void Add(Closure closure) => Closures.Add(closure);
    public void Remove(Closure closure) => Closures.Remove(closure);

    public void Invoke(params object[] p)
    {
        foreach (var closure in Closures)
        {
            closure.Call(p);
        }
    }
}