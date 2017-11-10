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
    ///     This is used for sprite/UI layout
    /// </summary>
    internal class Boundary : Drawable
    {
        // Constructor
        public Boundary()
        {
            SizeX = GameBase.Window.Width;
            SizeY = GameBase.Window.Height;
        }

        /// <summary>
        ///     Draws its children to the screen
        /// </summary>
        public override void Draw()
        {
            Children.ForEach(x => x.Draw());
        }
    }
}
