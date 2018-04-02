using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Text
{
    /// <inheritdoc />
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class QuaverTextbox : Drawable
    {
        /// <summary>
        ///     The Actual text of the text Box.
        /// </summary>
        private string _text = "";

        /// <summary>
        ///     The alignment of the text.
        /// </summary>
        internal Alignment TextAlignment { get; set; } = Alignment.MidCenter;

        /// <summary>
        ///     The target scale of the text.
        /// </summary>
        internal float TextScale { get; set; } = 1;

        /// <summary>
        ///     How the text will wrap/scale inside the text box
        /// </summary>
        internal TextBoxStyle TextBoxStyle { get; set; } = TextBoxStyle.OverflowSingleLine;

        /// <summary>
        ///     The Rectangle of the rendered text inside the QuaverTextSprite.
        /// </summary>
        private DrawRectangle _globalTextVect = new DrawRectangle();

        /// <summary>
        ///     The position of the text box
        /// </summary>
        private Vector2 _textPos = Vector2.Zero;

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the QuaverTextSprite. Used to reference Text Size.
        /// </summary>
        private DrawRectangle _textVect = new DrawRectangle();

        /// <summary>
        ///     The size of the rendered text box in a single row.
        /// </summary>
        private Vector2 _textSize;

        /// <summary>
        ///     The scale of the text.
        /// </summary>
        private float _textScale { get; set; } = 1;

        /// <summary>
        ///     The font of this object
        /// </summary>
        internal SpriteFont Font { get; set; } = Fonts.Medium12;

        /// <summary>
        ///     The text of this QuaverTextSprite
        /// </summary>
        internal string Text
        {
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

        /// <summary>
        ///     The tint this Text Object will inherit.
        /// </summary>
        internal Color TextColor
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
        internal float Alpha
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

        internal override void Update(double dt)
        {
            if (Changed) UpdateText();

            base.Update(dt);
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        internal override void Draw()
        {
            //TODO: SpriteFont.MeasureString()
            //Draw itself if it is in the window
            if (GraphicsHelper.RectangleIntercepts(GameBase.WindowRectangle, GlobalRectangle) && Visible)
            {
                if (_textScale == 1 )
                    GameBase.SpriteBatch.DrawString(Font, _text, _textPos, _color);
                else
                    GameBase.SpriteBatch.DrawString(Font, _text, _textPos, _color, 0, Vector2.One, Vector2.One * _textScale, SpriteEffects.None, 0);
            }

            base.Draw();
        }

        private void UpdateText()
        {
            //Update TextSize
            _textSize = Font.MeasureString(Text);
            _textScale = TextScale;

            // Update text with given textbox style
            switch (TextBoxStyle)
            {
                case TextBoxStyle.OverflowMultiLine:
                    _text = WrapText(Text, true, true);
                    break;
                case TextBoxStyle.WordwrapMultiLine:
                    _text = WrapText(Text, true);
                    break;
                case TextBoxStyle.OverflowSingleLine:
                    _text = Text;
                    break;
                case TextBoxStyle.WordwrapSingleLine:
                    _text = WrapText(Text, false);
                    break;
                case TextBoxStyle.ScaledSingleLine:
                    _text = Text;
                    _textScale = ScaleText(AbsoluteSize, _textSize * TextScale) * TextScale;
                    break;
            }

            //Update TextRect
            _textVect.Width = _textSize.X * _textScale;
            _textVect.Height = _textSize.Y * _textScale;

            //Update GlobalTextRect
            _globalTextVect = GraphicsHelper.AlignRect(TextAlignment, _textVect, GlobalRectangle);
            _textPos.X = _globalTextVect.X;
            _textPos.Y = _globalTextVect.Y;
        }

        private float ScaleText(Vector2 boundary, Vector2 textboxsize)
        {
            var sizeYRatio = (boundary.Y / boundary.X) / (textboxsize.Y / textboxsize.X);
            if (sizeYRatio > 1)
                return (boundary.X / textboxsize.X);
            else
                return (boundary.Y / textboxsize.Y);
        }

        private string WrapText(string text, bool multiLine, bool overflow = false)
        {
            //Check if text is not short enough to fit on its on box
            if (Font.MeasureString(text).X < Size.X.Offset) return text;

            //Reference Variables
            string[] words = text.Split(' ');
            var wrappedText = new StringBuilder();
            var linewidth = 0f;
            var spaceWidth = Font.MeasureString(" ").X;
            var textline = 0;
            var MaxTextLines = 99; //todo: remove

            //Update Text
            foreach (var a in words)
            {
                Vector2 size = Font.MeasureString(a);
                if (linewidth + size.X < AbsoluteSize.X)
                {
                    linewidth += size.X + spaceWidth;
                }
                else if (multiLine)
                {
                    //Add new line
                    wrappedText.Append("\n");
                    linewidth = size.X + spaceWidth;

                    //Check if text wrap should continue
                    textline++;
                    if (textline >= MaxTextLines) break;
                }
                else break;
                wrappedText.Append(a + " ");
            }
            //Console.WriteLine("MAX: {0}, TOTAL {1}", MaxTextLines, textline);
            return wrappedText.ToString();
        }
    }
}
