using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Sprites
{
    /// <inheritdoc />
    /// <summary>
    ///     Any drawable object that uses 
    /// </summary>
    internal class Sprite : Drawable
    {
        /// <summary>
        ///     Image Texture of the sprite.
        /// </summary>
        private Texture2D _image = GameBase.QuaverUserInterface.BlankBox;
        internal Texture2D Image
        {
            get => _image;
            set
            {
                _image = value;
                _origin.X = _image.Width / 2f;
                _origin.Y = _image.Height / 2f;
                Changed = true;
            }
        }

        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        private float _rotation;
        internal float Rotation 
        {
            get => _rotation;
            set => _rotation = MathHelper.ToRadians(value);
        }

        /// <summary>
        ///     The Effects this sprite will inherit.
        /// </summary>
        internal SpriteEffects SpriteEffect { get; set; } = SpriteEffects.None;

        /// <summary>
        ///     The origin of this object used for rotation.
        /// </summary>
        private Vector2 _origin = new Vector2(GameBase.QuaverUserInterface.BlankBox.Width/2f, GameBase.QuaverUserInterface.BlankBox.Height/2f); //TODO: bake this value later

        /// <summary>
        ///     The Rectangle used to render the sprite.
        /// </summary>
        private Rectangle _renderRectangle;

        /// <summary>
        /// 
        /// </summary>
        private readonly DrawRectangle _originRectangle = new DrawRectangle();

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        private Color _tint = Color.White;
        private Color _color = Color.White;
        internal Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            } 
        }

        /// <summary>
        ///     The transparency of this QuaverSprite.
        /// </summary>
        private float _alpha = 1f;
        internal float Alpha {
            get => _alpha;
            set
            {
                _alpha = value;
                _color = _tint * _alpha;
            } 
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update the sprite every frame.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
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
        
        /// <inheritdoc />
        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        internal override void Draw()
        {
            //Draw itself if it is in the window
            if (GraphicsHelper.RectangleIntercepts(GlobalRectangle, GameBase.WindowRectangle) && Visible)
            {
                GameBase.SpriteBatch.Draw(_image, _renderRectangle, null, _color, _rotation, _origin, SpriteEffect, 0f);
            }

            //Draw children
            base.Draw();
        }

        /// <summary>
        ///     Recalculate Origin + Rotation of sprite
        /// </summary>
        private void RecalculateOrigin()
        {
            // Update Origin Rect
            _originRectangle.Width = GlobalRectangle.Width;
            _originRectangle.Height = GlobalRectangle.Height;
            _originRectangle.X = (GlobalRectangle.X + (GlobalRectangle.Width / 2f));
            _originRectangle.Y = (GlobalRectangle.Y + (GlobalRectangle.Height / 2f));

            // Update Render Rect
            _renderRectangle.X = (int)_originRectangle.X;
            _renderRectangle.Y = (int)_originRectangle.Y;
            _renderRectangle.Width = (int) _originRectangle.Width;
            _renderRectangle.Height = (int) _originRectangle.Height;
        }

        /// <summary>
        ///     Fades out the sprite to a given alpha.
        /// </summary>
        internal void Fade(double dt, float target, float scale) => Alpha = GraphicsHelper.Tween(target, Alpha, Math.Min(dt / scale, 1)); 
        
        /// <summary>
        ///     Completely fades out the object.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        internal void FadeOut(double dt, float scale) => Alpha = GraphicsHelper.Tween(0, Alpha, Math.Min(dt / scale, 1)); 
        
        /// <summary>
        ///     Completely fades in the object.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        internal void FadeIn(double dt, float scale) => Alpha = GraphicsHelper.Tween(1, Alpha, Math.Min(dt / scale, 1));

        /// <summary>
        ///     Moves the sprite to a given position.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        internal void Translate(Vector2 pos, double dt, float scale)
        {
            PosX = GraphicsHelper.Tween(pos.X, PosX, Math.Min(dt / scale, 1));
            PosY = GraphicsHelper.Tween(pos.Y, PosY, Math.Min(dt / scale, 1));
        }
        
        /// <summary>
        ///     Fades the sprite to a given color.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="dt"></param>
        /// <param name="scale"></param>
        internal void FadeToColor(Color color, double dt, float scale)
        {
            var r = GraphicsHelper.Tween(color.R, Tint.R, Math.Min(dt / scale, 1));
            var g = GraphicsHelper.Tween(color.G, Tint.G, Math.Min(dt / scale, 1));
            var b = GraphicsHelper.Tween(color.B, Tint.B, Math.Min(dt / scale, 1));
            
            Tint = new Color((int)r, (int)g, (int)b);
        }
    }
}
