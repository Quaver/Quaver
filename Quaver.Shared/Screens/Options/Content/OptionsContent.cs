using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Options.Content
{
    public class OptionsContent : Sprite
    {
        public OptionsContent(ScalableVector2 size)
        {
            Size = size;
            Tint = ColorHelper.HexToColor("#2F2F2F");
        }
    }
}