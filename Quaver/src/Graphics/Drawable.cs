using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Utility;

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
            _SetGlobalRect(_LocalRect);
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

        private Alignment _Alignment = Alignment.TopLeft;

        /// <summary>
        /// The Drawable's Parent.
        /// </summary>
        private Drawable _Parent;

        /// <summary>
        /// The alignment of the sprite relative to it's parent.
        /// </summary>
        public Alignment Alignment
        {
            get
            {
                return _Alignment;
            }
            set
            {
                _Alignment = value;
                _SetGlobalRect(_LocalRect);
            }
        }

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
                _SetGlobalRect(_LocalRect);
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
                _SetGlobalRect(value);
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
                _LocalRect.Width = (int)value.X;
                _LocalRect.Height = (int)value.Y;
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
                Rectangle newRect = _GlobalRect;
                newRect.X = (int)value.X;
                newRect.Y = (int)value.Y;
                _SetGlobalRect(newRect);
            }
        }

        /// <summary>
        /// Internally sets the object's global rect.
        /// </summary>
        /// <param name="newRect"></param>
        private void _SetGlobalRect(Rectangle newRect)
        {
            if (_Parent != null)
            {
                _GlobalRect = Util.DrawRect(_Alignment, newRect, Parent._GlobalRect);
                _GlobalRect.X += newRect.X;
                _GlobalRect.Y += newRect.Y;
                _LocalRect = newRect;
            }
            else
            {
                //Temporary. Access the screen size later.
                Rectangle tempRect = new Rectangle(0, 0, 800, 480);
                _GlobalRect = Util.DrawRect(_Alignment, newRect, tempRect);

                //Set rect
                _GlobalRect.X += newRect.X;
                _GlobalRect.Y += newRect.Y;
                _LocalRect = newRect;
                //Console.WriteLine(_GlobalRect);
            }
        }
    }
}
