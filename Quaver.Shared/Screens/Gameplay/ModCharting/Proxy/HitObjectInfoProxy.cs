using MoonSharp.Interpreter;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class HitObjectInfoProxy
{
    private readonly HitObjectInfo _info;

    [MoonSharpHidden]
    public HitObjectInfoProxy(HitObjectInfo info)
    {
        _info = info;
    }

    public int StartTime => _info.StartTime;
    public int EndTime => _info.EndTime;
    public int Lane => _info.Lane;
    public int EditorLayer => _info.EditorLayer;
    public HitSounds HitSound => _info.HitSound;
    public bool IsLongNote => _info.IsLongNote;
}