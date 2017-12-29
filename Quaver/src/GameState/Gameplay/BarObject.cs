using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay
{
    class BarObject
    {
        /// <summary>
        ///     The bar's object from the receptor
        /// </summary>
        internal ulong OffsetFromReceptor { get; set; }

        /// <summary>
        ///     The Sprite of the bar
        /// </summary>
        internal Sprite BarSprite { get; set; }

        internal void Initialize(Drawable parent, float sizeY, float posY)
        {
            //Create bar
            BarSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                PositionY = posY,
                ScaleX = 1,
                SizeY = sizeY,
                Parent = parent
            };
        }

        internal void Destroy()
        {
            BarSprite.Destroy();
        }
    }
}
