using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Graphics
{
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class Sprite : Drawable
    {
        /// <summary>
        /// Image Texture of the sprite.
        /// </summary>
        public Texture2D Image;

        /// <summary>
        /// Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        public float Rotation = 0;

        public Sprite(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            Tint = Color.White;
            //Image = FALLBACK IMAGE;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            //Debugging (Temporary)
            //Console.WriteLine("{0}, {1}, {2}, {3}", Rect.X, Rect.Y, Rect.Width, Rect.Height);

            spriteBatch.Draw(Image, Rect, Tint);
        }
        public override void Destroy()
        {
            
        }
        public override void Instantiate()
        {
            
        }
    }
}
