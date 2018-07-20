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
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UI;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.Skinning;
using Quaver.States.Gameplay.UI;
using Quaver.States.Gameplay.UI.Components;
using Quaver.States.Gameplay.UI.Components.Judgements;

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
        internal Sprite StageLeft { get; private set; }

        /// <summary>
        ///     The right side of the stage.
        /// </summary>
        internal Sprite StageRight { get; private set; }

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

        /// <summary>
        ///     Reference to the current skin.
        /// </summary>
        private SkinKeys Skin => GameBase.Skin.Keys[Screen.Map.Mode];

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
            CreateBgMask();
                     
            // Depending on what the skin.ini's value is, we'll want to either initialize
            // the receptors first, or the playfield first.
            if (Skin.ReceptorsOverHitObjects)
            {
                CreateHitObjectContainer();
                CreateReceptorsAndLighting();
            }
            else
            {
                CreateReceptorsAndLighting();
                CreateHitObjectContainer();
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
            var stageLeftX = Skin.StageLeftBorder.Width * GameBase.WindowRectangle.Height / Skin.StageLeftBorder.Height;

            StageLeft = new Sprite()
            {
                Image = Skin.StageLeftBorder,
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
            var stageRightX = Skin.StageRightBorder.Width * GameBase.WindowRectangle.Height / Skin.StageRightBorder.Height;
            StageRight = new Sprite
            {
                Image = Skin.StageRightBorder,
                Size = new UDim2D(stageRightX, GameBase.WindowRectangle.Height),
                Position = new UDim2D(stageRightX - 1),
                Parent = Playfield.BackgroundContainer,
                Alignment = Alignment.TopRight
            };
        }
      
        /// <summary>
        ///     Creates the BG Mask
        /// </summary>
        private void CreateBgMask()
        {
            var imageRatio = (double)Skin.StageBgMask.Width / Skin.StageBgMask.Height;
            var columnRatio = Playfield.Width / GameBase.WindowRectangle.Height;
            var bgMaskSize = (float)Math.Max(GameBase.WindowRectangle.Height * columnRatio / imageRatio, GameBase.WindowRectangle.Height);
            
            BgMask = new Sprite()
            {
                Image = Skin.StageBgMask,
                Alpha = Skin.BgMaskAlpha,
                Size = new UDim2D(Playfield.Width, bgMaskSize),
                Alignment = Alignment.MidCenter,
                Parent = Playfield.BackgroundContainer
            };
        }
        
        /// <summary>
        ///     Creates the receptors and column lighting
        /// </summary>
        private void CreateReceptorsAndLighting()
        {
            Receptors = new List<Sprite>();
            ColumnLightingObjects = new List<ColumnLighting>();
            
            // Go through and create the 4 receptors and column lighting objects.
            for (var i = 0; i < Playfield.Map.GetKeyCount(); i++)
            {
                var posX = (Playfield.LaneSize + Playfield.ReceptorPadding) * i + Playfield.Padding;
                
                // Create individiaul receptor.
                Receptors.Add(new Sprite
                {
                    Size = new UDim2D(Playfield.LaneSize, Playfield.LaneSize * Skin.NoteReceptorsUp[i].Height / Skin.NoteReceptorsUp[i].Width),
                    Position = new UDim2D(posX, Playfield.ReceptorPositionY),
                    Alignment = Alignment.TopLeft,
                    Image = Skin.NoteReceptorsUp[i],
                    SpriteEffect = !GameModeRulesetKeys.IsDownscroll && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    Parent = Playfield.ForegroundContainer
                });
                
                // Create the column lighting sprite.
                var lightingY = Skin.ColumnLightingScale * Playfield.LaneSize * ((float)Skin.ColumnLighting.Height / Skin.ColumnLighting.Width);                 
                ColumnLightingObjects.Add(new ColumnLighting(new Sprite
                {
                    Image = Skin.ColumnLighting,
                    Size = new UDim2D(Playfield.LaneSize, lightingY),
                    Tint = Skin.ColumnColors[i],
                    PosX = posX,
                    PosY = GameModeRulesetKeys.IsDownscroll ? Playfield.ColumnLightingPositionY - lightingY : Playfield.ColumnLightingPositionY,
                    SpriteEffect = !GameModeRulesetKeys.IsDownscroll && Skin.FlipNoteImagesOnUpscroll ? SpriteEffects.FlipVertically : SpriteEffects.None,
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
            var sizeY = Skin.StageDistantOverlay.Height * Playfield.Width / Skin.StageDistantOverlay.Width;
            DistantOverlay = new Sprite
            {
                Image = Skin.StageDistantOverlay,
                Size = new UDim2D(Playfield.Width, sizeY),
                PosY = GameModeRulesetKeys.IsDownscroll ? -1 : 1,
                Alignment = GameModeRulesetKeys.IsDownscroll ? Alignment.TopRight : Alignment.BotRight,
                Parent = Playfield.ForegroundContainer
            };
        }

        /// <summary>
        ///     Creates the HitPositionOverlay
        /// </summary>
        private void CreateHitPositionOverlay()
        {            
            // Create Stage HitPosition Overlay
            var sizeY = Skin.StageHitPositionOverlay.Height * Playfield.Width / Skin.StageHitPositionOverlay.Width;
            var offsetY = Playfield.LaneSize * ((float)Skin.NoteReceptorsUp[0].Height / Skin.NoteReceptorsUp[0].Width);
            
            HitPositionOverlay = new Sprite()
            {
                Image = Skin.StageHitPositionOverlay,
                Size = new UDim2D(Playfield.Width, sizeY),
                PosY = GameModeRulesetKeys.IsDownscroll ? Playfield.ReceptorPositionY : Playfield.ReceptorPositionY + offsetY + sizeY,
                Parent = Playfield.ForegroundContainer
            };    
        }

        /// <summary>
        ///     Creates the display for combo.
        /// </summary>
        private void CreateComboDisplay()
        {
            // Create the combo display.
            ComboDisplay = new NumberDisplay(NumberDisplayType.Combo, "0", new Vector2(1, 1))
            {
                Parent = Playfield.ForegroundContainer,
                Alignment = Alignment.MidCenter,
                PosY = Skin.ComboPosY
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
            HitError = new HitErrorBar(new UDim2D(50, 10))
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
            var frames = GameBase.Skin.Judgements[Judgement.Miss];
            
            // Grab the first frame for convenience.
            var firstFrame = frames[0];
            
            // Set size w/ scaling.
            var size = new Vector2(firstFrame.Width, firstFrame.Height) * Skin.JudgementHitBurstScale / firstFrame.Height;
            
            JudgementHitBurst = new JudgementHitBurst(frames, size, Skin.JudgementBurstPosY)
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

            for (var i = 0; i < Screen.Map.GetKeyCount(); i++)
            {
                var hl = new HitLighting()
                {
                    Parent = Playfield.HitLightingContainer,
                    Visible = false
                };

                // If the width or height are less than 0, then we'll assume the user wants it to be the height of the texture
                // otherwise we'll use the one from their skin config.
                var width = Skin.HitLightingWidth <= 0 ? hl.Frames.First().Width : Skin.HitLightingWidth;
                var height = Skin.HitLightingHeight <= 0 ? hl.Frames.First().Height : Skin.HitLightingHeight;               
                hl.Size = new UDim2D(width, height);
                
                hl.Position = new UDim2D(Receptors[i].PosX - Playfield.LaneSize / 2f - Playfield.ReceptorPadding, 
                                            HitPositionOverlay.PosY - hl.SizeY / 2f + Skin.HitLightingY);
                
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
            if (pressed)
            {
                Receptors[index].Image = Skin.NoteReceptorsDown[index];
                ColumnLightingObjects[index].Active = true;
                ColumnLightingObjects[index].AnimationValue = 1.0f;
            }
            else
            {
                Receptors[index].Image = Skin.NoteReceptorsUp[index];
                ColumnLightingObjects[index].Active = false;
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