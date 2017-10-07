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
        public Texture2D Image;

        public Sprite(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {

        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Image,Rect,Tint);

            //Debugging (Temp)
            //Console.WriteLine("{0}, {1}, {2}, {3}", Rect.X, Rect.Y, Rect.Width, Rect.Height);
        }
        public override void Destroy()
        {
            
        }
        public override void Instantiate()
        {
            
        }
    }
}
