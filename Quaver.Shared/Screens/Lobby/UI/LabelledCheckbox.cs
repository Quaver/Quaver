using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class LabelledCheckbox : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextBitmap Key { get; set; }

        /// <summary>
        /// </summary>
        private Checkbox Checkbox { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public LabelledCheckbox(string key, Bindable<bool> value)
        {
            Key = new SpriteTextBitmap(FontsBitmap.GothamRegular, key)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                FontSize = 16,
            };

            Checkbox = new Checkbox(value, new Vector2(16, 16), FontAwesome.Get(FontAwesomeIcon.fa_check),
                FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty), false)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Key.X + Key.Width + 14,
            };

            Size = new ScalableVector2(Checkbox.Width + 5 + Key.Width, Checkbox.Height);
            Alpha = 0;
        }
    }
}