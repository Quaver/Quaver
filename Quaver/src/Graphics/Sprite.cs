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
    ///     Any drawable object that uses 
    /// </summary>
    internal class Sprite : Drawable
    {
        /// <summary>
        ///     Image Texture of the sprite.
        /// </summary>
        public Texture2D Image { get; set; }

        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        public float Rotation { get; set; }

        // Ctor
        public Sprite()
        {
            Tint = Color.White;

            //Todo: Set fallback image
            //Image = FALLBACK IMAGE;
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //Draw itself if it is in the window
            if (GameBase.Window.Intersects(GlobalRect))
            GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);

            //Draw children
            Children.ForEach(x => x.Draw());
        }

        /// <summary>
        ///     Will update the sprite. Used for animation/logic
        /// </summary>
        public override void Update(double dt)
        {
            UpdateRect();
            //Animation logic
        }
    }
}
