using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     This is used for optimization for static sprites. 
    ///     All sprites will be saved onto a texture to save space
    /// </summary>
    internal class QuaverBakeableSprite : Sprites.QuaverSprite
    {
        /// <summary>
        ///     All sprites will be baked onto here
        /// </summary>
        private RenderTarget2D BakedTexture { get;  set; }

        /// <summary>
        ///     Determines if the object is ready to be baked
        /// </summary>
        internal bool BakeReady { get; set; } = true;

        /// <summary>
        ///     This will bake all the children of this object onto a texture and destroy them.
        /// </summary>
        private void Bake()
        {
            BakedTexture = new RenderTarget2D(GameBase.GraphicsDevice, (int)Math.Ceiling(AbsoluteSize.X), (int)Math.Ceiling(AbsoluteSize.Y));
            Alpha = 0;
            GameBase.GraphicsDevice.SetRenderTarget(BakedTexture);
            GameBase.GraphicsDevice.Clear(Color.White * 0);
            GameBase.SpriteBatch.Begin();
            Draw();
            GameBase.SpriteBatch.End();
            GameBase.GraphicsDevice.SetRenderTarget(GameBase.MainRenderTarget);
            Children = new List<Drawable>();
            Image = BakedTexture;
            Alpha = 1;
        }

        /// <summary>
        ///     Draw
        /// </summary>
        internal override void Update(double dt)
        {
            base.Update(dt);

            // Bake the object after this object has been updated
            if (BakeReady)
            {
                BakeReady = false;
                Bake();
            }
        }
    }
}
