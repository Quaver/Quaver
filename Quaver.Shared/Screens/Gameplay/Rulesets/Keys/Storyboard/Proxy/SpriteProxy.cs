using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

// TODO decide if this should be included
public class SpriteProxy
{
    private readonly Sprite _sprite;

    public SpriteProxy(Sprite sprite)
    {
        _sprite = sprite;
    }

    public ScalableVector2 Position
    {
        get => _sprite.Position;
        set => _sprite.Position = value;
    }

    public ScalableVector2 Size
    {
        get => _sprite.Size;
        set => _sprite.Size = value;
    }

    public float Rotation
    {
        get => _sprite.Rotation;
        set => _sprite.Rotation = value;
    }

    public float Alpha
    {
        get => _sprite.Alpha;
        set => _sprite.Alpha = value;
    }
}