using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class LocalProfileTableItem : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Value { get; }

        /// <summary>
        /// </summary>
        private const int Padding = 16;

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public LocalProfileTableItem(string name, string value)
        {
            Alpha = 0;
            SetChildrenAlpha = true;

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Padding,
                SetChildrenAlpha = true
            };

            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), value, 22)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Padding,
                SetChildrenAlpha = true
            };
        }
    }
}