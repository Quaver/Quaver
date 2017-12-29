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
        public ulong OffsetFromReceptor { get; set; }

        /// <summary>
        ///     The Sprite of the bar
        /// </summary>
        public Sprite BarSprite { get; set; }

        /// <summary>
        ///     The parent of the bar object
        /// </summary>
        public Drawable ParentContainer { get; set; }

        /// <summary>
        ///     The position of the bar
        /// </summary>
        public float PositionY { get; set; }

        public void Initialize(Drawable parent, float sizex, float sizey)
        {
            //Create bar
            BarSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                PositionY = PositionY,
                SizeX = sizex,
                SizeY = sizey,
                Parent = ParentContainer
            };
        }

        public void Destroy()
        {
            BarSprite.Destroy();
        }
    }
}
