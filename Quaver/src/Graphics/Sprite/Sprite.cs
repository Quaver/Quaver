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
        public Texture2D Image
        {
            get
            {
                return _image;
                
            }
            set
            {
                _image = value;
            }
        }
        private Texture2D _image;


        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        public float Rotation {
            get
            {
                return _rotation;
                
            }
            set
            {
                _rotation = MathHelper.ToRadians(value);
                Changed = true;
            } 
        }
        private float _rotation;

        /// <summary>
        ///     The origin of this object used for rotation.
        /// </summary>
        private Vector2 Origin { get; set; }

        /// <summary>
        ///     The Rectangle used to render the sprite.
        /// </summary>
        private Rectangle RenderRect { get; set; }

        // Constructor
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
            //Old: GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            if (GameBase.Window.Intersects(GlobalRect) && Visible) //GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            GameBase.SpriteBatch.Draw(_image, RenderRect, null, Color.White, _rotation, Origin, SpriteEffects.None, 0f);
            //Draw children
            Children.ForEach(x => x.Draw());
        }

        /// <summary>
        ///     Update the sprite every frame.
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(double dt)
        {
            //_rotation += 0.0007f;
            var checkChange = Changed;
            base.Update(dt);
            if (checkChange) RecalculateOrigin();
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        private void RecalculateOrigin()
        {
            Origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
            RenderRect = new Rectangle((int)(GlobalRect.X + GlobalRect.Width / 2f), (int)(GlobalRect.Y + GlobalRect.Height / 2f),
                GlobalRect.Width, GlobalRect.Height);
        }
    }
}
