using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class PlayfieldUI : IHelper
    {
        /// <summary>
        ///     The parent of every Playfield QuaverUserInterface Component
        /// </summary>
        public QuaverContainer QuaverContainer { get; private set; }

        /// <summary>
        ///     This displays the judging (MARV/PERF/GREAT/ect)
        /// </summary>
        private QuaverSprite JudgeQuaverSprite { get; set; }

        /// <summary>
        ///     The sprite for every Offset Indicator bar
        /// </summary>
        private QuaverSprite[] OffsetIndicatorsQuaverSprites { get; set; }

        /// <summary>
        ///     Bar images which display the player's current multiplier
        /// </summary>
        private QuaverSprite[] MultiplierBars { get; set; }

        /// <summary>
        ///     The player's health bar
        /// </summary>
        private QuaverSprite HealthBarOver { get; set; }

        /// <summary>
        ///     Used to reference the images for JudgeQuaverSprite
        /// </summary>
        private Texture2D[] JudgeImages { get; set; }

        /// <summary>
        ///     Reference to the size each judge image is
        /// </summary>
        private Vector2[] JudgeSizes { get; set; }

        /// <summary>
        ///     The text displaying combo
        /// </summary>
        private QuaverTextbox ComboQuaverText { get; set; }

        /// <summary>
        ///     When the JudgeQuaverSprite gets updated, it'll update JudgeQuaverSprite.PositionY to this variable.
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
        ///     The alpha of the entire QuaverUserInterface set. Will turn invisible if the set is not being updated.
        /// </summary>
        private double SpriteAlphaHold { get; set; }

        /// <summary>
        ///     Total number of multiplier bars which are active
        /// </summary>
        private int ActiveMultiplierBars { get; set; }

        internal float PlayfieldSize { get; set; }

        public void Draw()
        {
            QuaverContainer.Draw();
        }

        public void Initialize(IGameState state)
        {
            // Reference Variables
            SpriteAlphaHold = 0;
            CurrentOffsetObjectIndex = 0;
            ActiveMultiplierBars = 0;

            // Create Judge QuaverSprite/References
            JudgeImages = new Texture2D[6]
            {
                GameBase.LoadedSkin.JudgeMarv,
                GameBase.LoadedSkin.JudgePerf,
                GameBase.LoadedSkin.JudgeGreat,
                GameBase.LoadedSkin.JudgeGood,
                GameBase.LoadedSkin.JudgeOkay,
                GameBase.LoadedSkin.JudgeMiss
            };

            JudgeSizes = new Vector2[6];
            for (var i = 0; i < 6; i++)
            {
                //todo: replace 40 with skin.ini value
                JudgeSizes[i] = new Vector2(JudgeImages[i].Width, JudgeImages[i].Height) * 40f * GameBase.WindowUIScale / JudgeImages[i].Height;
            }
            JudgeHitOffset = -5f * GameBase.WindowUIScale;

            // Create QuaverContainer
            QuaverContainer = new QuaverContainer()
            {
                Size = new UDim2D(PlayfieldSize, 0, 0, 1),
                Alignment = Alignment.MidCenter
            };

            // TODO: add judge scale
            JudgeQuaverSprite = new QuaverSprite()
            {
                Size = new UDim2D(JudgeSizes[0].X, JudgeSizes[0].Y),
                Alignment = Alignment.MidCenter,
                Image = JudgeImages[0],
                Parent = QuaverContainer,
                Alpha = 0
            };

            // Create Combo Text
            ComboQuaverText = new QuaverTextbox()
            {
                Size = new UDim2D(100 * GameBase.WindowUIScale, 20 * GameBase.WindowUIScale),
                Position = new UDim2D(0, 45 * GameBase.WindowUIScale),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.TopCenter,
                Text = "0x",
                TextScale = GameBase.WindowUIScale,
                Font = Fonts.Medium16,
                Parent = QuaverContainer,
                Alpha = 0
            };

            // Create Offset Gauge
            var offsetGaugeBoundary = new QuaverContainer()
            {
                Size = new UDim2D(220 * GameBase.WindowUIScale, 10 * GameBase.WindowUIScale),
                Position = new UDim2D(0, 30 * GameBase.WindowUIScale),
                Alignment = Alignment.MidCenter,
                Parent = QuaverContainer
            };

            //todo: offsetGaugeBoundary.SizeX with a new size. Right now the offset gauge is the same size as the hitwindow
            //OffsetGaugeSize = offsetGaugeBoundary.SizeX / (GameplayReferences.PressWindowLatest * 2 * GameBase.WindowUIScale);
            OffsetGaugeSize = offsetGaugeBoundary.SizeX / (200 * GameBase.WindowUIScale);

            OffsetIndicatorsQuaverSprites = new QuaverSprite[OffsetIndicatorSize];
            for (var i = 0; i < OffsetIndicatorSize; i++)
            {
                OffsetIndicatorsQuaverSprites[i] = new QuaverSprite()
                {
                    Parent = offsetGaugeBoundary,
                    Size = new UDim2D(4, 0, 0, 1),
                    Alignment = Alignment.MidCenter,
                    Alpha = 0
                };
            }

            var offsetGaugeMiddle = new QuaverSprite()
            {
                Size = new UDim2D(2, 0, 0, 1),
                Alignment = Alignment.MidCenter,
                Parent = offsetGaugeBoundary
            };

            // Create Health Bar
            var healthMultiplierBoundary = new QuaverContainer()
            {
                Size = new UDim2D(PlayfieldSize - 4, 20 * GameBase.WindowUIScale),
                PosY = Config.ConfigManager.HealthBarPositionTop ? 2 : -2,
                Alignment = Config.ConfigManager.HealthBarPositionTop ? Alignment.TopCenter : Alignment.BotCenter,
                Parent = QuaverContainer
            };

            var healthBarUnder = new QuaverSprite()
            {
                Size = new UDim2D(0, 10 * GameBase.WindowUIScale -1, 1, 0),
                Alignment = Config.ConfigManager.HealthBarPositionTop ? Alignment.TopCenter : Alignment.BotCenter,
                Parent = healthMultiplierBoundary
            };

            HealthBarOver = new QuaverSprite()
            {
                Size = new UDim2D(-2, -2, 1, 1),
                PosX = 1,
                Parent = healthBarUnder,
                Alignment = Alignment.MidLeft,
                Tint = Color.Green
            };

            // Create Multiplier Bars
            MultiplierBars = new QuaverSprite[15];
            for (var i = 0; i < 15; i++)
            {
                MultiplierBars[i] = new QuaverSprite()
                {
                    Size = new UDim2D(14 * GameBase.WindowUIScale, 10 * GameBase.WindowUIScale -1),
                    PosX = (i-7.5f) * 16 * GameBase.WindowUIScale,
                    PosY = Config.ConfigManager.HealthBarPositionTop ? 10 * GameBase.WindowUIScale + 1 : 0,
                    Alignment = Alignment.TopCenter,
                    Image = GameBase.QuaverUserInterface.HollowBox,
                    Parent = healthMultiplierBoundary
                };
            }
        }

        public void UnloadContent()
        {
            QuaverContainer.Destroy();
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
            foreach (var sprite in OffsetIndicatorsQuaverSprites)
            {
                sprite.Alpha = GraphicsHelper.Tween(0, sprite.Alpha, tween / 30);
            }

            // Update Judge Alpha
            JudgeQuaverSprite.PosY = GraphicsHelper.Tween(0, JudgeQuaverSprite.PosY, tween / 2);
            if (SpriteAlphaHold > 500 && PriorityJudgeLength <= 0)
            {
                JudgeQuaverSprite.Alpha = GraphicsHelper.Tween(0, JudgeQuaverSprite.Alpha, tween / 10);
                ComboQuaverText.Alpha = GraphicsHelper.Tween(0, ComboQuaverText.Alpha, tween / 10);
            }

            //Update QuaverContainer
            QuaverContainer.Update(dt);
        }

        /// <summary>
        ///     Update Judge Image and Combo/Note ms offset
        /// </summary>
        /// <param name="index"></param>
        /// <param name="combo"></param>
        /// <param name="release"></param>
        /// <param name="offset"></param>
        internal void UpdateJudge(int index, int combo, bool release = false, double? offset = null)
        {
            //TODO: add judge scale
            ComboQuaverText.Text = combo + "x";
            ComboQuaverText.Alpha = 1;
            JudgeQuaverSprite.Alpha = 1;
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
                JudgeQuaverSprite.SizeX = JudgeSizes[index].X;
                JudgeQuaverSprite.SizeY = JudgeSizes[index].Y;
                JudgeQuaverSprite.Image = JudgeImages[index];
                JudgeQuaverSprite.PosY = JudgeHitOffset;
                JudgeQuaverSprite.Update(0);
            }

            if (index != 5 && !release && offset != null)
            {
                CurrentOffsetObjectIndex++;
                if (CurrentOffsetObjectIndex >= OffsetIndicatorSize) CurrentOffsetObjectIndex = 0;
                OffsetIndicatorsQuaverSprites[CurrentOffsetObjectIndex].Tint = GameBase.LoadedSkin.JudgeColors[index];
                OffsetIndicatorsQuaverSprites[CurrentOffsetObjectIndex].PosX = -(float)offset * OffsetGaugeSize;
                OffsetIndicatorsQuaverSprites[CurrentOffsetObjectIndex].Alpha = 0.5f;
                OffsetIndicatorsQuaverSprites[CurrentOffsetObjectIndex].Update(0);
            }
        }

        /// <summary>
        ///     Update Multiplier bars
        ///     todo: create cool fx
        /// </summary>
        /// <param name="total"></param>
        internal void UpdateMultiplierBars(int total)
        {
            //total should be between or equal to 0 and 15
            if (total > 15 || total < 0) return;

            // If a new bar turns active, do fx and stuff
            if (total > ActiveMultiplierBars)
            {
                ActiveMultiplierBars = total;
                MultiplierBars[total-1].Image = GameBase.QuaverUserInterface.BlankBox;
            }
            // If a new bar turns inactive
            else if (total < ActiveMultiplierBars)
            {
                ActiveMultiplierBars = total;
                for (var i = 1; i <= 15; i++)
                {
                    if (i > total)
                        MultiplierBars[i-1].Image = GameBase.QuaverUserInterface.HollowBox;
                    else
                        MultiplierBars[i-1].Image = GameBase.QuaverUserInterface.BlankBox;
                }
            }
        }

        /// <summary>
        ///     Update the health bar
        /// </summary>
        /// <param name="health"></param>
        internal void UpdateHealthBar(double health)
        {
            HealthBarOver.ScaleX = (float)health / 100;
        }
    }
}
