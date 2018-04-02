using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Graphics.Sprite
{
    internal class BarDisplay : Boundary
    {
        /// <summary>
        ///     The sprite-middle of every bar
        /// </summary>
        private Sprite[] BarSpriteMiddle { get; set; }

        /// <summary>
        ///     The sprite-end of every bar
        /// </summary>
        private Sprite[] BarSpriteEnd { get; set; }

        /// <summary>
        ///     The sprite-begin of every bar
        /// </summary>
        private Sprite[] BarSpriteBegin { get; set; }

        /// <summary>
        ///     The scale for every bar sprite
        /// </summary>
        private float[] BarScale { get; set; }

        private Sprite BarAxisMidBox { get; set; }

        private Sprite BarAxisTopCorner { get; set; }
        private Sprite BarAxisTopBox { get; set; }
        private Sprite BarAxisTopCap { get; set; }

        private Sprite BarAxisBotCorner { get; set; }
        private Sprite BarAxisBotBox { get; set; }
        private Sprite BarAxisBotCap { get; set; }

        private float BarSpacing { get; } = 2;
        private float BarDefaultSize { get; } = 4;

        private float Length { get; set; }
        private float SpriteScale { get; set; }

        // Constructor
        public BarDisplay(float sScale, float length, Color[] BarColors, bool Vertical = false)
        {
            var bsize = BarColors.Length;
            var sscale = BarDefaultSize * sScale;
            // Create Boundary Size and variables
            Length = length;
            SpriteScale = sScale;
            Size.X.Offset = length;
            Size.Y.Offset = (((BarSpacing + BarDefaultSize) * bsize) + BarDefaultSize) * sScale;

            BarScale = new float[bsize];
            BarSpriteMiddle = new Sprite[bsize];
            BarSpriteBegin = new Sprite[bsize];
            BarSpriteEnd = new Sprite[bsize];

            // Create Bar Axis

            // Bar Axis Bot
            BarAxisBotCorner = new Sprite()
            {
                Image = GameBase.UI.BarCorner,
                Size = new UDim2(sscale,sscale),
                Parent = this
            };

            BarAxisBotBox = new Sprite()
            {
                Size = new UDim2(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2(0, sscale),
                Parent = this
            };

            BarAxisBotCap = new Sprite()
            {
                Image = GameBase.UI.BarCap,
                Size = new UDim2(sscale, sscale),
                Position = new UDim2(0, BarAxisBotBox.Size.Y.Offset + sscale),
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Bar Axis Mid
            BarAxisMidBox = new Sprite()
            {
                Size = new UDim2(length - (2 * BarDefaultSize), sscale),
                Position = new UDim2(BarAxisBotBox.Size.X.Offset, 0),
                Parent = this
            };

            //Bar Axis Topp
            BarAxisTopCorner = new Sprite()
            {
                Image = GameBase.UI.BarCorner,
                Rotation = 90,
                Size = new UDim2(sscale, sscale),
                Position = new UDim2(BarAxisMidBox.Position.X.Offset + BarAxisMidBox.Size.X.Offset, 0),
                Parent = this
            };

            BarAxisTopBox = new Sprite()
            {
                Size = new UDim2(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2(BarAxisTopCorner.Position.X.Offset, sscale),
                Parent = this
            };

            BarAxisTopCap = new Sprite()
            {
                Image = GameBase.UI.BarCap,
                Position = new UDim2(BarAxisTopBox.Position.X.Offset, BarAxisTopBox.Size.Y.Offset + sscale),
                Size = new UDim2(sscale, sscale),
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Create Bar Sprites
            for (var i = 0; i < bsize; i++)
            {
                BarScale[i] = 0;

                BarSpriteBegin[i] = new Sprite()
                {
                    Image = GameBase.UI.BarCap,
                    Rotation = -90,
                    Tint = BarColors[i],
                    Position = new UDim2(sscale, ((((BarSpacing + BarDefaultSize) * i) + BarDefaultSize) * sScale) + BarSpacing),
                    Size = new UDim2(sscale, sscale),
                    Parent = this
                };

                BarSpriteMiddle[i] = new Sprite()
                {
                    Tint = BarColors[i],
                    Position = new UDim2(BarSpriteBegin[i].Position.X.Offset + BarSpriteBegin[i].Size.X.Offset, BarSpriteBegin[i].Position.Y.Offset),
                    Size = new UDim2(sscale, sscale),
                    Parent = this
                };

                BarSpriteEnd[i] = new Sprite()
                {
                    Image = GameBase.UI.BarCap,
                    Rotation = 90,
                    Alignment = Alignment.TopRight,
                    Tint = BarColors[i],
                    Position = new UDim2(sscale, 0),
                    Size = new UDim2(sscale, sscale),
                    Parent = BarSpriteMiddle[i]
                };
            }
        }

        public void UpdateBar(int index, float value, Color? color = null)
        {
            if (index < BarSpriteMiddle.Length)
            {
                BarScale[index] = value;
                BarSpriteMiddle[index].Size.X.Offset = (Length - (BarDefaultSize * SpriteScale * 4)) * BarScale[index];
                BarSpriteEnd[index].RecalculateRect();
                if (color != null)
                {
                    BarSpriteMiddle[index].Tint = (Color)color;
                    BarSpriteEnd[index].Tint = (Color)color;
                    BarSpriteBegin[index].Tint = (Color)color;
                }
            }
        }

        public float GetBarScale(int index)
        {
            if (index < BarSpriteMiddle.Length) return BarScale[index];
            else return 0;
        }
    }
}
