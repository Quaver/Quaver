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
        ///     Will get toggled on if BarScale gets changed
        /// </summary>
        private bool BarChanged { get; set; } = true;

        /// <summary>
        ///     The sprite-middle of every bar
        /// </summary>
        public Sprite[] BarSpriteMiddle { get; set; }

        /// <summary>
        ///     The sprite-end of every bar
        /// </summary>
        public Sprite[] BarSpriteEnd { get; set; }

        /// <summary>
        ///     The sprite-begin of every bar
        /// </summary>
        public Sprite[] BarSpriteBegin { get; set; }

        /// <summary>
        ///     The scale for every bar sprite
        /// </summary>
        public float[] BarScale {
            get
            {
                return _barScale;
            }
            set
            {
                _barScale = value;
                BarChanged = true;
            }
        }
        private float[] _barScale { get; set; }

        private Sprite BarAxisMidBox { get; set; }

        private Sprite BarAxisTopBox { get; set; }
        private Sprite BarAxisTopCap { get; set; }

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
            // Create Boundary Size and variables
            Length = length;
            SpriteScale = sScale;
            SizeX = length;
            SizeY = (((BarSpacing + BarDefaultSize) * bsize) + BarDefaultSize) * sScale;

            BarScale = new float[bsize];
            _barScale = new float[bsize];
            BarSpriteMiddle = new Sprite[bsize];
            BarSpriteBegin = new Sprite[bsize];
            BarSpriteEnd = new Sprite[bsize];

            // Create Bar Axis
            BarAxisBotBox = new Sprite()
            {
                SizeX = BarDefaultSize * sScale,
                SizeY = (((BarSpacing + BarDefaultSize) * bsize) + BarDefaultSize) * sScale,
                Parent = this

            };

            BarAxisBotCap = new Sprite()
            {
                Image = GameBase.UI.BarCap,
                Size = Vector2.One * BarDefaultSize * sScale,
                PositionY = BarAxisBotBox.SizeY,
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            BarAxisMidBox = new Sprite()
            {
                SizeX = length - (2 * BarDefaultSize),
                SizeY = BarDefaultSize * sScale,
                PositionX = BarAxisBotBox.SizeX,
                Parent = this
            };

            BarAxisTopBox = new Sprite()
            {
                PositionX = length - (BarDefaultSize * sScale),
                SizeX = BarDefaultSize * sScale,
                SizeY = BarAxisBotBox.SizeY,
                Parent = this

            };

            BarAxisTopCap = new Sprite()
            {
                Image = GameBase.UI.BarCap,
                PositionX = BarAxisTopBox.PositionX,
                Size = Vector2.One * BarDefaultSize * sScale,
                PositionY = BarAxisTopBox.SizeY,
                SpriteEffect = SpriteEffects.FlipVertically,
                Parent = this
            };

            // Create Bar Sprites
            for (var i = 0; i < bsize; i++)
            {
                BarSpriteBegin[i] = new Sprite()
                {
                    Image = GameBase.UI.BarCap,
                    Rotation = -90,
                    Tint = BarColors[i],
                    PositionX = BarDefaultSize * sScale,
                    PositionY = ((((BarSpacing + BarDefaultSize) * i) + BarDefaultSize) * sScale) + BarSpacing,
                    Size = Vector2.One * BarDefaultSize * sScale,
                    Parent = this
                };

                BarSpriteMiddle[i] = new Sprite()
                {
                    Tint = BarColors[i],
                    PositionX = BarSpriteBegin[i].PositionX + BarSpriteBegin[i].SizeX,
                    PositionY = BarSpriteBegin[i].PositionY,
                    SizeX = 0,
                    SizeY = BarDefaultSize * sScale,
                    Parent = this
                };

                BarSpriteEnd[i] = new Sprite()
                {
                    Image = GameBase.UI.BarCap,
                    Rotation = 90,
                    Alignment = Alignment.TopRight,
                    Tint = BarColors[i],
                    PositionX = BarDefaultSize * sScale,
                    Size = Vector2.One * BarDefaultSize * sScale,
                    Parent = BarSpriteMiddle[i]
                };
            }
        }

        public override void Update(double dt)
        {
            if (BarChanged) RecalculateBars();
            base.Update(dt);
        }

        private void RecalculateBars()
        {
            BarChanged = false;
            for (var i = 0; i < BarSpriteMiddle.Length; i++)
            {
                BarSpriteMiddle[i].SizeX = Length - (BarDefaultSize * SpriteScale * 4);
            }
        }
    }
}
