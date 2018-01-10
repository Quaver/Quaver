﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Utility;
using Quaver.Logging;

namespace Quaver.Graphics.Sprite
{
    /// <summary>
    ///     This is used for optimization for static sprites. 
    ///     All sprites will be saved onto a texture to save space
    /// </summary>
    internal class BakeableSprite : Sprite
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
        internal override void Draw()
        {
            base.Draw();
        }
    }
}
