﻿using System;
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
        public int MaxTextLines { get; set; }

        /// <summary>
        ///     The alignment of the text.
        /// </summary>
        public Alignment TextAlignment { get; set; } = Alignment.MidCenter;

        /// <summary>
        ///     The scale of the text.
        /// </summary>
        public float TextScale { get; set; } = 1.5f;

        /// <summary>
        ///     Determines if the text will stop before overflowing.
        ///     If multiline is enabled, this value will stop vertical overflowing.
        ///     If multiline is disabled, this value will stop horizontal overflowing.
        /// </summary>
        public bool Wordwrap { get; set; } = true;

        /// <summary>
        ///     Determines if more than 1 line of text should be used. 
        ///     If wordwrap is enabled, it will cut off the bottom. 
        ///     If word wrap isn't enabled, it will not cut off at the bottom.
        /// </summary>
        public bool Multiline { get; set; }

        /// <summary>
        ///     The Rectangle of the rendered text inside the TextSprite.
        /// </summary>
        private Vector4 _globalTextVect = Vector4.Zero;

        /// <summary>
        ///     The position of the text box
        /// </summary>
        private Vector2 _textPos = Vector2.Zero;

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the TextSprite. Used to reference Text Size.
        /// </summary>
        private Vector4 _textVect;

        /// <summary>
        ///     The size of the rendered text box in a single row.
        /// </summary>
        private Vector2 _textSize;

        /// <summary>
        ///     The font of this object
        /// </summary>
        public SpriteFont Font { get; set; } = Fonts.Medium12;

        /// <summary>
        ///     The text of this TextSprite
        /// </summary>
        public string Text
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
            if (Changed) UpdateText();

            base.Update(dt);
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //TODO: SpriteFont.MeasureString()
            //Draw itself if it is in the window
            if (Util.Vector4Intercepts(GameBase.Window, GlobalVect) && Visible)
                GameBase.SpriteBatch.DrawString(Font, _text, _textPos, _color, 0, Vector2.One, Vector2.One * TextScale, SpriteEffects.None, 0);

            /*
                 SpriteFont spriteFont,
                 string text,
                 Vector2 position,
                 Color color,
                 float rotation,
                 Vector2 origin,
                 Vector2 scale,
                 SpriteEffects effects,
                 float layerDepth
            */

            base.Draw();
        }

        private void UpdateText()
        {
            //Update TextSize
            _textSize = Font.MeasureString(Text);

            //Update TextRect
            _textVect.W = _textSize.X * TextScale;
            _textVect.Z = _textSize.Y * TextScale;

            //Update GlobalTextRect
            _globalTextVect = Util.DrawRect(TextAlignment, _textVect, GlobalVect);
            _textPos.X = _globalTextVect.X;
            _textPos.Y = _globalTextVect.Y;

            if (Multiline)
            {
                MaxTextLines = (int)Math.Max(Math.Floor(SizeY / _textSize.Y), 1); //TODO: implement max text lines update later
                _text = WrapText(Text, false);
            }
            else if (Wordwrap)
                _text = WrapText(Text, true);
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
                    if (Wordwrap && textline >= MaxTextLines) break;
                }
                else break;
                wrappedText.Append(a + " ");
            }
            //Console.WriteLine("MAX: {0}, TOTAL {1}", MaxTextLines, textline);
            return wrappedText.ToString();
        }
    }
}
