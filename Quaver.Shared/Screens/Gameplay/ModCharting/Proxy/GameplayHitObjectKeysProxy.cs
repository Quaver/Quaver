using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class GameplayHitObjectKeysProxy
{
    private readonly GameplayHitObjectKeys _hitObject;

    [MoonSharpHidden]
    public GameplayHitObjectKeysProxy(GameplayHitObjectKeys hitObject)
    {
        _hitObject = hitObject;
    }

    public Sprite Sprite => _hitObject.HitObjectSprite;
    public Sprite LongNoteBodySprite => _hitObject.LongNoteBodySprite;
    public Sprite LongNoteEndSprite => _hitObject.LongNoteEndSprite;
}