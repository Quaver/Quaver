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
            Console.WriteLine("A");
            base.Update(dt);
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //TODO: SpriteFont.MeasureString()
            //Console.WriteLine(GlobalRect);
            //Draw itself if it is in the window
            //if (GameBase.Window.Intersects(GlobalRect))
                GameBase.SpriteBatch.DrawString(Font, Text, new Vector2(GlobalRect.X, GlobalRect.Y), TextColor);

            base.Draw();
        }
    }
}
