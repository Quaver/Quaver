using System;
using System.ComponentModel;
using Quaver.API.Enums;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.States.Gameplay.Keys.Playfield
{
    internal class KeysPlayfieldStage
    {
        /// <summary>
        ///     Reference to the parent playfield.
        /// </summary>
        private KeysPlayfield Playfield { get; set; }

        /// <summary>
        ///     The left side of the stage.
        /// </summary>
        private QuaverSprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private QuaverSprite StageRight { get; set; }

        /// <summary>
        ///     The Bg mask of the stage.
        /// </summary>
        private QuaverSprite BgMask { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="playfield"></param>
        internal KeysPlayfieldStage(KeysPlayfield playfield)
        {
            Playfield = playfield;
       
            CreateStageLeft();
            CreateStageRight();
            CreateBgMask();
        }

        /// <summary>
        ///     Creates the left side of the stage.
        /// </summary>
        private void CreateStageLeft()
        {
            // Create the left side of the stage.
            var stageLeftX = GameBase.LoadedSkin.StageLeftBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageLeftBorder.Height;
            StageLeft = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageLeftBorder,
                Size = new UDim2D(stageLeftX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(-stageLeftX + 1),
                Parent = Playfield.BackgroundContainer
            };   
        }

        /// <summary>
        ///     Creates the right side of the stage.
        /// </summary>
        private void CreateStageRight()
        {
            // Create the right side of the stage.
            // Create Stage Right
            var stageRightX = GameBase.LoadedSkin.StageRightBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageRightBorder.Height;
            StageRight = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageRightBorder,
                Size = new UDim2D(stageRightX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(stageRightX - 1, 0),
                Parent = Playfield.BackgroundContainer
            };
            
            StageRight.Alignment = Alignment.TopRight;
        }

        /// <summary>
        ///     Creates the Bg Mask for the stage.
        /// </summary>
        private void CreateBgMask()
        {
            // Create BG Mask
            switch (Playfield.Map.Mode)
            {
                case GameMode.Keys4:
                    CreateBgMask4K();
                    break;
                case GameMode.Keys7:
                    CreateBgMask7K();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        ///     Creates the BG Mask for 4K.
        /// </summary>
        private void CreateBgMask4K()
        {
            var imageRatio = (double)GameBase.LoadedSkin.StageBgMask4K.Width / GameBase.LoadedSkin.StageBgMask4K.Height;
            var columnRatio = Playfield.SizeX / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);
            
            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask4K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(Playfield.SizeX, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = Playfield.BackgroundContainer
            };
        }
        
        /// <summary>
        ///      Creates the BG Mask for 7K
        /// </summary>
        private void CreateBgMask7K()
        {
            // Create BG Mask
            var imageRatio = (double)GameBase.LoadedSkin.StageBgMask7K.Width / GameBase.LoadedSkin.StageBgMask7K.Height;
            var columnRatio = Playfield.SizeX / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);

            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask7K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(Playfield.SizeX, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = Playfield.BackgroundContainer
            };
        }
    }
}