using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States;
using SQLite;

namespace Quaver.Graphics.Overlays.Volume
{
    internal class VolumeController : IGameStateComponent
    {
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
        private List<QuaverSlider> Sliders { get; set; }

        /// <summary>
        ///     The size of each slider.
        /// </summary>
        private Vector2 SliderSize { get; }

        /// <summary>
        ///     The color of the slider line.
        /// </summary>
        private Color SliderColor { get;  }

        /// <summary>
        ///     The color of the slider progress thing.
        /// </summary>
        private Color SliderProgressColor { get; }

        /// <summary>
        ///     The scroll wheel value of the previous frame.
        /// </summary>
        private int PreviousScrollWheelValue { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal VolumeController()
        {
            SliderSize = new Vector2(300, 3);
            SliderColor = Color.White;
            SliderProgressColor = new Color(165, 223, 255);
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
                Parent = Container
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
            HandleInput(dt);
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
        private void HandleInput(double dt)
        {
            // Only have the volume control be activated and usable when holding down either ALT key.
            if (!GameBase.KeyboardState.IsKeyDown(Keys.LeftAlt) && !GameBase.KeyboardState.IsKeyDown(Keys.RightAlt))
            {
                PreviousScrollWheelValue = GameBase.MouseState.ScrollWheelValue;
                return;
            }

            // Handle when the mouse's scroll wheel 
            if (GameBase.MouseState.ScrollWheelValue != PreviousScrollWheelValue)
            {
                // TODO: If not visible, do a fade in effect.
                if (!SurroundingBox.Visible)
                    SurroundingBox.Visible = true;

                // Handle increase
                if (GameBase.MouseState.ScrollWheelValue > PreviousScrollWheelValue)
                {
                    Console.WriteLine("increase");
                    var focusedSlider = GetFocusedSlider();
                    Console.WriteLine(focusedSlider.BindedValue.Name);
                    focusedSlider.BindedValue.Value += 5;
                }
                // Handle decrease.
                else
                {
                    Console.WriteLine("decrease");
                    var focusedSlider = GetFocusedSlider();
                    Console.WriteLine(focusedSlider.BindedValue.Name);
                    focusedSlider.BindedValue.Value -= 5;
                }
            }
            
            PreviousScrollWheelValue = GameBase.MouseState.ScrollWheelValue;
        }

        /// <summary>
        ///     Get the currently focused slider from the list of sliders.
        ///     If none are focused, the default that's returned is the master volume
        ///     slider.
        /// </summary>
        /// <returns></returns>
        private QuaverSlider GetFocusedSlider()
        {
            return Sliders.Find(x => x.IsHovered) ?? MasterVolumeSlider;
        }
        
        /// <summary>
        ///     Makes sure the slider colours highlighted/normal for each slider.
        /// </summary>
        private void ChangeSliderColorsOnHover()
        {
            // Master
            if (MasterVolumeSlider.IsHovered)
            {
                MasterVolumeSlider.ChangeColor(Color.Yellow);
                MasterVolumeSliderIcon.Tint = Color.Yellow;
            }
            else
            {
                MasterVolumeSlider.ChangeColor(Color.White);
                MasterVolumeSliderIcon.Tint = Color.White;
            }
            
            // Music
            if (MusicVolumeSlider.IsHovered)
            {
                MusicVolumeSlider.ChangeColor(Color.Yellow);
                MusicVolumeSliderIcon.Tint = Color.Yellow;
            }
            else
            {
                MusicVolumeSlider.ChangeColor(Color.White);
                MusicVolumeSliderIcon.Tint = Color.White;
            }

            // Effect
            if (EffectVolumeSlider.IsHovered)
            {
                EffectVolumeSlider.ChangeColor(Color.Yellow);
                EffectVolumeSliderIcon.Tint = Color.Yellow;
            }
            else
            {
                EffectVolumeSlider.ChangeColor(Color.White);
                EffectVolumeSliderIcon.Tint = Color.White;
            }
        }
    }
}