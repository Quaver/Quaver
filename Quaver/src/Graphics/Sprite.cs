using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Utility;

namespace Quaver.Graphics
{
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class Sprite : Drawable
    {
        /// <summary>
        /// Image Texture of the sprite.
        /// </summary>
        public Texture2D Image
        {
            get;
            set;
        }

        /// <summary>
        /// Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        public float Rotation
        {
            get;
            set;
        }

        //Constructor
        public Sprite(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            Tint = Color.White;
            //Todo: Set fallback image
            //Image = FALLBACK IMAGE;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Draw itself
            spriteBatch.Draw(Image, GlobalRect, Tint);

            //Draw children
            for(int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(spriteBatch);
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
