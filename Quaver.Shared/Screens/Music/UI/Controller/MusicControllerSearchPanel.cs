using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerSearchPanel : Sprite
    {
        /// <summary>
        /// </summary>
        public MusicControllerSearchPanel(float width)
        {
            Size = new ScalableVector2(width, 74);
            Tint = ColorHelper.HexToColor("#242424");
        }
    }
}