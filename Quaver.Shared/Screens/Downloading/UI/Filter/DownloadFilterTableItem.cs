using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterTableItem : Sprite
    {
        /// <summary>
        /// </summary>
        protected SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="name"></param>
        public DownloadFilterTableItem(int width, string name)
        {
            Size = new ScalableVector2(width, 58);
            CreateName(name);
        }

        /// <summary>
        /// </summary>
        private void CreateName(string name) => Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 22)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = 18
        };
    }
}