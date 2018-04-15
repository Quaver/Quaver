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
        ///     The slider that controls the master volume.
        /// </summary>
        private QuaverSlider MasterVolumeSlider { get; set; }

        /// <summary>
        ///     The slider that controls the music volume.
        /// </summary>
        private QuaverSlider MusicVolumeSlider { get; set; }

        /// <summary>
        ///     The slider that controls the sound effects.
        /// </summary>
        private QuaverSlider EffectVolumeSlider { get; set; }

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
        ///     The spacing between each slider.
        /// </summary>
        private int SliderYSpacing { get; } = 60;

        /// <summary>
        ///     The slider's x position.
        /// </summary>
        private int SliderPosX { get; } = -60;

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

            // Create master volume slider.
            MasterVolumeSlider = new QuaverSlider(ConfigManager.VolumeGlobal, SliderSize, SliderColor, SliderProgressColor, false, FontAwesome.Volume)
            {
                Parent = Container,
                Alignment = Alignment.BotRight,
                PosY = -180,
                PosX = SliderPosX
            };

            // Create the music volume slider.
            MusicVolumeSlider = new QuaverSlider(ConfigManager.VolumeMusic, SliderSize, SliderColor, SliderProgressColor, false, FontAwesome.Music)
            {
                Parent = Container,
                Alignment = Alignment.BotRight,
                PosY = MasterVolumeSlider.PosY + SliderYSpacing,
                PosX = SliderPosX
            };

            // Create the effect volume slider.
            EffectVolumeSlider = new QuaverSlider(ConfigManager.VolumeEffect, SliderSize, SliderColor, SliderProgressColor, false, FontAwesome.Headphones)
            {
                Parent = Container,
                Alignment = Alignment.BotRight,
                PosY = MasterVolumeSlider.PosY + SliderYSpacing * 2,
                PosX = SliderPosX
            };
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