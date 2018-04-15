using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States;

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
        ///     Ctor - 
        /// </summary>
        internal VolumeController()
        {
            SliderSize = new Vector2(300, 3);
            SliderColor = Color.White;
            SliderProgressColor = new Color(165, 223, 255);
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
            
            #endregion
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
            if (MasterVolumeSlider.IsHovered)
            {
                MasterVolumeSlider.ChangeColor(Color.Yellow);
                MasterVolumeSliderIcon.Tint = Color.Yellow;
            }
            else
            {
                MasterVolumeSlider.ChangeColor(Color.White);
                MasterVolumeSliderIcon.Tint = Color.White;;
            }


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

            
            Container.Update(dt);
        }

        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            Container.Draw();
        }
    }
}