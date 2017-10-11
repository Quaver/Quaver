using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Graphics
{
    /// <summary>
    ///     Any object that will be drawn to the game inherits from IDrawable.
    /// </summary>
    internal interface IDrawable
    {
        void Instantiate();
        void Draw();
        void Destroy();
    }
}
