using MoonSharp.Interpreter;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class GameplayHitObjectKeysInfoProxy
{
    private readonly GameplayHitObjectKeysInfo _info;

    [MoonSharpHidden]
    public GameplayHitObjectKeysInfoProxy(GameplayHitObjectKeysInfo info)
    {
        _info = info;
    }

    public HitObjectInfo HitObjectInfo => _info.HitObjectInfo;
    public GameplayHitObjectKeys HitObject => _info.HitObject;
    
    public int StartTime => _info.StartTime;
    public int EndTime => _info.EndTime;
    public int Lane => _info.Lane;
    public bool IsLongNote => _info.IsLongNote;
}