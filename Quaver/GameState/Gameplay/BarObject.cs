﻿using Quaver.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;

namespace Quaver.GameState.Gameplay
{
    class BarObject
    {
        /// <summary>
        ///     The bar's object from the receptor
        /// </summary>
        internal ulong OffsetFromReceptor { get; set; }

        /// <summary>
        ///     The QuaverSprite of the bar
        /// </summary>
        internal QuaverSprite BarQuaverSprite { get; set; }

        internal void Initialize(Drawable parent, float sizeY, float posY)
        {
            //Create bar
            BarQuaverSprite = new QuaverSprite()
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
            BarQuaverSprite.Destroy();
        }
    }
}
