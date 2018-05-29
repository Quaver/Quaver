using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.Mania;
using Quaver.States.Gameplay.UI;
using Quaver.States.Gameplay.UI.Judgements;

namespace Quaver.States.Gameplay.GameModes.Keys.Playfield
{
    internal class KeysPlayfieldStage
    {       
        /// <summary>
        ///     The container that holds all of the HitObjects
        /// </summary>
        internal Container HitObjectContainer { get; set; }

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
        private Sprite StageLeft { get; set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        private Sprite StageRight { get; set; }

        /// <summary>
        ///     The Bg mask of the stage.
        /// </summary>
        private Sprite BgMask { get; set; }

        /// <summary>
        ///     The receptors for this stage.
        /// </summary>
        internal List<Sprite> Receptors { get; set; }

        /// <summary>
        ///     The column lighting objects.
        /// </summary>
        private List<ColumnLighting> ColumnLightingObjects { get; set; }

        /// <summary>
        ///     The sprite that essentially covers the top (or bottom if upscroll) of the playfield.
        /// </summary>
        private Sprite DistantOverlay { get; set; }

        /// <summary>
        ///     The sprite that goes over the hit position.
        /// </summary>
        private Sprite HitPositionOverlay { get; set; }

        /// <summary>
        ///     The display for combo.
        /// </summary>
        internal NumberDisplay ComboDisplay { get; set; }

        /// <summary>
        ///     The original value for the combo display's Y position,
        ///     so we can use this to set it back after it's done with its animation.
        /// </summary>
        private float OriginalComboDisplayY { get; set; }

        /// <summary>
        ///     The hit error.
        /// </summary>
        internal HitErrorBar HitError { get; set; }

        /// <summary>
        ///     The judgement hit burst when hitting objects.
        /// </summary>
        internal JudgementHitBurst JudgementHitBurst { get; set; }

        /// <summary>
        ///     When hitting an object, this is the sprite that will be shown at
        ///     the hitposition.
        /// </summary>
        internal List<HitLighting> HitLighting { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="playfield"></param>
        /// <param name="screen"></param>
        internal KeysPlayfieldStage(KeysPlayfield playfield, GameplayScreen screen)
        {
            Playfield = playfield;
            Screen = screen;
       
            CreateStageLeft();
            CreateStageRight();
            CreateHitPositionOverlay();
                          
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
            
            // Create distant overlay last so it shows over the objects.
            CreateDistantOverlay();
            
            // Create combo display.
            CreateComboDisplay();
            
            // Create HitError
            CreateHitError();
            
            // Create judgement hit burst
            CreateJudgementHitBurst();
            
            CreateHitLighting();
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
            HitObjectContainer = new Container
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

            StageLeft = new Sprite()
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
            StageRight = new Sprite
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
            
            BgMask = new Sprite()
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

            BgMask = new Sprite()
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
            Receptors = new List<Sprite>();
            ColumnLightingObjects = new List<ColumnLighting>();
            
            // Go through and create the 4 receptors and column lighting objects.
            for (var i = 0; i < 4; i++)
            {
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;
                
                // Create individiaul receptor.
                Receptors.Add(new Sprite
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
                ColumnLightingObjects.Add(new ColumnLighting(new Sprite
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
            Receptors = new List<Sprite>();
            ColumnLightingObjects = new List<ColumnLighting>();
            
            // Go through and create the 7 receptors and column lighting objects.
            for (var i = 0; i < 7; i++)
            {
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;
                
                // Create individiaul receptor.
                Receptors.Add(new Sprite
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
                ColumnLightingObjects.Add(new ColumnLighting(new Sprite
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
            DistantOverlay = new Sprite
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
            
            HitPositionOverlay = new Sprite()
            {
                Image = GameBase.LoadedSkin.StageHitPositionOverlay,
                Size = new UDim2D(Playfield.Width, sizeY),
                PosY = modeDownscroll.Value ? Playfield.ReceptorPositionY : Playfield.ReceptorPositionY + offsetY + sizeY,
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

            OriginalComboDisplayY = ComboDisplay.PosY;
            ComboDisplay.PosX = -ComboDisplay.TotalWidth / 2f;
            
            // Start off the map by making the display invisible.
            ComboDisplay.MakeInvisible();
        }

        /// <summary>
        ///     Creates the hit error bar.
        /// </summary>
        private void CreateHitError()
        {
            HitError = new HitErrorBar(HitErrorType.Quaver, new UDim2D(50, 10))
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
                Position = new UDim2D(0, 55)
            };
        }

        /// <summary>
        ///     Creates the judgement hit burst.
        /// </summary>
        private void CreateJudgementHitBurst()
        {
            // Default the frames to miss.
            var frames = GameBase.LoadedSkin.JudgeMiss;
            
            // Grab the first frame for convenience.
            var firstFrame = frames[0];
            
            // Set size w/ scaling.
            var size = new Vector2(firstFrame.Width, firstFrame.Height) * GameBase.LoadedSkin.JudgementHitBurstScale / firstFrame.Height;
            
            JudgementHitBurst = new JudgementHitBurst(frames, size, 105)
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
            };
        }

        /// <summary>
        ///     Creates the hit lighting sprites.
        /// </summary>
        private void CreateHitLighting()
        {
            HitLighting = new List<HitLighting>();

            for (var i = 0; i < Screen.Map.FindKeyCountFromMode(); i++)
            {
                var hl = new HitLighting()
                {
                    Parent = Playfield.HitLightingContainer,
                    Visible = false
                };

                // If the width or height are less than 0, then we'll assume the user wants it to be the height of the texture
                // otherwise we'll use the one from their skin config.
                var width = GameBase.LoadedSkin.HitLightingWidth <= 0 ? hl.Frames.First().Width : GameBase.LoadedSkin.HitLightingWidth;
                var height = GameBase.LoadedSkin.HitLightingHeight <= 0 ? hl.Frames.First().Height : GameBase.LoadedSkin.HitLightingHeight;               
                hl.Size = new UDim2D(width, height);
                
                hl.Position = new UDim2D(Receptors[i].PosX - Playfield.LaneSize / 2f, HitPositionOverlay.PosY - hl.SizeY / 2f + GameBase.LoadedSkin.HitLightingY);
                
                HitLighting.Add(hl);
            }
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
            ComboDisplay.Value = Screen.Ruleset.ScoreProcessor.Combo.ToString();
            
            // If the combo needs repositioning, do so accordingly.
            if (oldCombo.Length != ComboDisplay.Value.Length)
                ComboDisplay.PosX = -ComboDisplay.TotalWidth / 2f;

            // Set the position and scale  of the combo display, so that we can perform some animations.
            if (oldCombo != ComboDisplay.Value && ComboDisplay.Visible)
                ComboDisplay.PosY = OriginalComboDisplayY - 5;
         
            // Gradually tween the position back to what it was originally.
            ComboDisplay.PosY = GraphicsHelper.Tween(OriginalComboDisplayY, ComboDisplay.PosY, Math.Min(dt / 30, 1) / 2);          
        }

        /// <summary>
        ///     Fades out all of the sprites.
        /// </summary>
        /// <param name="dt"></param>
        internal void FadeOut(double dt)
        {
            const int scale = 480;
            
            StageLeft.FadeOut(dt, scale);
            StageRight.FadeOut(dt, scale);
            BgMask.FadeOut(dt, scale);
            Receptors.ForEach(x => x.FadeOut(dt, scale));
            DistantOverlay.FadeOut(dt, scale);
            HitPositionOverlay.FadeOut(dt, scale);
            ComboDisplay.Digits.ForEach(x => x.FadeOut(dt, scale));
            HitError.LastHitCheveron.FadeOut(dt, scale);
            HitError.MiddleLine.FadeOut(dt, scale);
            HitError.LineObjectPool.ForEach(x => x.FadeOut(dt, scale));  
            JudgementHitBurst.FadeOut(dt, scale);
        }
#endregion
    }
}