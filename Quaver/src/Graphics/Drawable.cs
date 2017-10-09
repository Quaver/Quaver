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
        /// This field is used to calculate the local position of the object.
        /// </summary>
        private Rectangle _LocalRect = new Rectangle();

        /// <summary>
        /// This field is used to calculate the global position of the object.
        /// </summary>
        private Rectangle _GlobalRect = new Rectangle();

        /// <summary>
        /// The Drawable's Parent.
        /// </summary>
        private Drawable _Parent;

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
            get
            {
                return _Parent;
            }
            set
            {
                if (Parent != null)
                {
                    int cIndex = Parent.Children.FindIndex(r => r == this);
                    Parent.Children.RemoveAt(cIndex);
                }
                value.Children.Add(this);
                _Parent = value;

                //efficiently moves object's global position relative to it's parent's position and it's own local position
                LocalRect = _LocalRect;
            }
        }

        /// <summary>
        /// The rectangle of the Drawable. (Position.X, Position.Y, Size.Width, Size.Height).
        /// </summary>
        public Rectangle LocalRect
        {
            get
            {
                return _LocalRect;
            }
            set
            {
                if (_Parent != null)
                {
                    _GlobalRect.X = _Parent._GlobalRect.X + value.X;
                    _GlobalRect.Y = _Parent._GlobalRect.Y + value.Y;
                    _LocalRect = value;
                }
                else
                {
                    _GlobalRect = value;
                    _LocalRect = value;
                }
            }
        }

        public Rectangle GlobalRect
        {
            get
            {
                return _GlobalRect;
            }
        }

        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return new Vector2(_GlobalRect.Width, _GlobalRect.Height);
            }
            set
            {
                _GlobalRect.Width = (int)value.X;
                _GlobalRect.Height = (int)value.Y;
            }
        }

        /// <summary>
        /// This is the object's position relative to it's parent.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(_GlobalRect.X, _GlobalRect.Y);
            }
            set
            {
                if (_Parent != null)
                {
                    Rectangle newRect = _GlobalRect;
                    newRect.X = _Parent._GlobalRect.X + (int)value.X;
                    newRect.Y = _Parent._GlobalRect.Y + (int)value.Y;
                    _GlobalRect = newRect;
                }
                _LocalRect.X = (int)value.X;
                _LocalRect.Y = (int)value.Y;
            }
        }
    }
}
