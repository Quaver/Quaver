using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States;
using SQLite;

namespace Quaver.Graphics.Overlays.Volume
{
    internal class VolumeController : IGameStateComponent
    {
        /// <summary>
        ///     If the volume controller is currently active.
        ///     (It's considered active if the box is visible and the user is holding down either alt key.)
        /// </summary>
        internal bool IsActive => SurroundingBox.Visible && (GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt) || GameBase.KeyboardState.IsKeyDown(Keys.RightAlt));

        /// <summary>
        ///     The container for the volume controller.   
        /// </summary>
        private QuaverContainer Container { get; set; }

        /// <summary>
        ///     The surrounding box of the volume controller.
        /// </summary>
        private QuaverSprite SurroundingBox { get; set; }

        /// <summary>
        ///     The slider that controls the master volume.
        /// </summary>
        private QuaverSlider MasterVolumeSlider { get; set; }

        /// <summary>
        ///  The icon to that's associated with the master volume.
        /// </summary>
        private QuaverSprite MasterVolumeSliderIcon { get; set; }
        
        /// <summary>
        ///     The slider that controls the music volume.
        /// </summary>
        private QuaverSlider MusicVolumeSlider { get; set; }

        /// <summary>
        ///     The icon that's asscoiated with the music volume slider.
        /// </summary>
        private QuaverSprite MusicVolumeSliderIcon { get; set; }

        /// <summary>
        ///     The slider that controls the sound effects.
        /// </summary>
        private QuaverSlider EffectVolumeSlider { get; set; }

        /// <summary>
        ///     The icon that's associated with the effect volume slider.
        /// </summary>
        private QuaverSprite EffectVolumeSliderIcon { get; set; }

        /// <summary>
        ///     Array containing all of the sliders, so we can iterate over them.
        /// </summary>
        private List<QuaverSlider> Sliders { get; }

        /// <summary>
        ///     The size of each slider.
        /// </summary>
        private Vector2 SliderSize { get; }

        /// <summary>
        ///     The slider that is currently "focused"
        /// </summary>
        private QuaverSlider FocusedSlider { get; set; }

        /// <summary>
        ///     The time elapsed since the last volume change.
        /// </summary>
        private double TimeElapsedSinceLastVolumeChange { get; set; } = 50;

        /// <summary>
        ///     The amount of time that the volume controller has been inactive.
        /// </summary>
        private double TimeInactive { get; set; }

        /// <summary>
        ///     Keeps track of if the volume control box is fading in.
        /// </summary>
        private bool IsFadingIn { get; set; }
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal VolumeController()
        {
            SliderSize = new Vector2(300, 3);
            Sliders = new List<QuaverSlider>();
        }
        
        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {            
            Container = new QuaverContainer();

            // Create the surrounding box that will house the sliders.
            SurroundingBox = new QuaverSprite()
            {
                Size = new UDim2D(250, 150, 0.1f, 0),
                Alignment = Alignment.TopRight,
                PosY = 60,
                PosX =  -25,
                Tint = new Color(0f, 0f, 0f, 0.65f),
                Parent = Container,
                // Set alpha to 0 upon creation as it'll be faded in.
                Alpha = 0 
            };
    
            #region masterVolumeSlider
            
            // Create master volume slider.
            MasterVolumeSlider = new QuaverSlider(ConfigManager.VolumeGlobal, SliderSize)
            {
                Parent = SurroundingBox,
                Alignment = Alignment.TopLeft,
                PosY = 30,
                PosX = 50
            };
            
            // Create the icon next to the slider.
            MasterVolumeSliderIcon = new QuaverSprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Volume,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(25, 25),
                PosY = MasterVolumeSlider.PosY - 10,
                PosX = 10
            };
            
            Sliders.Add(MasterVolumeSlider);
            
            #endregion

            #region musicVolumeSlider

            // Create music volume slider.
            MusicVolumeSlider = new QuaverSlider(ConfigManager.VolumeMusic, SliderSize)
            {
                Parent = SurroundingBox,
                Alignment = Alignment.MidLeft,
                PosY = 0,
                PosX = 50
            };
            
            // Create the icon next to the music slider.
            MusicVolumeSliderIcon = new QuaverSprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Music,
                Alignment = Alignment.MidLeft,
                Size = new UDim2D(25, 25),
                PosY = MusicVolumeSlider.PosY,
                PosX = 10
            };
            
            Sliders.Add(MusicVolumeSlider);
            
            #endregion

            #region effectVolumeSlider
            
            // Create master volume slider.
            EffectVolumeSlider = new QuaverSlider(ConfigManager.VolumeEffect, SliderSize)
            {
                Parent = SurroundingBox,
                Alignment = Alignment.BotLeft,
                PosY = -30,
                PosX = 50
            };
            
            // Create the icon that's next to the effect volume slider.
            EffectVolumeSliderIcon = new QuaverSprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Headphones,
                Alignment = Alignment.BotLeft,
                Size = new UDim2D(25, 25),
                PosY = EffectVolumeSlider.PosY + 10,
                PosX = 10
            };
            
            Sliders.Add(EffectVolumeSlider);
            
            #endregion

            SurroundingBox.Visible = false;
            FocusedSlider = MasterVolumeSlider;
        }

        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        public void Update(double dt)
        {
            // Handle all needed input.
            HandleInput();

            // Update last volume change timer.
            TimeElapsedSinceLastVolumeChange += dt;
            
            // As long as the box is visible, start 
            if (SurroundingBox.Visible)
                TimeInactive += dt;
            
            // Handle all fade effects when they need to happen.
            HandleFadeIn(dt);
            HandleInactiveFadeOut(dt);
            
            // Update the container as normla.
            Container.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }

        /// <summary>
        ///     Handles the overall input of the volume controller. Called every frame.
        /// </summary>
        private void HandleInput()
        {
            // Dictate which slider is the one that is currently focused.
            SetFocusedSlider();
            ChangeFocusedSliderColor();

            // Require either alt key to be pressed when changing volume.
            if (!GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt) && !GameBase.KeyboardState.IsKeyDown(Keys.RightAlt))
                return;
            
            // Activate the volume control box.
            if (InputHelper.IsUniqueKeyPress(Keys.Up)|| InputHelper.IsUniqueKeyPress(Keys.Down) || 
                InputHelper.IsUniqueKeyPress(Keys.Left) || InputHelper.IsUniqueKeyPress(Keys.Right)
                || GameBase.MouseState.ScrollWheelValue != GameBase.PreviousMouseState.ScrollWheelValue)
            {
                // Fade in the volume controller box when we want it to become active again.
                IsFadingIn = true;
                TimeInactive = 0;
            }
            
            // Mouse wheel input.
            if (GameBase.MouseState.ScrollWheelValue > GameBase.PreviousMouseState.ScrollWheelValue)
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(10);
            }
            else if (GameBase.MouseState.ScrollWheelValue < GameBase.PreviousMouseState.ScrollWheelValue)
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(-10);
            }
                
            // Keyboard input.
            if (GameBase.KeyboardState.IsKeyDown(Keys.Right))
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(5);
            }
            else if (GameBase.KeyboardState.IsKeyDown(Keys.Left))
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(-5);
            }
        }

        /// <summary>
        ///     Sets the currently focused slider out of our list of sliders.
        ///     The default focused slider is the master volume.
        /// </summary>
        private void SetFocusedSlider()
        {
            // A slider with the mouse currently hovered over it takes precedence over
            // any other action. That is automatically the focused slider.
            var focused = Sliders.Find(x => x.MouseInHoldSequence) ?? Sliders.Find(x => x.IsHovered);
            if (focused != null)
            {
                FocusedSlider = focused;
                TimeInactive = 0;
            }

            // If the user pressed the up key when determine the focused slider, 
            // it becomes the one above. (If first in the list, it becomes the last.)
            if (InputHelper.IsUniqueKeyPress(Keys.Up) && (GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt) || GameBase.KeyboardState.IsKeyDown(Keys.RightAlt)))
            {
                // Play hover sound effect
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHover);
                
                // Reset inactive timer.
                TimeInactive = 0;
                
                // If the focused slider is the first one in the list, we set it to the last.
                if (FocusedSlider == Sliders.First())
                {
                    FocusedSlider = Sliders.Last();
                    return;
                }

                // Otherwise, just set the focused to the previous in the list.
                var index = Sliders.IndexOf(FocusedSlider);
                FocusedSlider = Sliders[index - 1];
                return;
            }
            
            // If the user presses the down key, we switch the focused slider to the 
            if (InputHelper.IsUniqueKeyPress(Keys.Down) && (GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt) || GameBase.KeyboardState.IsKeyDown(Keys.RightAlt)))
            {
                // Play hover sound effect
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHover);

                // Reset inactive timer.
                TimeInactive = 0;
                
                // If the focused slider is the last one in the list, we set it to the first.
                if (FocusedSlider == Sliders.Last())
                {
                    FocusedSlider = Sliders.First();
                    return;
                }
                
                var index = Sliders.IndexOf(FocusedSlider);
                FocusedSlider = Sliders[index + 1];
            }
        }

        /// <summary>
        ///     Responsible for updating the sound of the focused slider.
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateVolume(int amount)
        {
            FocusedSlider.BindedValue.Value += amount;
            TimeElapsedSinceLastVolumeChange = 0;
            TimeInactive = 0;
        }
        
        /// <summary>
        ///     Makes sure the slider colours highlighted/normal for each slider.
        /// </summary>
        private void ChangeFocusedSliderColor()
        {
            // Change unfocused sliders
            foreach (var slider in Sliders.FindAll(x => x != FocusedSlider).ToList())
                slider.ChangeColor(Color.White);
            
            // Change focused sliders.
            FocusedSlider.ChangeColor(Color.Yellow);
        }

        /// <summary>
        ///    Controls the fadeout of the volume controller after it has been inactive
        ///     for a period of time. 
        /// </summary>
        private void HandleInactiveFadeOut(double dt)
        {
            if (TimeInactive >= 1500)
            {
                SurroundingBox.Alpha = GraphicsHelper.Tween(0, SurroundingBox.Alpha, Math.Min(dt / 30, 1));
    
                if (SurroundingBox.Alpha >= 0.01f) 
                    return;
                
                SurroundingBox.Visible = false;
                TimeInactive = 0;
            }
        }

        /// <summary>
        ///     Controls the FadeIn effect of the volume controller.
        /// </summary>
        /// <param name="dt"></param>
        private void HandleFadeIn(double dt)
        {
            if (!IsFadingIn)
                return;
            
            // Set the box to be visible of course.
            SurroundingBox.Visible = true;
            
            // Begin tweening to fade in the box.
            SurroundingBox.Alpha = GraphicsHelper.Tween(1, SurroundingBox.Alpha, Math.Min(dt / 60, 1));

            // When the box is fully apparent, then we can stop the fade effect.
            if (SurroundingBox.Alpha >= 0.98)
                IsFadingIn = false;
        }
    }
}