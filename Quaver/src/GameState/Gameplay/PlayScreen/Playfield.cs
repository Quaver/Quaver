﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState.States;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Graphics.Text;

using Quaver.Skins;
using Quaver.Utility;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    internal class Playfield : IHelper
    {
        /// <summary>
        ///     The size of the playfield padding.
        /// </summary>
        private int PlayfieldPadding { get; set; }

        /// <summary>
        ///     The padding of the receptors.
        /// </summary>
        private int ReceptorPadding { get; set; }

        /// <summary>
        ///     The receptor sprites.
        /// </summary>
        private Sprite[] Receptors { get; set; }

        /// <summary>
        ///     The target size for each receptors.
        /// </summary>
        private float[] ReceptorTargetSize { get; set; }

        /// <summary>
        ///     The current size for each receptors. Used for animation.
        /// </summary>
        private float[] ReceptorCurrentSize { get; set; }

        /// <summary>
        ///     The first layer of the playfield. Used to render receptors/FX
        /// </summary>
        private Boundary BoundaryUnder { get; set; }

        /// <summary>
        ///     The second layer of the playfield. Used to render judge/HitBurst
        /// </summary>
        private Boundary BoundaryOver { get; set; }

        /// <summary>
        ///     The background mask of the playfield.
        /// </summary>
        private Sprite BgMask { get; set; }

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

        private int PriorityJudgeImage { get; set; } = 0;
        private double PriorityJudgeLength { get; set; }

        private Vector2[] JudgeSizes { get; set; }

        private Boundary OffsetGaugeBoundary { get; set; }
        private Sprite OffsetGaugeMiddle { get; set; }
        private const int OffsetIndicatorSize = 32;
        private float OffsetGaugeSize { get; set; }

        private int CurrentOffsetObjectIndex { get; set; }
        private Sprite[] OffsetIndicatorsSprites { get; set; }

        private TextBoxSprite ComboText { get; set; }
        private double AlphaHold { get; set; }

        /// <summary>
        ///     Initializes necessary playfield variables for gameplay.
        /// </summary>
        public void Initialize(IGameState state)
        {
            PlayScreenState playScreen = (PlayScreenState)state;
            //PlayScreen = playScreen;

            // Set default reference variables
            GameplayReferences.ReceptorXPosition = new float[GameBase.SelectedBeatmap.Qua.KeyCount];
            AlphaHold = 0;
            CurrentOffsetObjectIndex = 0;

            // Calculate skin reference variables.
            GameplayReferences.PlayfieldObjectSize = (int)(GameBase.LoadedSkin.ColumnSize * GameBase.WindowYRatio);

            // Calculate skin reference variables - Set Object size based on key count as well.
            if (GameBase.SelectedBeatmap.Qua.KeyCount == 4)
                GameplayReferences.PlayfieldObjectSize = (int)(GameBase.LoadedSkin.ColumnSize * GameBase.WindowYRatio);
            else if (GameBase.SelectedBeatmap.Qua.KeyCount == 7)
                GameplayReferences.PlayfieldObjectSize = (int)(GameBase.LoadedSkin.ColumnSize7K * GameBase.WindowYRatio);

            PlayfieldPadding = (int) (GameBase.LoadedSkin.BgMaskPadding * GameBase.WindowYRatio);
            ReceptorPadding = (int)(GameBase.LoadedSkin.NotePadding * GameBase.WindowYRatio);
            GameplayReferences.PlayfieldSize = ((GameplayReferences.PlayfieldObjectSize + ReceptorPadding) * GameBase.SelectedBeatmap.Qua.KeyCount) + (PlayfieldPadding * 2) - ReceptorPadding;

            // Calculate Config stuff
            GameplayReferences.ReceptorYOffset = Config.Configuration.DownScroll ? (int)GameBase.Window.Z + (int)GameBase.Window.Y - GameBase.LoadedSkin.ReceptorYOffset - GameplayReferences.PlayfieldObjectSize : GameBase.LoadedSkin.ReceptorYOffset;

            // Create playfield boundary
            BoundaryUnder = new Boundary()
            {
                Size = new Vector2(GameplayReferences.PlayfieldSize, GameBase.Window.Z),
                Alignment = Alignment.TopCenter
            };

            BoundaryOver = new Boundary()
            {
                Size = new Vector2(GameplayReferences.PlayfieldSize, GameBase.Window.Z),
                Alignment = Alignment.TopCenter
            };

            // Create BG Mask
            BgMask = new Sprite()
            {
                //Image = GameBase.LoadedSkin.ColumnBgMask,
                Tint = Color.Black, //todo: remove
                Alpha = 0.8f, //todo: remove
                Parent = BoundaryUnder,
                Scale = Vector2.One
            };

            // Create Receptors
            Receptors = new Sprite[GameBase.SelectedBeatmap.Qua.KeyCount];
            ReceptorCurrentSize = new float[GameBase.SelectedBeatmap.Qua.KeyCount];
            ReceptorTargetSize = new float[GameBase.SelectedBeatmap.Qua.KeyCount];

            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.KeyCount; i++)
            {
                ReceptorCurrentSize[i] = 1;
                ReceptorTargetSize[i] = 1;
            }

            for (var i = 0; i < Receptors.Length; i++)
            {
                // Set ReceptorXPos 
                GameplayReferences.ReceptorXPosition[i] = ((GameplayReferences.PlayfieldObjectSize + ReceptorPadding) * i) + PlayfieldPadding;

                // Create new Receptor Sprite
                Receptors[i] = new Sprite
                {

                    SizeX = GameplayReferences.PlayfieldObjectSize,
                    
                    Position = new Vector2(GameplayReferences.ReceptorXPosition[i], GameplayReferences.ReceptorYOffset),
                    Alignment = Alignment.TopLeft,
                    Parent = BoundaryUnder
                };

                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.KeyCount)
                {
                    case 4:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptors[i];
                        Receptors[i].SizeY = GameplayReferences.PlayfieldObjectSize * (float) GameBase.LoadedSkin.NoteReceptors[i].Height / GameBase.LoadedSkin.NoteReceptors[i].Width;
                        break;
                    case 7:
                        Receptors[i].Image = GameBase.LoadedSkin.NoteReceptors7K[i];
                        Receptors[i].SizeY = GameplayReferences.PlayfieldObjectSize * (float) GameBase.LoadedSkin.NoteReceptors7K[i].Height / GameBase.LoadedSkin.NoteReceptors7K[i].Width;
                        break;
                }
            }

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

            //TODO: add judge scale
            JudgeSprite = new Sprite()
            {
                Size = JudgeSizes[0],
                Alignment = Alignment.MidCenter,
                Image = JudgeImages[0],
                Parent = BoundaryOver,
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
                Font = Fonts.Medium16,
                Parent = BoundaryOver,
                Alpha = 0
            };

            // Create Offset Gauge
            OffsetGaugeBoundary = new Boundary()
            {
                SizeX = 220 * GameBase.WindowYRatio,
                SizeY = 10 * GameBase.WindowYRatio,
                PositionY = 30 * GameBase.WindowYRatio,
                Alignment = Alignment.MidCenter,
                Parent = BoundaryOver
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

        public void Draw()
        {
            //draw stuff
        }

        /// <summary>
        ///     Draws the first layer of the Playfield (Renders before Notes)
        /// </summary>
        internal void DrawUnder()
        {
            BoundaryUnder.Draw();
        }

        /// <summary>
        ///     Draws the second layer of the Playfield (Renders after Notes)
        /// </summary>
        internal void DrawOver()
        {
            BoundaryOver.Draw();
        }

        /// <summary>
        ///     Updates the current playfield.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Update the delta time tweening variable for animation.
            AlphaHold += dt;
            PriorityJudgeLength -= dt;
            if (PriorityJudgeLength <= 0)
            {
                PriorityJudgeLength = 0;
                PriorityJudgeImage = 0;
            }
            var tween = Math.Min(dt / 30, 1);

            // Update receptors
            /*
            for (var i = 0; i < GameBase.SelectedBeatmap.Qua.KeyCount; i++)
            {
                var receptorSizeOffset = (ReceptorCurrentSize[i] - 1) * PlayfieldObjectSize / 2f;

                // Update receptor Size/Position
                ReceptorCurrentSize[i] = Util.Tween(ReceptorTargetSize[i], ReceptorCurrentSize[i], tween);
                Receptors[i].Size = Vector2.One * ReceptorCurrentSize[i] * PlayfieldObjectSize;
                Receptors[i].PositionX = ReceptorXPosition[i] - receptorSizeOffset;
                Receptors[i].PositionY = ReceptorYOffset - receptorSizeOffset;
            }*/

            // Update Offset Indicators
            foreach (var sprite in OffsetIndicatorsSprites)
            {
                sprite.Alpha = Util.Tween(0, sprite.Alpha, tween/30);
            }

            // Update Judge Alpha
            JudgeSprite.PositionY = Util.Tween(0, JudgeSprite.PositionY, tween/2);
            if (AlphaHold > 500 && PriorityJudgeLength <= 0)
            {
                JudgeSprite.Alpha = Util.Tween(0, JudgeSprite.Alpha, tween/10);
                ComboText.Alpha = Util.Tween(0, ComboText.Alpha, tween/10);
            }

            //Update Playfield + Children
            BoundaryUnder.Update(dt);
            BoundaryOver.Update(dt);
        }

        /// <summary>
        ///     Unloads content to free memory
        /// </summary>
        public  void UnloadContent()
        {
            BoundaryUnder.Destroy();
        }

        public  void UpdateJudge(int index, bool release = false, double? offset = null)
        {
            //TODO: add judge scale
            ComboText.Text = GameplayReferences.Combo + "x";
            ComboText.Alpha = 1;
            JudgeSprite.Alpha = 1;
            AlphaHold = 0;

            if (index >= PriorityJudgeImage || PriorityJudgeLength <= 0)
            {
                // Priority Judge Image to show
                if (index < 2) PriorityJudgeLength = 50;
                else if (index == 2) PriorityJudgeLength = 100;
                else if (index == 3) PriorityJudgeLength = 300;
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

        /// <summary>
        /// Gets called whenever a key gets pressed. This method updates the receptor state.
        /// </summary>
        /// <param name="curReceptor"></param>
        public bool UpdateReceptor(int curReceptor, bool keyDown)
        {
            if (keyDown)
            {
                //TODO: CHANGE TO RECEPTOR_DOWN SKIN LATER WHEN RECEPTOR IS PRESSED
                Receptors[curReceptor].Image = GameBase.LoadedSkin.ColumnHitLighting;
                ReceptorTargetSize[curReceptor] = 1.1f;
            }
            else
            {
                // Set current receptor's image based on the current key count.
                switch (GameBase.SelectedBeatmap.Qua.KeyCount)
                {
                    case 4:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptors[curReceptor];
                        break;
                    case 7:
                        Receptors[curReceptor].Image = GameBase.LoadedSkin.NoteReceptors7K[curReceptor];
                        break;
                }
                
                ReceptorTargetSize[curReceptor] = 1;
            }

            return true;
        }
    }
}
