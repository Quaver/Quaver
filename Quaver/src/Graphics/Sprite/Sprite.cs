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
                ImageChanged = true;
            }
        }
        private Texture2D _image = GameBase.UI.BlankBox;


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
                ImageChanged = true;
            } 
        }
        private float _rotation;

        public SpriteEffects SpriteEffect { get; set; }

        /// <summary>
        ///     The origin of this object used for rotation.
        /// </summary>
        private Vector2 Origin { get; set; }

        /// <summary>
        ///     The Rectangle used to render the sprite.
        /// </summary>
        private Rectangle _renderRect;

        /// <summary>
        ///     Gets toggled on whenever the image or rotation gets changed.
        /// </summary>
        private bool ImageChanged { get; set; }

        public Color Tint { get; set; } = Color.White;

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            if (Changed) Console.WriteLine(_image);
            if (ImageChanged)
            {
                RecalculateOrigin();
                //Console.WriteLine(_image);
            }

            //Draw itself if it is in the window
            //Old: GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            if (GameBase.Window.Intersects(GlobalRect) && Visible) //GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            GameBase.SpriteBatch.Draw(_image, _renderRect, null, Tint, _rotation, Origin, SpriteEffect, 0f);

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
            if (Changed) ImageChanged = true;
            base.Update(dt);
            if (ImageChanged) RecalculateOrigin();
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        private void RecalculateOrigin()
        {
            Origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
            _renderRect = GlobalRect;
            _renderRect.X = (int)(GlobalRect.X + GlobalRect.Width / 2f);
            _renderRect.Y = (int) (GlobalRect.Y + GlobalRect.Height / 2f);

            ImageChanged = false;
        }
    }
}
