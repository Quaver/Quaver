using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class LabelledMetadata : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextBitmap Key { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Value { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public LabelledMetadata(float width, string key, string value)
        {
            Width = width;

            Key = new SpriteTextBitmap(FontsBitmap.GothamRegular, key)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                FontSize = 17,
            };

            Value = new SpriteTextBitmap(FontsBitmap.GothamRegular, value)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                FontSize = 17
            };

            Height = Key.Height;
            Alpha = 0;
        }
    }
}