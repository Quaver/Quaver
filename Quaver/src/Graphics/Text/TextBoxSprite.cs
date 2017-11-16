using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Utility;


namespace Quaver.Graphics.Text
{
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class TextBoxSprite : Drawable
    {
        /// <summary>
        ///     The Actual text of the text Box.
        /// </summary>
        private string _text = "";

        /// <summary>
        ///     Maximum lines of text this body can have before vertical overflow.
        /// </summary>
        private int MaxTextLines { get; set; }

        /// <summary>
        ///     The text of this TextSprite
        /// </summary>
        public string Text {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                Changed = true;
            } 
        }

        public Alignment TextAlignment { get; set; } = Alignment.MidCenter;

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
        private Vector2 _textSize;

        /// <summary>
        ///     The font of this object
        /// </summary>
        public SpriteFont Font { get; set; } = Fonts.Medium12;

        /// <summary>
        ///     The tint this Text Object will inherit.
        /// </summary>
        public Color TextColor
        {
            get
            {
                return _tint;

            }
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            }

        }
        private Color _tint = Color.White;

        /// <summary>
        ///     The transparency of this Text Object.
        /// </summary>
        public float Alpha
        {
            get
            {
                return _alpha;

            }
            set
            {
                _alpha = value;
                _color = _tint * _alpha;
            }
        }
        private float _alpha = 1f;

        /// <summary>
        ///     The color of this Text Object.
        /// </summary>
        private Color _color = Color.White;

        public override void Update(double dt)
        {
            if (Changed)
            {
                //Update TextSize
                _textSize = Font.MeasureString(Text);

                //Update TextRect
                _textRect.Width = (int) _textSize.X;
                _textRect.Height = (int) _textSize.Y;

                //Update GlobalTextRect
                GlobalTextRect = Util.DrawRect(TextAlignment, _textRect, GlobalRect);

                if (Multiline)
                {
                    MaxTextLines = (int) Math.Max(Math.Floor(SizeY / _textSize.Y),1); //TODO: update later
                    _text = WrapText(Text, false);
                }
                else if (Textwrap)
                    _text = WrapText(Text, true);
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
            if (GameBase.Window.Intersects(GlobalRect) && Visible)
                GameBase.SpriteBatch.DrawString(Font, _text, new Vector2(GlobalTextRect.X, GlobalTextRect.Y), _color);

            base.Draw();
        }

        private string WrapText(string text, bool singleLine)
        {
            //Check if text is not short enough to fit on its on box
            if (Font.MeasureString(text).X < SizeX) return text;

            //Reference Variables
            string[] words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").X;
            var textline = 0;

            //Update Text
            foreach (var a in words)
            {
                Vector2 size = Font.MeasureString(a);
                if (linewidth + size.X < SizeX)
                {
                    linewidth += size.X + spaceWidth;
                }
                else if (!singleLine)
                {
                    //Add new line
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;

                    //Check if text wrap should continue
                    textline++;
                    if (Textwrap && textline >= MaxTextLines) break;

                }
                else break;
                wrappedText.Append(a + " ");
            }

            //Console.WriteLine("MAX: {0}, TOTAL {1}", MaxTextLines, textline);

            return wrappedText.ToString();
        }
    }
}
