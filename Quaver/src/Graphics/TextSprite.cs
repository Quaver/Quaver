using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Utility;
using Quaver.Main;

namespace Quaver.Graphics
{
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class TextSprite : Drawable
    {
        /// <summary>
        ///     The text of this TextSprite
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Determines if the text will stop before overflowing.
        ///     If multiline is enabled, this value will stop vertical overflowing.
        ///     If multiline is disabled, this value will stop horizontal overflowing.
        /// </summary>
        public bool Textwrap { get; set; } = true;

        /// <summary>
        ///     Determines if more than 1 line of text should be used. 
        ///     If wordwrap is enabled, it will cut off the bottom. 
        ///     If word wrap isn't enabled, it will not cut off at the bottom.
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        ///     The Rectangle of the rendered text inside the TextSprite.
        /// </summary>
        private Rectangle GlobalTextRect { get; set; }

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the TextSprite. Used to reference Text Size.
        /// </summary>
        private Rectangle _textRect;

        /// <summary>
        ///     The size of the rendered text in a single row.
        /// </summary>
        private Vector2 _textSize = new Vector2();

        /// <summary>
        ///     The font of this object
        /// </summary>
        public SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        ///     The color of this object
        /// </summary>
        public Color TextColor = Color.White;

        // Ctor
        public TextSprite()
        {
            Tint = Color.White;
        }

        public override void Update(double dt)
        {
            if (!_changed)
            {
                //Update TextSize
                _textSize = Font.MeasureString(Text);

                //Update TextRect
                _textRect.Width = (int) _textSize.X;
                _textRect.Height = (int) _textSize.Y;

                //Update GlobalTextRect
                GlobalTextRect = Util.DrawRect(Alignment, _textRect, GlobalRect);

                if (Multiline)
                {
                    if (Textwrap)
                        Text = WrapText(Text, false);
                    else
                        //TODO: Cut text after x lines
                        Text = WrapText(Text, false);
                }
                else if (Textwrap)
                    Text = WrapText(Text, true);
            }

            base.Update(dt);
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //TODO: SpriteFont.MeasureString()
            //Draw itself if it is in the window
            if (GameBase.Window.Intersects(GlobalRect))
                GameBase.SpriteBatch.DrawString(Font, Text, new Vector2(GlobalTextRect.X, GlobalTextRect.Y), TextColor);

            base.Draw();
        }

        private string WrapText(string text, bool singleLine)
        {
            if (Font.MeasureString(text).X < SizeX)
            {
                return text;
            }

            string[] words = text.Split(' ');
            StringBuilder wrappedText = new StringBuilder();
            float linewidth = 0f;
            float spaceWidth = Font.MeasureString(" ").X;
            for (int i = 0; i < words.Length; ++i)
            {
                Vector2 size = Font.MeasureString(words[i]);
                if (linewidth + size.X < SizeX)
                {
                    linewidth += size.X + spaceWidth;
                }
                else if (!singleLine)
                {
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;
                }
                else break;
                wrappedText.Append(words[i]);
                wrappedText.Append(" ");
            }

            return wrappedText.ToString();
        }
    }
}
