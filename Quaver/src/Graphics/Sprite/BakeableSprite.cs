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
    ///     This is used for optimization for static sprites. 
    ///     All sprites will be saved onto a texture to save space
    /// </summary>
    internal class BakeableSprite : Drawable
    {
        /// <summary>
        ///     All sprites will be baked onto here
        /// </summary>
        private RenderTarget2D BakedTexture { get;  set; }

        /// <summary>
        ///     This will bake all the children of this object onto a texture and destroy them.
        /// </summary>
        internal void Bake()
        {
            BakedTexture = new RenderTarget2D(GameBase.GraphicsDevice, GameBase.GraphicsDevice.Viewport.Width, GameBase.GraphicsDevice.Viewport.Height);
            GameBase.GraphicsDevice.SetRenderTarget(BakedTexture);
            GameBase.SpriteBatch.Begin();
            Children.ForEach(x => x.Draw());
            GameBase.SpriteBatch.End();
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);
            Children.ForEach(x => x.Destroy());
        }

        /// <summary>
        ///     Draw
        /// </summary>
        internal override void Draw()
        {
            GameBase.SpriteBatch.Draw(BakedTexture, Vector2.Zero, Color.White);
        }
    }
}
