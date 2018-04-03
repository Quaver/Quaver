using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface
{
    internal class QuaverBarDisplay : QuaverContainer
    {
        /// <summary>
        ///     The sprite-middle of every bar
        /// </summary>
        private Sprites.QuaverSprite[] BarQuaverSpriteMiddle { get; set; }

        /// <summary>
        ///     The sprite-end of every bar
        /// </summary>
        private Sprites.QuaverSprite[] BarQuaverSpriteEnd { get; set; }

        /// <summary>
        ///     The sprite-begin of every bar
        /// </summary>
        private Sprites.QuaverSprite[] BarQuaverSpriteBegin { get; set; }

        /// <summary>
        ///     The scale for every bar sprite
        /// </summary>
        private float[] BarScale { get; set; }

        private Sprites.QuaverSprite BarAxisMidBox { get; set; }

        private Sprites.QuaverSprite BarAxisTopCorner { get; set; }
        private Sprites.QuaverSprite BarAxisTopBox { get; set; }
        private Sprites.QuaverSprite BarAxisTopCap { get; set; }

        private Sprites.QuaverSprite BarAxisBotCorner { get; set; }
        private Sprites.QuaverSprite BarAxisBotBox { get; set; }
        private Sprites.QuaverSprite BarAxisBotCap { get; set; }

        private float BarSpacing { get; } = 2;
        private float BarDefaultSize { get; } = 4;

        private float Length { get; set; }
        private float SpriteScale { get; set; }

        // Constructor
        public QuaverBarDisplay(float sScale, float length, Color[] BarColors, bool Vertical = false)
        {
            var bsize = BarColors.Length;
            var sscale = BarDefaultSize * sScale;
            // Create QuaverContainer Size and variables
            Length = length;
            SpriteScale = sScale;
            Size.X.Offset = length;
            Size.Y.Offset = (((BarSpacing + BarDefaultSize) * bsize) + BarDefaultSize) * sScale;

            BarScale = new float[bsize];
            BarQuaverSpriteMiddle = new Sprites.QuaverSprite[bsize];
            BarQuaverSpriteBegin = new Sprites.QuaverSprite[bsize];
            BarQuaverSpriteEnd = new Sprites.QuaverSprite[bsize];

            // Create Bar Axis

            // Bar Axis Bot
            BarAxisBotCorner = new Sprites.QuaverSprite()
            {
                Image = GameBase.QuaverUserInterface.BarCorner,
                Size = new UDim2D(sscale,sscale),
                Parent = this
            };

            BarAxisBotBox = new Sprites.QuaverSprite()
            {
                Size = new UDim2D(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2D(0, sscale),
                Parent = this
            };

            BarAxisBotCap = new Sprites.QuaverSprite()
            {
                Image = GameBase.QuaverUserInterface.BarCap,
                Size = new UDim2D(sscale, sscale),
                Position = new UDim2D(0, BarAxisBotBox.Size.Y.Offset + sscale),
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Bar Axis Mid
            BarAxisMidBox = new Sprites.QuaverSprite()
            {
                Size = new UDim2D(length - (2 * BarDefaultSize), sscale),
                Position = new UDim2D(BarAxisBotBox.Size.X.Offset, 0),
                Parent = this
            };

            //Bar Axis Topp
            BarAxisTopCorner = new Sprites.QuaverSprite()
            {
                Image = GameBase.QuaverUserInterface.BarCorner,
                Rotation = 90,
                Size = new UDim2D(sscale, sscale),
                Position = new UDim2D(BarAxisMidBox.Position.X.Offset + BarAxisMidBox.Size.X.Offset, 0),
                Parent = this
            };

            BarAxisTopBox = new Sprites.QuaverSprite()
            {
                Size = new UDim2D(sscale, (((BarSpacing + BarDefaultSize) * bsize)) * sScale),
                Position = new UDim2D(BarAxisTopCorner.Position.X.Offset, sscale),
                Parent = this
            };

            BarAxisTopCap = new Sprites.QuaverSprite()
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

                BarQuaverSpriteBegin[i] = new Sprites.QuaverSprite()
                {
                    Image = GameBase.QuaverUserInterface.BarCap,
                    Rotation = -90,
                    Tint = BarColors[i],
                    Position = new UDim2D(sscale, ((((BarSpacing + BarDefaultSize) * i) + BarDefaultSize) * sScale) + BarSpacing),
                    Size = new UDim2D(sscale, sscale),
                    Parent = this
                };

                BarQuaverSpriteMiddle[i] = new Sprites.QuaverSprite()
                {
                    Tint = BarColors[i],
                    Position = new UDim2D(BarQuaverSpriteBegin[i].Position.X.Offset + BarQuaverSpriteBegin[i].Size.X.Offset, BarQuaverSpriteBegin[i].Position.Y.Offset),
                    Size = new UDim2D(sscale, sscale),
                    Parent = this
                };

                BarQuaverSpriteEnd[i] = new Sprites.QuaverSprite()
                {
                    Image = GameBase.QuaverUserInterface.BarCap,
                    Rotation = 90,
                    Alignment = Alignment.TopRight,
                    Tint = BarColors[i],
                    Position = new UDim2D(sscale, 0),
                    Size = new UDim2D(sscale, sscale),
                    Parent = BarQuaverSpriteMiddle[i]
                };
            }
        }

        public void UpdateBar(int index, float value, Color? color = null)
        {
            if (index < BarQuaverSpriteMiddle.Length)
            {
                BarScale[index] = value;
                BarQuaverSpriteMiddle[index].Size.X.Offset = (Length - (BarDefaultSize * SpriteScale * 4)) * BarScale[index];
                BarQuaverSpriteEnd[index].RecalculateRect();
                if (color != null)
                {
                    BarQuaverSpriteMiddle[index].Tint = (Color)color;
                    BarQuaverSpriteEnd[index].Tint = (Color)color;
                    BarQuaverSpriteBegin[index].Tint = (Color)color;
                }
            }
        }

        public float GetBarScale(int index)
        {
            if (index < BarQuaverSpriteMiddle.Length) return BarScale[index];
            else return 0;
        }
    }
}
