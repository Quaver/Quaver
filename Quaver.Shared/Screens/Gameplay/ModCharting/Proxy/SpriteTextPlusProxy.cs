using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class SpriteTextPlusProxy : SpriteProxy
{
    private readonly SpriteTextPlus _text;

    public SpriteTextPlusProxy(SpriteTextPlus text) : base(text)
    {
        _text = text;
        FontSizeProp = new ModChartPropertyInt(() => _text.FontSize, v => _text.FontSize = v);
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

    public readonly ModChartPropertyInt FontSizeProp;

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