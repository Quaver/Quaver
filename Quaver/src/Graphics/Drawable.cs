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
        //Drawable Object Variables
        protected GraphicsDevice GraphicsDevice;
        public Color Tint = new Color();
        public List<Drawable> Children = new List<Drawable>();
        public Drawable Parent;

        //Constructor
        protected Drawable(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract void Destroy();

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

        /// <summary>
        /// Use this method to set the parent of the Drawable Object.
        /// </summary>
        /// <param name="newParent"></param>
        public void SetParent(Drawable newParent)
        {
            if(Parent != null)
                {
                try
                {
                    int cIndex = Parent.Children.FindIndex(r => r == this);
                    Parent.Children.RemoveAt(cIndex);
                }
                catch
                {
                    Console.WriteLine("[Quaver.Graphics.Drawable]: Error: Cannot find instance of this object from parent");
                }
            }
            newParent.Children.Add(this);
            Parent = newParent;
        }
    }
}
