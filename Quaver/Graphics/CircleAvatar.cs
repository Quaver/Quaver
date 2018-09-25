using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Graphics
{
    public class CircleAvatar : SpriteMaskContainer
    {
        /// <summary>
        ///     The sprite for the masked avatar.
        /// </summary>
        public Sprite AvatarSprite { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="image"></param>
        public CircleAvatar(ScalableVector2 size, Texture2D image)
        {
            Image = FontAwesome.CircleClosed;
            Size = size;

            AvatarSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = Size,
                Image = image,
            };

            AddContainedSprite(AvatarSprite);
        }
    }
}