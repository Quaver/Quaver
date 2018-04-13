using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Tests
{
    internal class SliderTestState : IGameState
    {
        /// <summary>
        ///     Test Screen
        /// </summary>
        public State CurrentState { get; set; } = State.TestScreen;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        private QuaverContainer Container { get; set; }

        /// <summary>
        ///     Navbar sprite
        /// </summary>
        private QuaverSlider Slider { get; set; }
        
        public void Initialize()
        {
            Container = new QuaverContainer();
            Slider = new QuaverSlider(ConfigManager.VolumeGlobal);
            Slider.Initialize(this);
            
            UpdateReady = true;
        }

        public void UnloadContent()
        {
        }

        public void Update(double dt)
        {
            Slider.Update(dt);
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            Slider.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}