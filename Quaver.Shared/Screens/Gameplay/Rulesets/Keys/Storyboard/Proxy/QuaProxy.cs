using System.Collections.Generic;
using MoonSharp.Interpreter;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class QuaProxy
{
    private readonly Qua _target;

    [MoonSharpHidden]
    public QuaProxy(Qua target)
    {
        _target = target;
    }

    public int Length => _target.Length;
    public int KeyCount => _target.GetKeyCount();
    public string Title => _target.Title;
    public string Description => _target.Description;
    public bool HasScratchKey => _target.HasScratchKey;
    public List<TimingPointInfo> TimingPoints => _target.TimingPoints;
    public List<HitObjectInfo> HitObjects => _target.HitObjects;
}