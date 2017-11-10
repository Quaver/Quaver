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
    internal abstract class Drawable 
    {
        //Methods that will be inherited
        public abstract void Draw();

        public abstract void Update(double dt);

        //Local variables
        private Rectangle _localRect;
        private Rectangle _globalRect;
        private Drawable _parent;
        private Vector2 _scaleSize;
        private Vector2 _scalePercent;
        private Vector2 _localSize;
        private Vector2 _absoluteSize;
        private Vector2 _localPosition;

        /// <summary>
        /// The alignment of the sprite relative to it's parent.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.TopLeft;

        /// <summary>
        /// The color tint of this object. White = no tint.
        /// </summary>
        public Color Tint { get; set; } = new Color();

        /// <summary>
        /// The children of this object that depend on this object's position/size.
        /// </summary>
        public List<Drawable> Children { get; set; } = new List<Drawable>();

        /// <summary>
        /// The parent of this object which it depends on for position/size.
        /// </summary>
        public Drawable Parent
        {
            get => _parent;
            set
            {
                //Remove this object from its old parent's Children list
                if (Parent != null)
                {
                    var cIndex = Parent.Children.FindIndex(r => r == this);
                    Parent.Children.RemoveAt(cIndex);
                }

                //Add this object to its new parent's Children list
                if (value != null) value.Children.Add(this);

                //Assign parent in this object
                _parent = value;
            }
        }

        /// <summary>
        /// (Read-only) Returns the Drawable's GlobalRect.
        /// </summary>
        public Rectangle GlobalRect { get => _globalRect; }


        /// <summary>
        /// (Read-only) Returns the Drawable's LocalRect.
        /// </summary>
        public Rectangle LocalRect { get => _localRect; }

        /// <summary>
        /// The scale of the object relative to its parent.
        /// </summary>
        public Vector2 Scale
        {
            get => _scalePercent;
            set
            {
                _scalePercent = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// (Read-only) The size of the object after it has been scaled.
        /// </summary>
        public Vector2 AbsoluteSize { get => new Vector2(_absoluteSize.X, _absoluteSize.Y); }

        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get => _localSize;
            set
            {
                _localSize = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// The X Size of the Object.
        /// </summary>
        public float SizeX
        {
            get => _localSize.X;
            set
            {
                _localSize.X = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// The Y Size of the Object.
        /// </summary>
        public float SizeY
        {
            get => _localSize.Y;
            set
            {
                _localSize.Y = value;
                SetLocalSize();
            }
        }

        /// <summary>
        /// This is the object's position relative to its parent.
        /// </summary>
        public Vector2 Position
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// The X Position of the Object.
        /// </summary>
        public float PositionX
        {
            get => _localPosition.X;
            set
            {
                _localPosition.X = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// The Y Position of the Object.
        /// </summary>
        public float PositionY
        {
            get => _localPosition.Y;
            set
            {
                _localPosition.Y = value;
                SetLocalPosition();
            }
        }

        /// <summary>
        /// This method is called when the object will be removed from memory.
        /// </summary>
        public void Destroy()
        {
            Parent = null;
        }

        /// <summary>
        /// This method will be called whenever the object gets created.
        /// </summary>
        public void Instantiate()
        {
            
        }

        /// <summary>
        /// This method will get called everytime the size of this object changes.
        /// </summary>
        private void SetLocalSize()
        {
            if (Parent != null)
            {
                _scaleSize.X = _parent._globalRect.Width * _scalePercent.X;
                _scaleSize.Y = _parent._globalRect.Height * _scalePercent.Y;
            }
            else
            {
                _scaleSize.X = GameBase.Window.Width * _scalePercent.X;
                _scaleSize.Y = GameBase.Window.Height * _scalePercent.Y;
            }
            _absoluteSize.X = _localSize.X + _scaleSize.X;
            _absoluteSize.Y = _localSize.Y + _scaleSize.Y;

            _localRect.Width = (int)_absoluteSize.X;
            _localRect.Height = (int)_absoluteSize.Y;
        }

        /// <summary>
        /// This method will get called everytime the position of this object changes.
        /// </summary>
        private void SetLocalPosition()
        {
            _localRect.X = (int)_localPosition.X;
            _localRect.Y = (int)_localPosition.Y;
        }

        /// <summary>
        /// This method will be called everytime a property of this object gets updated.
        /// </summary>
        public void UpdateRect()
        {
            //Children.ForEach(x => x.UpdateRect());

            if (_parent != null)
            {
                _globalRect = Util.DrawRect(Alignment, _localRect, Parent._globalRect);
            }
            else
            {
                //sets the window as the sprite's boundary
                var newBoundary = new Rectangle()
                {
                    Width = (int)GameBase.Window.Width,
                    Height = (int)GameBase.Window.Height
                };

                _globalRect = Util.DrawRect(Alignment, _localRect, newBoundary);
            }
        }
    }
}
