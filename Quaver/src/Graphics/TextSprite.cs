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

        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        public float Rotation { get; set; }

        // Ctor
        public TextSprite()
        {
            Tint = Color.White;
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //TODO: SpriteFont.MeasureString()

            //Draw itself if it is in the window
            Console.WriteLine(GlobalRect);
            if (GameBase.Window.Intersects(GlobalRect))
                GameBase.SpriteBatch.DrawString(Font, Text, new Vector2(GlobalRect.X, GlobalRect.Y), TextColor);

            //Draw children
            Children.ForEach(x => x.Draw());
        }

        /// <summary>
        ///     Will update the sprite. Used for animation/logic
        /// </summary>
        public override void Update(double dt)
        {
            //Animation logic
        }
    }
}
