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
    ///     This class is for any objects that will be drawn to the screen.
    /// </summary>
    internal abstract class Drawable : IDrawable
    {
        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw();
        public abstract void Destroy();

        private Rectangle _LocalRect = new Rectangle();
        private Rectangle _GlobalRect = new Rectangle();
        private Alignment _Alignment = Alignment.TopLeft;
        private List<Drawable> _Children = new List<Drawable>();
        private Drawable _Parent;
        private Vector2 _ScaleSize;
        private Vector2 _ScalePercent;
        private Vector2 _LocalSize;
        private Vector2 _AbsoluteSize;
        private Vector2 _LocalPosition;

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
            get
            {
                return _Children;
            }
        }

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
                    int cIndex = Parent._Children.FindIndex(r => r == this);
                    Parent._Children.RemoveAt(cIndex);
                }
                value._Children.Add(this);
                _Parent = value;
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
        /// (Read-only) Returns the Drawable's LocalRect.
        /// </summary>
        public Rectangle LocalRect
        {
            get
            {
                return _LocalRect;
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
                SetLocalSize();
            }
        }

        /// <summary>
        /// (Read-only) The size of the object after it has been scaled.
        /// </summary>
        public Vector2 AbsoluteSize
        {
            get
            {
                return new Vector2(_AbsoluteSize.X, _AbsoluteSize.Y);
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
                SetLocalSize();
            }
        }

        /// <summary>
        /// The X Size of the Object.
        /// </summary>
        public float SizeX
        {
            get
            {
                return _LocalSize.X;
            }
            set
            {
                _LocalSize.X = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// The Y Size of the Object.
        /// </summary>
        public float SizeY
        {
            get
            {
                return _LocalSize.Y;
            }
            set
            {
                _LocalSize.Y = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// This is the object's position relative to its parent.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return _LocalPosition;
            }
            set
            {
                _LocalPosition = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// The X Position of the Object.
        /// </summary>
        public float PositionX
        {
            get
            {
                return _LocalPosition.X;
            }
            set
            {
                _LocalPosition.X = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// The Y Position of the Object.
        /// </summary>
        public float PositionY
        {
            get
            {
                return _LocalPosition.Y;
            }
            set
            {
                _LocalPosition.Y = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// This method will get called everytime the size of this object changes.
        /// </summary>
        /// <param name="newScale"></param>
        private void SetLocalSize()
        {
            if (Parent != null)
            {
                _ScaleSize.X = _Parent._GlobalRect.Width * _ScalePercent.X;
                _ScaleSize.Y = _Parent._GlobalRect.Height * _ScalePercent.Y;
            }
            else
            {
                _ScaleSize.X = GameBase.WindowSize.X * _ScalePercent.X;
                _ScaleSize.Y = GameBase.WindowSize.Y * _ScalePercent.Y;
            }
            _AbsoluteSize.X = _LocalSize.X + _ScaleSize.X;
            _AbsoluteSize.Y = _LocalSize.Y + _ScaleSize.Y;

            _LocalRect.Width = (int)_AbsoluteSize.X;
            _LocalRect.Height = (int)_AbsoluteSize.Y;
        }

        /// <summary>
        /// This method will get called everytime the position of this object changes.
        /// </summary>
        /// <param name="newPos"></param>
        private void SetLocalPosition()
        {
            _LocalRect.X = (int)_LocalPosition.X;
            _LocalRect.Y = (int)_LocalPosition.Y;
        }

        /// <summary>
        /// This method will be called everytime a property of this object gets updated.
        /// </summary>
        public void UpdateRect()
        {
            if (_Parent != null)
            {
                _GlobalRect = Util.DrawRect(_Alignment, _LocalRect, Parent._GlobalRect);
                //_GlobalRect.X += _Parent._GlobalRect.X;
                //_GlobalRect.Y += _Parent._GlobalRect.Y;
            }
            else
            {
                //sets the window as the sprite's boundary
                Rectangle newBoundary = new Rectangle()
                {
                    Width = (int)GameBase.WindowSize.X,
                    Height = (int)GameBase.WindowSize.Y
                };

                _GlobalRect = Util.DrawRect(_Alignment, _LocalRect, newBoundary);
            }
        }
    }
}
