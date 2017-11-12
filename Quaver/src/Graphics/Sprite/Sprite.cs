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

namespace Quaver.Graphics.Sprite
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
            Image = GameBase.UI.BlankBox;
        }

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            //Draw itself if it is in the window
            if (GameBase.Window.Intersects(GlobalRect) && Visible)
            GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);

            //Draw children
            Children.ForEach(x => x.Draw());
        }
    }
}
