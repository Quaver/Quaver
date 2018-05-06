using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States.Gameplay.Mania;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class KeysPlayfieldStage
    {
        /// <summary>
        ///     Reference to the parent playfield.
        /// </summary>
        private KeysPlayfield Playfield { get; }

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

        /// <summary>
        ///     The receptors for this stage.
        /// </summary>
        private List<QuaverSprite> Receptors { get; set; }

        /// <summary>
        ///     The column lighting objects.
        /// </summary>
        private List<ColumnLighting> ColumnLightingObjects { get; set; }

        /// <summary>
        ///     The sprite that essentially covers the top (or bottom if upscroll) of the playfield.
        /// </summary>
        private QuaverSprite DistantOverlay { get; set; }

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
            CreateDistantOverlay();

            // Create game mode specific sprites.
            // 4K and 7K are two separate modes and do NOT use the same textures
            // or skin properties. So we have to implement them separately.
            switch (Playfield.Map.Mode)
            {
                case GameMode.Keys4:
                    CreateBgMask4K();
                    CreateReceptorsAndLighting4K();
                    break;
                case GameMode.Keys7:
                    CreateBgMask7K();
                    CreateReceptorsAndLighting7K();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            var stageRightX = GameBase.LoadedSkin.StageRightBorder.Width * GameBase.WindowRectangle.Height / GameBase.LoadedSkin.StageRightBorder.Height;
            StageRight = new QuaverSprite
            {
                Image = GameBase.LoadedSkin.StageRightBorder,
                Size = new UDim2D(stageRightX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(stageRightX - 1, 0),
                Parent = Playfield.BackgroundContainer,
                Alignment = Alignment.TopRight
            };
        }
      
        /// <summary>
        ///     Creates the BG Mask for 4K.
        /// </summary>
        private void CreateBgMask4K()
        {
            var imageRatio = (double)GameBase.LoadedSkin.StageBgMask4K.Width / GameBase.LoadedSkin.StageBgMask4K.Height;
            var columnRatio = Playfield.Width / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);
            
            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask4K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(Playfield.Width, bgMaskSize),
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
            var columnRatio = Playfield.Width / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);

            BgMask = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageBgMask7K,
                Alpha = GameBase.LoadedSkin.BgMaskAlpha,
                Size = new UDim2D(Playfield.Width, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = Playfield.BackgroundContainer
            };
        }

        /// <summary>
        ///     Creates the receptors and column lighting for 4K.
        /// </summary>
        private void CreateReceptorsAndLighting4K()
        {
            Receptors = new List<QuaverSprite>();
            ColumnLightingObjects = new List<ColumnLighting>();
            
            // Go through and create the 4 receptors and column lighting objects.
            for (var i = 0; i < 4; i++)
            {
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;
                
                // Create individiaul receptor.
                Receptors.Add(new QuaverSprite
                {
                    Size = new UDim2D(Playfield.LaneSize, Playfield.LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width),
                    Position = new UDim2D(posX, Playfield.ReceptorPositionY),
                    Alignment = Alignment.TopLeft,
                    Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i],
                    SpriteEffect = !ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Parent = Playfield.ForegroundContainer
                });
                
                // Create the column lighting sprite.
                var lightingY = GameBase.LoadedSkin.ColumnLightingScale * Playfield.LaneSize * ((float)GameBase.LoadedSkin.ColumnLighting4K.Height / GameBase.LoadedSkin.ColumnLighting4K.Width);                 
                ColumnLightingObjects.Add(new ColumnLighting(new QuaverSprite
                {
                    Image = GameBase.LoadedSkin.ColumnLighting4K,
                    Size = new UDim2D(Playfield.LaneSize, lightingY),
                    Tint = GameBase.LoadedSkin.ColumnColors4K[i],
                    PosX = posX,
                    PosY = ConfigManager.DownScroll4K.Value ? Playfield.ColumnLightingPositionY - lightingY : Playfield.ColumnLightingPositionY,
                    SpriteEffect = !ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Alignment = Alignment.TopLeft,
                    Parent = Playfield.BackgroundContainer
                }));
            }      
        }
        
         /// <summary>
        ///     Creates the receptors and column lighting for 7K.
        /// </summary>
        private void CreateReceptorsAndLighting7K()
        {
            Receptors = new List<QuaverSprite>();
            ColumnLightingObjects = new List<ColumnLighting>();
            
            // Go through and create the 7 receptors and column lighting objects.
            for (var i = 0; i < 7; i++)
            {
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;
                
                // Create individiaul receptor.
                Receptors.Add(new QuaverSprite
                {
                    Size = new UDim2D(Playfield.LaneSize, Playfield.LaneSize * GameBase.LoadedSkin.NoteReceptorsUp7K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp7K[i].Width),
                    Position = new UDim2D(posX, Playfield.ReceptorPositionY),
                    Alignment = Alignment.TopLeft,
                    Image = GameBase.LoadedSkin.NoteReceptorsUp7K[i],
                    SpriteEffect = !ConfigManager.DownScroll7K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Parent = Playfield.ForegroundContainer
                });
                
                // Create the column lighting sprite.
                var lightingY = GameBase.LoadedSkin.ColumnLightingScale * Playfield.LaneSize * ((float)GameBase.LoadedSkin.ColumnLighting7K.Height / GameBase.LoadedSkin.ColumnLighting7K.Width);                 
                ColumnLightingObjects.Add(new ColumnLighting(new QuaverSprite
                {
                    Image = GameBase.LoadedSkin.ColumnLighting7K,
                    Size = new UDim2D(Playfield.LaneSize, lightingY),
                    Tint = GameBase.LoadedSkin.ColumnColors7K[i],
                    PosX = posX,
                    PosY = ConfigManager.DownScroll7K.Value ? Playfield.ColumnLightingPositionY - lightingY : Playfield.ColumnLightingPositionY,
                    SpriteEffect = !ConfigManager.DownScroll7K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll7K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Alignment = Alignment.TopLeft,
                    Parent = Playfield.BackgroundContainer
                }));
            }      
        }

        /// <summary>
        ///     Creates the distant overlay sprite.
        /// </summary>
        private void CreateDistantOverlay()
        {
            // Get the downscroll setting for this mode.
            // We handle it here because it's too basic to re-copy its implementation for 7K.
            var modeDownscroll = Playfield.Map.Mode == GameMode.Keys4 ? ConfigManager.DownScroll4K : ConfigManager.DownScroll7K;
            
            var sizeY = GameBase.LoadedSkin.StageDistantOverlay.Height * Playfield.Width / GameBase.LoadedSkin.StageDistantOverlay.Width;
            DistantOverlay = new QuaverSprite
            {
                Image = GameBase.LoadedSkin.StageDistantOverlay,
                Size = new UDim2D(Playfield.Width, sizeY),
                PosY = modeDownscroll.Value ? -1 : 1,
                Alignment = modeDownscroll.Value ? Alignment.TopRight : Alignment.BotRight,
                Parent = Playfield.ForegroundContainer
            };
        }

        /// <summary>
        ///     Creates the HitPositionOverlay and utilizes the 
        /// </summary>
        private void CreateHitPositionOverlay()
        {
            
        }
    }
}