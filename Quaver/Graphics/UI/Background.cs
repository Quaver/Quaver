using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.Graphics.UI
{
    internal class Background : Sprite
    {
        private Sprite BrightnessEffect { get; set; }

        internal int Dim { get; private set; }

        internal Background(Texture2D image, int dim = 100)
        {
            Image = image;            
            Dim = dim;
            Size = new UDim2D(GameBase.WindowRectangle.Width + 100, GameBase.WindowRectangle.Height + 100);
            
            BrightnessEffect = new Sprite
            {
                Tint = Color.Black,
                Parent = this,
                Size = new UDim2D(SizeX, SizeY),
                Alpha = Dim / 100f
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            PerformParallaxEffect();        
            base.Update(dt);
        }

        /// <summary>
        ///     Changes the dim of the the background.
        /// </summary>
        /// <param name="dim"></param>
        internal void ChangeDim(int dim)
        {
            Dim = dim;
            BrightnessEffect.Alpha = Dim / 100f;
        }

        /// <summary>
        ///     Adds a parallax effect for the background.
        /// </summary>
        private void PerformParallaxEffect()
        {
            if (!ConfigManager.BackgroundParallax.Value)
                return;
            
            // Parallax
            var mousePos = GameBase.MouseState.Position;

            PosY = (mousePos.Y - GameBase.WindowRectangle.Height / 2f) / 60f - 50;
            PosX = (mousePos.X - GameBase.WindowRectangle.Width / 2f) / 60f - 50;
        }
    }
}