using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface
{
    internal class BarDisplay : Container
    {
        /// <summary>
        ///     The sprite-middle of every bar
        /// </summary>
        private Sprites.Sprite[] BarSpriteMiddle { get; set; }

        /// <summary>
        ///     The sprite-end of every bar
        /// </summary>
        private Sprites.Sprite[] BarSpriteEnd { get; set; }

        /// <summary>
        ///     The sprite-begin of every bar
        /// </summary>
        private Sprites.Sprite[] BarSpriteBegin { get; set; }

        /// <summary>
        ///     The scale for every bar sprite
        /// </summary>
        private float[] BarScale { get; set; }

        private Sprites.Sprite BarAxisMidBox { get; set; }

        private Sprites.Sprite BarAxisTopCorner { get; set; }
        private Sprites.Sprite BarAxisTopBox { get; set; }
        private Sprites.Sprite BarAxisTopCap { get; set; }

        private Sprites.Sprite BarAxisBotCorner { get; set; }
        private Sprites.Sprite BarAxisBotBox { get; set; }
        private Sprites.Sprite BarAxisBotCap { get; set; }

        private float BarSpacing { get; } = 2;
        private float BarDefaultSize { get; } = 4;

        private float Length { get; set; }
        private float SpriteScale { get; set; }

        // Constructor
        public BarDisplay(float sScale, float length, Color[] BarColors, bool Vertical = false)
        {
            var bsize = BarColors.Length;
            var sscale = BarDefaultSize * sScale;
            // Create QuaverContainer Size and variables
            Length = length;
            SpriteScale = sScale;
            Size.X.Offset = length;
            Size.Y.Offset = (((BarSpacing + BarDefaultSize) * bsize) + BarDefaultSize) * sScale;

            BarScale = new float[bsize];
            BarSpriteMiddle = new Sprites.Sprite[bsize];
            BarSpriteBegin = new Sprites.Sprite[bsize];
            BarSpriteEnd = new Sprites.Sprite[bsize];

            // Create Bar Axis

            // Bar Axis Bot
            BarAxisBotCorner = new Sprites.Sprite()
            {
                Image = GameBase.QuaverUserInterface.BarCorner,
                Size = new UDim2D(sscale,sscale),
                Parent = this
            };

            BarAxisBotBox = new Sprites.Sprite()
            {
                Size = new UDim2D(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2D(0, sscale),
                Parent = this
            };

            BarAxisBotCap = new Sprites.Sprite()
            {
                Image = GameBase.QuaverUserInterface.BarCap,
                Size = new UDim2D(sscale, sscale),
                Position = new UDim2D(0, BarAxisBotBox.Size.Y.Offset + sscale),
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Bar Axis Mid
            BarAxisMidBox = new Sprites.Sprite()
            {
                Size = new UDim2D(length - (2 * BarDefaultSize), sscale),
                Position = new UDim2D(BarAxisBotBox.Size.X.Offset, 0),
                Parent = this
            };

            //Bar Axis Topp
            BarAxisTopCorner = new Sprites.Sprite()
            {
                Image = GameBase.QuaverUserInterface.BarCorner,
                Rotation = 90,
                Size = new UDim2D(sscale, sscale),
                Position = new UDim2D(BarAxisMidBox.Position.X.Offset + BarAxisMidBox.Size.X.Offset, 0),
                Parent = this
            };

            BarAxisTopBox = new Sprites.Sprite()
            {
                Size = new UDim2D(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2D(BarAxisTopCorner.Position.X.Offset, sscale),
                Parent = this
            };

            BarAxisTopCap = new Sprites.Sprite()
            {
                Image = GameBase.QuaverUserInterface.BarCap,
                Position = new UDim2D(BarAxisTopBox.Position.X.Offset, BarAxisTopBox.Size.Y.Offset + sscale),
                Size = new UDim2D(sscale, sscale),
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Create Bar Sprites
            for (var i = 0; i < bsize; i++)
            {
                BarScale[i] = 0;

                BarSpriteBegin[i] = new Sprites.Sprite()
                {
                    Image = GameBase.QuaverUserInterface.BarCap,
                    Rotation = -90,
                    Tint = BarColors[i],
                    Position = new UDim2D(sscale, ((((BarSpacing + BarDefaultSize) * i) + BarDefaultSize) * sScale) + BarSpacing),
                    Size = new UDim2D(sscale, sscale),
                    Parent = this
                };

                BarSpriteMiddle[i] = new Sprites.Sprite()
                {
                    Tint = BarColors[i],
                    Position = new UDim2D(BarSpriteBegin[i].Position.X.Offset + BarSpriteBegin[i].Size.X.Offset, BarSpriteBegin[i].Position.Y.Offset),
                    Size = new UDim2D(sscale, sscale),
                    Parent = this
                };

                BarSpriteEnd[i] = new Sprites.Sprite()
                {
                    Image = GameBase.QuaverUserInterface.BarCap,
                    Rotation = 90,
                    Alignment = Alignment.TopRight,
                    Tint = BarColors[i],
                    Position = new UDim2D(sscale, 0),
                    Size = new UDim2D(sscale, sscale),
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
