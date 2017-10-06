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
    internal interface ISprite
    {
        void Instantiate();
        void Draw();
        void Destroy();
    }

    internal abstract class Sprite : ISprite
    {
        protected GraphicsDevice _graphicsDevice;
        public Vector2 Position;
        public Vector2 Size;

        public Sprite(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        //Interface default methods
        public abstract void Instantiate();
        public abstract void Draw();
        public abstract void Destroy();

    }
}
