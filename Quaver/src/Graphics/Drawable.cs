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
    internal abstract class Drawable 
    {
        //Local variables
        internal bool Changed { get; set; }
        private Vector4 _localVect;
        private Vector4 _globalVect;
        private Drawable _parent;
        private Vector2 _localScale;
        private Vector2 _localSize; //blok

        /// <summary>
        /// The alignment of the sprite relative to it's parent.
        /// </summary>
        public Alignment Alignment { get; set; } = Alignment.TopLeft;

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
                Changed = true;
            }
        }

        /// <summary>
        /// (Read-only) Returns the Drawable's GlobalRect.
        /// </summary>
        public Vector4 GlobalVect { get => _globalVect; }

        /// <summary>
        /// (Read-only) Returns the Drawable's LocalRect.
        /// </summary>
        public Vector4 LocalVect { get => _localVect; }

        /// <summary>
        /// The scale of the object relative to its parent.
        /// </summary>
        public Vector2 Scale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                Changed = true;
            }
        }

        /// <summary>
        /// The X scale of the object relative to its parent.
        /// </summary>
        public float ScaleX
        {
            get => _localScale.X;
            set
            {
                _localScale.X = value;
                Changed = true;
            }
        }

        /// <summary>
        /// The Y scale of the object relative to its parent.
        /// </summary>
        public float ScaleY
        {
            get => _localScale.Y;
            set
            {
                _localScale.Y = value;
                Changed = true;
            }
        }

        /// <summary>
        /// Extention of the object's Rect in relation with size
        /// </summary>
        public Vector2 Size
        {
            get => _localSize;
            set
            {
                _localSize = value;
                Changed = true;
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
                Changed = true;
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
                Changed = true;
            }
        }

        /// <summary>
        /// This is the object's position relative to its parent.
        /// </summary>
        public Vector2 Position
        {
            get => new Vector2(_localVect.X, _localVect.Y);
            set
            {
                _localVect.X = value.X;
                _localVect.Y = value.Y;
                Changed = true;
            }
        }

        /// <summary>
        /// The X Position of the Object.
        /// </summary>
        public float PositionX
        {
            get => _localVect.X;
            set
            {
                _localVect.X = value;
                Changed = true;
            }
        }

        /// <summary>
        /// The Y Position of the Object.
        /// </summary>
        public float PositionY
        {
            get => _localVect.Y;
            set
            {
                _localVect.Y = value;
                Changed = true;
            }
        }

        /// <summary>
        /// Determines if the Object is going to get drawn.
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// This method gets called every frame to update the object.
        /// </summary>
        /// <param name="dt"></param>
        public virtual void Update(double dt)
        {
            //Animation logic
            if (Changed)
            {
                Changed = false;
                RecalculateRect();
            }

            //Update Children
            Children.ForEach(x => x.Update(dt));
        }

        /// <summary>
        /// This method gets called every frame to draw the object.
        /// </summary>
        public virtual void Draw()
        {
            if (Visible)
            Children.ForEach(x => x.Draw());
        }

        /// <summary>
        /// This method will be called everytime a property of this object gets updated.
        /// </summary>
        internal void RecalculateRect()
        {
            //Calculate Scale
            if (_parent != null)
            {
                _localVect.W = _localSize.X + _parent.GlobalVect.W * _localScale.X;
                _localVect.Z = _localSize.Y + _parent.GlobalVect.Z * _localScale.Y;
            }
            else
            {
                _localVect.W = _localSize.X + GameBase.Window.W * _localScale.X;
                _localVect.Z = _localSize.Y + GameBase.Window.Z * _localScale.Y;
            }

            //Update Global Rect
            if (_parent != null)
                _globalVect = Util.DrawRect(Alignment, _localVect, Parent.GlobalVect);
            else
                _globalVect = Util.DrawRect(Alignment, _localVect, GameBase.Window);

            Children.ForEach(x => x.RecalculateRect());
        }

        /// <summary>
        /// This method is called when the object will be removed from memory.
        /// </summary>
        public void Destroy()
        {
            Parent = null;
        }
    }
}
