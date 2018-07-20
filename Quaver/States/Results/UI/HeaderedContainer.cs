using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;

namespace Quaver.States.Results.UI
{
    internal abstract class HeaderedContainer : Sprite
    {
        /// <summary>
        ///     The header background for the text
        /// </summary>
        protected Sprite Header { get; set; }

        /// <summary>
        ///     The text in the header.
        /// </summary>
        protected SpriteText HeaderText { get; set; }

        /// <summary>
        ///     The size of the content container.
        /// </summary>
        protected Vector2 ContentSize => new Vector2(SizeX, SizeY - Header.SizeY);

        /// <summary>
        ///     The content of
        /// </summary>
        protected Sprite Content { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="size"></param>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="textScale"></param>
        /// <param name="headerTextAlignment"></param>
        /// <param name="headerHeight"></param>
        /// <param name="headerColor"></param>
        internal HeaderedContainer(Vector2 size, string text, SpriteFont font, float textScale,
                                        Alignment headerTextAlignment,  float headerHeight, Color headerColor)
        {
            Size = new UDim2D(size.X, size.Y);
            Alpha = 0;

            Header = new Sprite
            {
                Parent = this,
                Size = new UDim2D(SizeX, headerHeight),
                Tint = headerColor
            };

            HeaderText = new SpriteText
            {
                Parent = Header,
                Text = text,
                Font = font,
                TextScale = textScale,
                Alignment = headerTextAlignment,
                TextAlignment = headerTextAlignment
            };
        }

        /// <summary>
        ///     Creates the sprite for the actual content.
        /// </summary>
        /// <returns></returns>
        protected abstract Sprite CreateContent();
    }
}