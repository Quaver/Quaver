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
        //Constructor
        protected Drawable()
        {
            SetGlobalRect(_LocalRect);
        }

        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw();
        public abstract void Destroy();

        private Rectangle _LocalRect = new Rectangle();
        private Rectangle _GlobalRect = new Rectangle();
        private Alignment _Alignment = Alignment.TopLeft;
        private Drawable _Parent;
        private Vector2 _ScaleSize;
        private Vector2 _ScalePercent;
        private Vector2 _LocalSize;

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
                SetGlobalRect(_LocalRect);
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
                SetGlobalRect(_LocalRect);
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
                SetGlobalRect(value);
            }
        }

        /// <summary>
        /// (Read-only) Returns the Drawable's GlobalRect.
        /// </summary>
        public Rectangle GlobalRect
        {
            get
            {
                return _GlobalRect;
            }
        }

        /// <summary>
        /// The scale of the object relative to its parent.
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                return _ScalePercent;
            }
            set
            {
                _ScalePercent = value;
                SetGlobalSize(value);
            }
        }

        /// <summary>
        /// (Read-only) The size of the object after it has been scaled.
        /// </summary>
        public Vector2 AbsoluteSize
        {
            get
            {
                return new Vector2(_LocalRect.Width, _LocalRect.Height);
            }
        }
        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get
            {
                return _LocalSize;
            }
            set
            {
                _LocalSize = value;
                SetGlobalSize(_ScalePercent);
            }
        }

        /// <summary>
        /// This is the object's position relative to its parent.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(_LocalRect.X, _LocalRect.Y);
            }
            set
            {
                Rectangle newRect = _LocalRect;
                newRect.X = (int)value.X;
                newRect.Y = (int)value.Y;
                SetGlobalRect(newRect);
            }
        }

        /// <summary>
        /// Internally sets the object's global size.
        /// </summary>
        /// <param name="newScale"></param>
        private void SetGlobalSize(Vector2 newScale)
        {
            if (Parent != null)
            {
                _ScaleSize.X = _Parent._GlobalRect.Width * newScale.X;
                _ScaleSize.Y = _Parent._GlobalRect.Height * newScale.Y;
            }
            else
            {
                //Temporary. Access the screen size later.
                //Console.WriteLine(_ScalePercent);
                Vector2 tempScreenSize = new Vector2(800, 400);
                _ScaleSize.X = tempScreenSize.X * newScale.X;
                _ScaleSize.Y = tempScreenSize.Y * newScale.Y;
                //_ScaleSize = Vector2.Zero;
            }
            _LocalRect.Width = (int)(_LocalSize.X + _ScaleSize.X);
            _LocalRect.Height = (int)(_LocalSize.Y + _ScaleSize.Y);
            SetGlobalRect(_LocalRect);
        }

        /// <summary>
        /// Internally sets the object's global rect.
        /// </summary>
        /// <param name="newRect"></param>
        private void SetGlobalRect(Rectangle newRect)
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

                //SetBlobalRect
                _GlobalRect = Util.DrawRect(_Alignment, newRect, tempRect);

                //Set rect
                _GlobalRect.X += newRect.X;
                _GlobalRect.Y += newRect.Y;
                _LocalRect = newRect;
                Console.WriteLine(_LocalSize);
                //Console.WriteLine(_GlobalRect);
            }
        }
    }
}
