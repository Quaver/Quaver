using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Maps
{
    public class DifficultyScrollContainer : ScrollContainer
    {
        public DifficultyScrollContainer() : base(
            new ScalableVector2(515, WindowManager.Height - 54 * 2 - 2),
            new ScalableVector2(515, WindowManager.Height - 54 * 2 - 2))
        {

        }
    }
}