using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Database.Maps;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.Selector
{
    public class SongSelector : ScrollContainer
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SongSelector() : base(new ScalableVector2(600, 610), new ScalableVector2(600, 700))
        {
            Alignment = Alignment.MidRight;
            X = 0;
            Alpha = 0f;
            Tint = Color.Black;

            Scrollbar.Width = 8;
            Scrollbar.Tint = Color.Black;
            Scrollbar.Alpha = 0.65f;

            AddContainedDrawable(new SongSelectorSet(this, MapManager.Mapsets.First())
            {
                Parent = this
            });

            AddContainedDrawable(new SongSelectorSet(this, MapManager.Mapsets.First())
            {
                Parent = this,
                Y =  70
            });
        }
    }
}
