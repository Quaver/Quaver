using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Utility;

namespace Quaver.Graphics.Sprite
{
    /// <summary>
    ///     This is used for sprite/UI layout
    /// </summary>
    internal class Boundary : Drawable
    {
        /// <summary>
        ///     Create a new Boundary Given Xposition, Yposition, Xsize, Ysize
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        public Boundary(float xPosition = 0, float yPosition = 0, float? xSize = null, float? ySize = null)
        {
            Size.X.Offset = xSize != null ? (float)xSize : GameBase.WindowRectangle.Width;
            Size.Y.Offset = ySize != null ? (float)ySize : GameBase.WindowRectangle.Height;
            Position.X.Offset = xPosition;
            Position.Y.Offset = yPosition;
        }
    }
}
