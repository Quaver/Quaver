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
                _origin.X = _image.Width / 2f;
                _origin.Y = _image.Height / 2f;
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
            } 
        }
        private float _rotation;

        /// <summary>
        ///     The Effects this sprite will inherit.
        /// </summary>
        public SpriteEffects SpriteEffect { get; set; } = SpriteEffects.None;

        /// <summary>
        ///     The origin of this object used for rotation.
        /// </summary>
        private Vector2 _origin = new Vector2(GameBase.UI.BlankBox.Width/2f, GameBase.UI.BlankBox.Width/2f); //TODO: bake this value later

        /// <summary>
        ///     The Rectangle used to render the sprite.
        /// </summary>
        private Rectangle _renderRect;

        /// <summary>
        ///     The tint this Sprite will inherit.
        /// </summary>
        public Color Tint
        {
            get
            {
                return _tint;
                
            }
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            } 
            
        }
        private Color _tint = Color.White;

        /// <summary>
        ///     The transparency of this Sprite.
        /// </summary>
        public float Alpha {
            get
            {
                return _alpha; 
                
            }
            set
            {
                _alpha = value;
                _color = _tint * _alpha;
            } 
        }
        private float _alpha = 1f;

        /// <summary>
        ///     The color of this Sprite.
        /// </summary>
        private Color _color = Color.White;

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        public override void Draw()
        {
            if (Changed) RecalculateOrigin();

            //Draw itself if it is in the window
            //Old: GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            if (GameBase.Window.Intersects(GlobalRect) && Visible)
            {
                GameBase.SpriteBatch.Draw(_image, _renderRect, null, _color, _rotation, _origin, SpriteEffect, 0f);
            }

            //Draw children
            base.Draw();
        }

        /// <summary>
        ///     Update the sprite every frame.
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(double dt)
        {
            //_rotation += 0.0007f;
            if (Changed)
            {
                base.Update(dt);
                RecalculateOrigin();
            }
            else
            base.Update(dt);
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        private void RecalculateOrigin()
        {
            _renderRect = GlobalRect;
            _renderRect.X = (int)(GlobalRect.X + GlobalRect.Width / 2f);
            _renderRect.Y = (int)(GlobalRect.Y + GlobalRect.Height / 2f);
        }
    }
}
