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
        /// The text of this TextSprite
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     The Rectangle of the rendered text inside the TextSprite.
        /// </summary>
        private Rectangle GlobalTextRect { get; set; }

        /// <summary>
        ///     The Local Rectangle of the rendered text inside the TextSprite. Used to reference Text Size.
        /// </summary>
        private Rectangle _textRect;

        private Vector2 _textSize = new Vector2();

        /// <summary>
        /// The font of this object
        /// </summary>
        public SpriteFont Font { get; } = GameBase.Content.Load<SpriteFont>("testFont");

        /// <summary>
        /// The color of this object
        /// </summary>
        public Color TextColor = Color.White;

        // Ctor
        public TextSprite()
        {
            Tint = Color.White;
        }

        public override void Update(double dt)
        {
            //Update TextSize
            _textSize = Font.MeasureString(Text);

            //Update TextRect
            _textRect.Width = (int)_textSize.X;
            _textRect.Height = (int)_textSize.Y;

            //Update GlobalTextRect
            GlobalTextRect = Util.DrawRect(Alignment, _textRect, GlobalRect);

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
    }
}
