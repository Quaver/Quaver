using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Graphics
{
    /// <summary>
    ///     TODO: ADD SUMMARY
    /// </summary>
    internal abstract class Sprite : ISprite
    {
        protected GraphicsDevice GraphicsDevice;
        public Vector2 Position;
        public Vector2 Size;

        protected Sprite(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw();
        public abstract void Destroy();
    }
}
