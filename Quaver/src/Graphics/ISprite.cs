using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics
{
    /// <summary>
    ///     TODO: Add Summary Here
    /// </summary>
    internal interface ISprite
    {
        void Instantiate();
        void Draw();
        void Destroy();
    }
}
