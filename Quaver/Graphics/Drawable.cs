using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Helpers;

namespace Quaver.Graphics
{
    /// <summary>
    ///     This class is for any objects that will be drawn to the screen.
    /// </summary>
    internal abstract class Drawable
    {
        //Local variables
        internal bool Changed { get; set; } = true;
        private DrawRectangle _localRectangle = new DrawRectangle();
        private DrawRectangle _globalRectangle = new DrawRectangle();
        private Drawable _parent = null;
        internal UDim2 _position = new UDim2();
        internal UDim2 _size = new UDim2();

        /// <summary>
        ///     Position of this Object
        /// </summary>
        internal UDim2 Position
        {
            get => _position;
            set
            {
                _position = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     X Position of this object
        /// </summary>
        internal float PosX
        {
            get => _position.X.Offset;
            set
            {
                _position.X.Offset = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     Y Position of this object
        /// </summary>
        internal float PosY
        {
            get => _position.Y.Offset;
            set
            {
                _position.Y.Offset = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     Size of this object
        /// </summary>
        internal UDim2 Size
        {
            get => _size;
            set
            {
                _size = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     X Size of this object
        /// </summary>
        internal float SizeX
        {
            get => _size.X.Offset;
            set
            {
                _size.X.Offset = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     Y Size of this object
        /// </summary>
        internal float SizeY
        {
            get => _size.Y.Offset;
            set
            {
                _size.Y.Offset = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     X Scale of this object
        /// </summary>
        internal float ScaleX
        {
            get => _size.X.Scale;
            set
            {
                _size.X.Scale = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     Y Scale of this object
        /// </summary>
        internal float ScaleY
        {
            get => _size.Y.Scale;
            set
            {
                _size.Y.Scale = value;
                Changed = true;
            }
        }

        /// <summary>
        ///     The alignment of the sprite relative to it's parent.
        /// </summary>
        internal Alignment Alignment { get; set; } = Alignment.TopLeft;

        /// <summary>
        ///     The children of this object that depend on this object's position/size.
        /// </summary>
        internal List<Drawable> Children { get; set; } = new List<Drawable>();

        /// <summary>
        ///     The parent of this object which it depends on for position/size.
        /// </summary>
        internal Drawable Parent
        {
            get => _parent;
            set
            {
                //Remove this object from its old parent's Children list
                if (_parent != null)
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
        ///     (Read-only) Returns the Drawable's GlobalRect.
        /// </summary>
        internal DrawRectangle GlobalRectangle { get => _globalRectangle; }

        /// <summary>
        ///     (Read-only) Returns the Drawable's LocalRect.
        /// </summary>
        internal DrawRectangle LocalRectangle { get => _localRectangle; }

        /// <summary>
        ///     (Read-only) Absolute _size of this object
        /// </summary>
        internal Vector2 AbsoluteSize { get => new Vector2(_globalRectangle.Width, _globalRectangle.Height); }

        /// <summary>
        ///     (Read-only) Absolute _position of this object
        /// </summary>
        internal Vector2 AbsolutePosition { get => new Vector2(_globalRectangle.X, _globalRectangle.Y); }

        /// <summary>
        ///     Determines if the Object is going to get drawn.
        /// </summary>
        internal bool Visible { get; set; } = true;

        /// <summary>
        ///     This method gets called every frame to update the object.
        /// </summary>
        /// <param name="dt"></param>
        internal virtual void Update(double dt)
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
        ///     This method gets called every frame to draw the object.
        /// </summary>
        internal virtual void Draw()
        {
            if (Visible)
            Children.ForEach(x => x.Draw());
        }

        /// <summary>
        ///     This method will be called everytime a property of this object gets updated.
        /// </summary>
        internal void RecalculateRect()
        {
            //Calculate Scale
            //todo: fix
            if (_parent != null)
            {
                _localRectangle.Width = _size.X.Offset + _parent.GlobalRectangle.Width * _size.X.Scale;
                _localRectangle.Height = _size.Y.Offset + _parent.GlobalRectangle.Height * _size.Y.Scale;
                _localRectangle.X = _position.X.Offset; //todo: implement scale
                _localRectangle.Y = _position.Y.Offset; //todo: implement scale
                //Console.WriteLine(_parent.GlobalRectangle.X + ", " + _parent.GlobalRectangle.Y + ", " + _parent.GlobalRectangle.Width + ", " + _parent.GlobalRectangle.Height);
            }
            else
            {
                _localRectangle.Width = _size.X.Offset + GameBase.WindowRectangle.Width * _size.X.Scale;
                _localRectangle.Height = _size.Y.Offset + GameBase.WindowRectangle.Height * _size.Y.Scale;
                _localRectangle.X = _position.X.Offset; //todo: implement scale
                _localRectangle.Y = _position.Y.Offset; //todo: implement scale
                //Console.WriteLine(GameBase.Window.X + ", " + GameBase.Window.Y + ", " + GameBase.Window.Width + ", " + GameBase.Window.Height);
            }
            //Console.WriteLine(_localRectangle.X + ", " + _localRectangle.Y);

            //Update Global Rect
            if (_parent != null)
                _globalRectangle = GraphicsHelper.AlignRect(Alignment, _localRectangle, Parent.GlobalRectangle);
            else
                _globalRectangle = GraphicsHelper.AlignRect(Alignment, _localRectangle, GameBase.WindowRectangle);

            //Console.WriteLine(_localRectangle.X + ", " + _localRectangle.Y + ", " + _localRectangle.Width + ", " + _localRectangle.Height);
            //Console.WriteLine(_globalRectangle.X + ", " + _globalRectangle.Y + ", " + _globalRectangle.Width + ", " + _globalRectangle.Height);
            Children.ForEach(x => x.Changed = true);
            Children.ForEach(x => x.RecalculateRect());
        }

        /// <summary>
        ///     This method is called when the object will be removed from memory.
        /// </summary>
        internal virtual void Destroy() => Parent = null;
    }
}
