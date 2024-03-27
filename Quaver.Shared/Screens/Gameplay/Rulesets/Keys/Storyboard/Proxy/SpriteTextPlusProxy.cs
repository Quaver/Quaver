using Microsoft.Xna.Framework;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class SpriteTextPlusProxy : SpriteProxy
{
    private SpriteTextPlus _text;
    public SpriteTextPlusProxy(SpriteTextPlus text) : base(text)
    {
        _text = text;
    }
    
    public Color Tint
    {
        get => _text.Tint;
        set => _text.Tint = value;
    }

    public string Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    public int FontSize
    {
        get => _text.FontSize;
        set => _text.FontSize = value;
    }

    public WobbleFontStore Font
    {
        get => _text.Font;
        set => _text.Font = value;
    }

    public float? MaxWidth
    {
        get => _text.MaxWidth;
        set => _text.MaxWidth = value;
    }

    public void TruncateWithEllipsis(int maxWidth) => _text.TruncateWithEllipsis(maxWidth);
}