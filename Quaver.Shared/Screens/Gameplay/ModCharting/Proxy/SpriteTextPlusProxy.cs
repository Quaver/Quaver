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
        TextProp = new ModChartGeneralProperty<string>(() => _text.Text, v => _text.Text = v);
        MaxWidthProp = new ModChartPropertyFloat(() => MaxWidth, v => MaxWidth = v);
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
    public readonly ModChartGeneralProperty<string> TextProp;

    public WobbleFontStore Font
    {
        get => _text.Font;
        set => _text.Font = value;
    }

    public float MaxWidth
    {
        get => _text.MaxWidth ?? 0;
        set => _text.MaxWidth = value <= 0 ? null : value;
    }

    public ModChartPropertyFloat MaxWidthProp;

    public void TruncateWithEllipsis(int maxWidth) => _text.TruncateWithEllipsis(maxWidth);
}