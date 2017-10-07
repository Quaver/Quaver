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
    ///     This class is for any objects that will be drawn to the screen.
    /// </summary>
    internal abstract class Drawable : IDrawable
    {
        protected GraphicsDevice GraphicsDevice;
        public Color Tint = new Color();

        /// <summary>
        /// The rectangle of the Drawable. (Position.X, Position.Y, Size.Width, Size.Height)
        /// </summary>
        public Rectangle Rect = new Rectangle();

        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(Rect.Width, Rect.Height);
            }
            set
            {
                Rect.Width = (int)value.X;
                Rect.Height = (int)value.Y;
            }
        }

        /// <summary>
        /// Extention of the object's Rect in relation with Position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(Rect.X, Rect.Y);
            }
            set
            {
                Rect.X = (int)value.X;
                Rect.Y = (int)value.Y;
            }
        }


        protected Drawable(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Destroy();
    }
}
