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
    internal class QuaverSprite : Drawable
    {
        /// <summary>
        ///     Image Texture of the sprite.
        /// </summary>
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
        private Texture2D _image = GameBase.QuaverUserInterface.BlankBox;

        /// <summary>
        ///     Angle of the sprite with it's origin in the centre. (TEMPORARILY NOT USED YET)
        /// </summary>
        internal float Rotation {
            get => _rotation;
            set
            {
                _rotation = MathHelper.ToRadians(value);
            } 
        }
        private float _rotation;

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

        private DrawRectangle _originRectangle = new DrawRectangle();

        /// <summary>
        ///     The tint this QuaverSprite will inherit.
        /// </summary>
        internal Color Tint
        {
            get => _tint;
            set
            {
                _tint = value;
                _color = _tint * _alpha;
            } 
        }
        private Color _tint = Color.White;

        /// <summary>
        ///     The transparency of this QuaverSprite.
        /// </summary>
        internal float Alpha {
            get => _alpha;
            set
            {
                _alpha = value;
                _color = _tint * _alpha;
            } 
        }
        private float _alpha = 1f;

        /// <summary>
        ///     The color of this QuaverSprite.
        /// </summary>
        private Color _color = Color.White;

        /// <summary>
        ///     Draws the sprite to the screen.
        /// </summary>
        internal override void Draw()
        {
            //Draw itself if it is in the window
            //Old: GameBase.SpriteBatch.Draw(Image, GlobalRect, Tint);
            if (GraphicsHelper.RectangleIntercepts(GlobalRectangle, GameBase.WindowRectangle) && Visible)
            {
                GameBase.SpriteBatch.Draw(_image, _renderRectangle, null, _color, _rotation, _origin, SpriteEffect, 0f);
            }

            //Draw children
            base.Draw();
        }

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
            _renderRectangle.Width = (int)_originRectangle.Width;
            _renderRectangle.Height = (int)_originRectangle.Height;
        }
    }
}
