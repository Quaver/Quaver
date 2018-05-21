using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.Mania;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class KeysPlayfieldStage
    {       
        /// <summary>
        ///     The container that holds all of the HitObjects
        /// </summary>
        internal QuaverContainer HitObjectContainer { get; set; }

        /// <summary>
        ///     Reference to the gameplay screen itself.
        /// </summary>
        private GameplayScreen Screen { get;  }

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
        internal List<QuaverSprite> Receptors { get; set; }

        /// <summary>
        ///     The column lighting objects.
        /// </summary>
        private List<ColumnLighting> ColumnLightingObjects { get; set; }

        /// <summary>
        ///     The sprite that essentially covers the top (or bottom if upscroll) of the playfield.
        /// </summary>
        private QuaverSprite DistantOverlay { get; set; }

        /// <summary>
        ///     The sprite that goes over the hit position.
        /// </summary>
        private QuaverSprite HitPositionOverlay { get; set; }

        /// <summary>
        ///     The display for combo.
        /// </summary>
        internal NumberDisplay ComboDisplay { get; set; }

        /// <summary>
        ///     The offset of the hit position.
        /// </summary>
        internal float HitPositionOffset
        {
            get
            {
                var playfield = (KeysPlayfield) Screen.GameModeComponent.Playfield;

                switch (Screen.GameModeComponent.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return GameBase.LoadedSkin.HitPositionOffset4K;
                        else
                            return -GameBase.LoadedSkin.HitPositionOffset4K;
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return GameBase.LoadedSkin.HitPositionOffset7K;
                        else
                            return -GameBase.LoadedSkin.HitPositionOffset7K;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="playfield"></param>
        internal KeysPlayfieldStage(KeysPlayfield playfield, GameplayScreen screen)
        {
            Playfield = playfield;
            Screen = screen;
       
            CreateStageLeft();
            CreateStageRight();
            
            // Create game mode specific sprites.
            // 4K and 7K are two separate modes and do NOT use the same textures
            // or skin properties. So we have to implement them separately.
            switch (Playfield.Map.Mode)
            {
                case GameMode.Keys4:
                    CreateBgMask4K();

                    // Depending on what the skin.ini's value is, we'll want to either initialize
                    // the receptors first, or the playfield first.
                    if (GameBase.LoadedSkin.ReceptorsOverHitObjects4K)
                    {
                        CreateHitObjectContainer();
                        CreateReceptorsAndLighting4K();                 
                    }
                    else
                    {
                        CreateReceptorsAndLighting4K();  
                        CreateHitObjectContainer();
                    }                          
                    break;
                case GameMode.Keys7:
                    CreateBgMask7K();

                    // Depending on what the skin.ini's value is, we'll want to either initialize
                    // the receptors first, or the playfield first.
                    if (GameBase.LoadedSkin.ReceptorsOverHitObjects7K)
                    {
                        CreateHitObjectContainer();
                        CreateReceptorsAndLighting7K();                 
                    }
                    else
                    {
                        CreateReceptorsAndLighting7K();  
                        CreateHitObjectContainer();
                    }                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Create the hit position overlay
            CreateHitPositionOverlay();

            // Create distant overlay last so it shows over the objects.
            CreateDistantOverlay();
            
            // Create combo display.
            CreateComboDisplay();
        }

        /// <summary>
        ///     Update method for the stage.
        /// </summary>
        /// <param name="dt"></param>
        internal void Update(double dt)
        {
            PeformAllColumnLightingAnimations(dt);
            UpdateComboDisplay(dt);
        }
        
#region SPRITE_CREATION
        
        /// <summary>
        ///     Creates the HitObjectContainer.
        /// </summary>
        private void CreateHitObjectContainer()
        {
            HitObjectContainer = new QuaverContainer
            {
                Size = new UDim2D(Playfield.Width, 0, 0, 1),
                Alignment = Alignment.TopCenter,
                Parent = Playfield.ForegroundContainer
            };
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
                Parent = Playfield.BackgroundContainer,
                Alignment = Alignment.TopLeft
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
                Position = new UDim2D(stageRightX - 1),
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
                    Position = new UDim2D(posX, Playfield.ReceptorPositionY),
                    Alignment = Alignment.TopLeft,
                    Image = GameBase.LoadedSkin.NoteReceptorsUp4K[i],
                    SpriteEffect = !ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Parent = Playfield.ForegroundContainer,
                    Size = new UDim2D(Playfield.LaneSize, Playfield.LaneSize * GameBase.LoadedSkin.NoteReceptorsUp4K[i].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[i].Width)
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
            // Get the downscroll setting for this mode.
            // We handle it here because it's too basic to re-copy its implementation for 7K.
            var modeDownscroll = Playfield.Map.Mode == GameMode.Keys4 ? ConfigManager.DownScroll4K : ConfigManager.DownScroll7K;
            
            // Create Stage HitPosition Overlay
            var sizeY = GameBase.LoadedSkin.StageHitPositionOverlay.Height * Playfield.Width / GameBase.LoadedSkin.StageHitPositionOverlay.Width;
            var offsetY = Playfield.LaneSize * ((float)GameBase.LoadedSkin.NoteReceptorsUp4K[0].Height / GameBase.LoadedSkin.NoteReceptorsUp4K[0].Width);
            
            HitPositionOverlay = new QuaverSprite()
            {
                Image = GameBase.LoadedSkin.StageHitPositionOverlay,
                Size = new UDim2D(Playfield.Width, sizeY),
                PosY = HitPositionOffset,
                Alignment = Alignment.TopLeft,
                Parent = Playfield.ForegroundContainer
            };    
        }

        /// <summary>
        ///     Creates the display for combo.
        /// </summary>
        private void CreateComboDisplay()
        {
            // Create the combo display.
            ComboDisplay = new NumberDisplay(NumberDisplayType.Combo, "0")
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
            };
            
            ComboDisplay.PosX = -ComboDisplay.TotalWidth / 2f;
            
            // Start off the map by making the display invisible.
            ComboDisplay.MakeInvisible();
        }
#endregion

#region ANIMATIONS
         /// <summary>
        ///     Performs the animations for all column lighting
        /// </summary>
        /// <param name="dt"></param>
         private void PeformAllColumnLightingAnimations(double dt)
        {
            foreach (var light in ColumnLightingObjects)
                light.PerformAnimation(dt);
        }

        /// <summary>
        ///     Updates the given receptor and column lighting activity
        ///     (Called when pressing/releasing keys.)
        /// </summary>
        internal void SetReceptorAndLightingActivity(int index, bool pressed)
        {      
            switch (GameBase.SelectedMap.Qua.Mode)
            {
                case GameMode.Keys4:
                    if (pressed)
                    {
                        Receptors[index].Image = GameBase.LoadedSkin.NoteReceptorsDown4K[index];
                        ColumnLightingObjects[index].Active = true;
                        ColumnLightingObjects[index].AnimationValue = 1.0f;
                    }
                    else
                    {
                        Receptors[index].Image = GameBase.LoadedSkin.NoteReceptorsUp4K[index];
                        ColumnLightingObjects[index].Active = false;
                    }
                    break;
                case GameMode.Keys7:
                    if (pressed)
                    {
                        Receptors[index].Image = GameBase.LoadedSkin.NoteReceptorsDown7K[index];
                        ColumnLightingObjects[index].Active = true;
                        ColumnLightingObjects[index].AnimationValue = 1.0f;
                    }
                    else
                    {
                        Receptors[index].Image = GameBase.LoadedSkin.NoteReceptorsUp7K[index];
                        ColumnLightingObjects[index].Active = false;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Updates the combo display.
        /// </summary>
        private void UpdateComboDisplay(double dt)
        {                
            // Grab the old value
            var oldCombo = ComboDisplay.Value;
            
            // Set the new one
            ComboDisplay.Value = Screen.GameModeComponent.ScoreProcessor.Combo.ToString();
            
            // If the combo needs repositioning, do so accordingly.
            if (oldCombo.Length != ComboDisplay.Value.Length)
                ComboDisplay.PosX = -ComboDisplay.TotalWidth / 2f;
        }
#endregion
    }
}