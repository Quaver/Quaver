using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class TimingPointInfoProxy
{
    private readonly TimingPointInfo _info;

    [MoonSharpHidden]
    public TimingPointInfoProxy(TimingPointInfo info)
    {
        _info = info;
    }

    public float StartTime => _info.StartTime;
    public float Bpm => _info.Bpm;
    public int Signature => (int)_info.Signature;
}