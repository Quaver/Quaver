using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.Mania.UI.Measures
{
    internal class ManiaBarObject
    {
        /// <summary>
        ///     The bar's object from the receptor
        /// </summary>
        internal ulong OffsetFromReceptor { get; set; }

        /// <summary>
        ///     The QuaverSprite of the bar
        /// </summary>
        internal Sprite BarSprite { get; set; }

        internal void Initialize(Drawable parent, float sizeY, float posY)
        {
            //Create bar
            BarSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Image = GameBase.LoadedSkin.StageTimingBar,
                Position = new UDim2D(0, posY),
                Size = new UDim2D(0, sizeY, 1, 0),
                Parent = parent
            };
        }

        internal void Destroy()
        {
            BarSprite.Destroy();
        }
    }
}
