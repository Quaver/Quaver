using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class Texture2DProxy
{
    private readonly Texture2D _texture;

    [MoonSharpHidden]
    public Texture2DProxy(Texture2D texture)
    {
        _texture = texture;
    }
}