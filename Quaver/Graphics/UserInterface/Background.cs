using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface
{
    internal class Background : Sprite
    {
        private Sprite BrightnessEffect { get; set; }

        internal int Dim { get; private set; }

        internal Background(Texture2D image, int dim = 100)
        {
            Image = image;            
            Dim = dim;
            Size = new UDim2D(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height);

            BrightnessEffect = new Sprite
            {
                Tint = Color.Black,
                Parent = this,
                Size = new UDim2D(GameBase.WindowRectangle.Width, GameBase.WindowRectangle.Height),
                Alpha = Dim / 100f
            };
        }

        internal void ChangeDim(int dim)
        {
            Dim = dim;
            BrightnessEffect.Alpha = Dim / 100f;
        }
    }
}