using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public abstract class MultiplayerSlot : Sprite
    {
        public MultiplayerSlot()
        {
            Size = new ScalableVector2(570, 62);
            Tint = ColorHelper.HexToColor("#242424");
        }
    }
}