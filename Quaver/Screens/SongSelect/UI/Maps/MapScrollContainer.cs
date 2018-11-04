using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.SongSelect.UI.Maps
{
    public class MapScrollContainer : ScrollContainer
    {
        public MapScrollContainer(ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false) : base(size, contentSize, startFromBottom)
        {
        }
    }
}