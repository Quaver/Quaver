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
        /// The alignment of the sprite relative to it's parent.
        /// </summary>
        public Alignment Alignment
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

        public Sprite(GraphicsDevice graphicsDevice) :base(graphicsDevice)
        {
            Tint = Color.White;
            //Image = FALLBACK IMAGE;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Parent == null)
            {
                spriteBatch.Draw(Image, Rect, Tint);
            }
            else
            {
                Rectangle NewOffset = Util.DrawRect(Alignment, Size , Parent.Rect, Position);
                spriteBatch.Draw(Image, NewOffset, Tint);
            }

            //Draws children
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
