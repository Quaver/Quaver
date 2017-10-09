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
        /// The rectangle of the Drawable. (Position.X, Position.Y, Size.Width, Size.Height). This variable can only be accessed within the Drawable Class.
        /// </summary>
        private Rectangle _Rect = new Rectangle();

        /// <summary>
        /// TODO: Add summary later.
        /// </summary>
        public Color Tint
        {
            get;
            set;
        } = new Color();

        /// <summary>
        /// TODO: Add summary later.
        /// </summary>
        public List<Drawable> Children
        {
            get;
            set;
        } = new List<Drawable>();

        /// <summary>
        /// TODO: Add summary later.
        /// </summary>
        public Drawable Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(_Rect.Width, _Rect.Height);
            }
            set
            {
                _Rect.Width = (int)value.X;
                _Rect.Height = (int)value.Y;
            }
        }

        /// <summary>
        /// The rectangle of the Drawable. (Position.X, Position.Y, Size.Width, Size.Height).
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                return _Rect;
            }
            set
            {
                _Rect = value;
            }
        }

        /// <summary>
        /// Extention of the object's Rect in relation with Position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(_Rect.X, _Rect.Y);
            }
            set
            {
                _Rect.X = (int)value.X;
                _Rect.Y = (int)value.Y;
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
