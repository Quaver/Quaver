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
        public Boundary()
        {
            Size = GameBase.WindowSize;
        }

        public override void Draw()
        {

            //Draw children
            for(int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw();
            }
        }

        public override void Destroy()
        {            
        }

        public override void Instantiate()
        {            
        }
    }
}
