using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;
using Quaver.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class PlayfieldUI : IHelper
    {
        /// <summary>
        ///     This displays the judging (MARV/PERF/GREAT/ect)
        /// </summary>
        private Sprite JudgeSprite { get; set; }

        /// <summary>
        ///     Used to reference the images for JudgeSprite
        /// </summary>
        private Texture2D[] JudgeImages { get; set; }

        /// <summary>
        ///     When the JudgeSprite gets updated, it'll update JudgeSprite.PositionY to this variable.
        /// </summary>
        private float JudgeHitOffset { get; set; }

        /// <summary>
        ///     The judge image that has priority other judge imaages that is displayed. Worse judgement has more priority (MISS > BAD > OKAY... ect)
        /// </summary>
        private int PriorityJudgeImage { get; set; } = 0;

        /// <summary>
        ///     How long the prioritized judge image will be displayed for
        /// </summary>
        private double PriorityJudgeLength { get; set; }

        /// <summary>
        ///     Reference to the size each judge image is
        /// </summary>
        private Vector2[] JudgeSizes { get; set; }

        /// <summary>
        ///     The Parent of every Offset Gauge component
        /// </summary>
        private Boundary OffsetGaugeBoundary { get; set; }

        /// <summary>
        ///     The white bar in the middle of the offset gauage
        /// </summary>
        private Sprite OffsetGaugeMiddle { get; set; }

        /// <summary>
        ///     The bars which indicate how off players are from the receptor
        /// </summary>
        private const int OffsetIndicatorSize = 32;

        /// <summary>
        ///     The size of the Offset Gauage
        /// </summary>
        private float OffsetGaugeSize { get; set; }

        /// <summary>
        ///     Index of the current Offset Indicator. It will cycle through whatever the indicator size is to pool the indicator sprites
        /// </summary>
        private int CurrentOffsetObjectIndex { get; set; }

        /// <summary>
        ///     The sprite for every Offset Indicator bar
        /// </summary>
        private Sprite[] OffsetIndicatorsSprites { get; set; }

        /// <summary>
        ///     The text displaying combo
        /// </summary>
        private TextBoxSprite ComboText { get; set; }

        /// <summary>
        ///     The alpha of the entire UI set. Will turn invisible if the set is not being updated.
        /// </summary>
        private double SpriteAlphaHold { get; set; }

        /// <summary>
        ///     The parent of every Playfield UI Component
        /// </summary>
        public Boundary Boundary { get; private set; }

        public void Draw()
        {
            Boundary.Draw();
        }

        public void Initialize(IGameState state)
        {
            // Reference Variables
            SpriteAlphaHold = 0;
            CurrentOffsetObjectIndex = 0;

            // Create Judge Sprite/References
            JudgeImages = new Texture2D[6]
            {
                GameBase.LoadedSkin.JudgeMarv,
                GameBase.LoadedSkin.JudgePerfect,
                GameBase.LoadedSkin.JudgeGreat,
                GameBase.LoadedSkin.JudgeGood,
                GameBase.LoadedSkin.JudgeBad,
                GameBase.LoadedSkin.JudgeMiss
            };

            JudgeSizes = new Vector2[6];
            for (var i = 0; i < 6; i++)
            {
                //todo: replace 40 with skin.ini value
                JudgeSizes[i] = new Vector2(JudgeImages[i].Width, JudgeImages[i].Height) * 40f * GameBase.WindowYRatio / JudgeImages[i].Height;
            }
            JudgeHitOffset = -5f * GameBase.WindowYRatio;

            // Create Boundary
            Boundary = new Boundary()
            {
                SizeX = GameplayReferences.PlayfieldSize,
                ScaleY = 1,
                Alignment = Alignment.MidCenter
            };

            // TODO: add judge scale
            JudgeSprite = new Sprite()
            {
                Size = JudgeSizes[0],
                Alignment = Alignment.MidCenter,
                Image = JudgeImages[0],
                Parent = Boundary,
                Alpha = 0
            };

            // Create Combo Text
            ComboText = new TextBoxSprite()
            {
                SizeX = 100 * GameBase.WindowYRatio,
                SizeY = 20 * GameBase.WindowYRatio,
                PositionY = 45 * GameBase.WindowYRatio,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.TopCenter,
                Text = "0x",
                TextScale = GameBase.WindowYRatio,
                Font = Fonts.Medium16,
                Parent = Boundary,
                Alpha = 0
            };

            // Create Offset Gauge
            OffsetGaugeBoundary = new Boundary()
            {
                SizeX = 220 * GameBase.WindowYRatio,
                SizeY = 10 * GameBase.WindowYRatio,
                PositionY = 30 * GameBase.WindowYRatio,
                Alignment = Alignment.MidCenter,
                Parent = Boundary
            };

            //todo: OffsetGaugeBoundary.SizeX with a new size. Right now the offset gauge is the same size as the hitwindow
            OffsetGaugeSize = OffsetGaugeBoundary.SizeX / (GameplayReferences.PressWindowLatest * 2 * GameBase.WindowYRatio);

            OffsetIndicatorsSprites = new Sprite[OffsetIndicatorSize];
            for (var i = 0; i < OffsetIndicatorSize; i++)
            {
                OffsetIndicatorsSprites[i] = new Sprite()
                {
                    Parent = OffsetGaugeBoundary,
                    ScaleY = 1,
                    SizeX = 4,
                    Alignment = Alignment.MidCenter,
                    PositionX = 0,
                    Alpha = 0
                };
            }

            OffsetGaugeMiddle = new Sprite()
            {
                SizeX = 2,
                ScaleY = 1,
                Alignment = Alignment.MidCenter,
                Parent = OffsetGaugeBoundary
            };
        }

        public void UnloadContent()
        {
            Boundary.Destroy();
        }

        public void Update(double dt)
        {
            // Update the delta time tweening variable for animation.
            SpriteAlphaHold += dt;
            PriorityJudgeLength -= dt;
            if (PriorityJudgeLength <= 0)
            {
                PriorityJudgeLength = 0;
                PriorityJudgeImage = 0;
            }
            var tween = Math.Min(dt / 30, 1);

            // Update Offset Indicators
            foreach (var sprite in OffsetIndicatorsSprites)
            {
                sprite.Alpha = Util.Tween(0, sprite.Alpha, tween / 30);
            }

            // Update Judge Alpha
            JudgeSprite.PositionY = Util.Tween(0, JudgeSprite.PositionY, tween / 2);
            if (SpriteAlphaHold > 500 && PriorityJudgeLength <= 0)
            {
                JudgeSprite.Alpha = Util.Tween(0, JudgeSprite.Alpha, tween / 10);
                ComboText.Alpha = Util.Tween(0, ComboText.Alpha, tween / 10);
            }

            //Update Boundary
            Boundary.Update(dt);
        }

        public void UpdateJudge(int index, int combo, bool release = false, double? offset = null)
        {
            //TODO: add judge scale
            ComboText.Text = combo + "x";
            ComboText.Alpha = 1;
            JudgeSprite.Alpha = 1;
            SpriteAlphaHold = 0;

            if (index >= PriorityJudgeImage || PriorityJudgeLength <= 0)
            {
                // Priority Judge Image to show
                if (index < 2) PriorityJudgeLength = 10;
                else if (index == 2) PriorityJudgeLength = 50;
                else if (index == 3) PriorityJudgeLength = 100;
                else PriorityJudgeLength = 500;
                PriorityJudgeImage = index;

                // Update judge sprite
                JudgeSprite.Size = JudgeSizes[index];
                JudgeSprite.Image = JudgeImages[index];
                JudgeSprite.PositionY = JudgeHitOffset;
            }

            if (index != 5 && !release && offset != null)
            {
                CurrentOffsetObjectIndex++;
                if (CurrentOffsetObjectIndex >= OffsetIndicatorSize) CurrentOffsetObjectIndex = 0;
                OffsetIndicatorsSprites[CurrentOffsetObjectIndex].Tint = CustomColors.JudgeColors[index];
                OffsetIndicatorsSprites[CurrentOffsetObjectIndex].PositionX = -(float)offset * OffsetGaugeSize;
                OffsetIndicatorsSprites[CurrentOffsetObjectIndex].Alpha = 0.5f;
            }
        }
    }
}
