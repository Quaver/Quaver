using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
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

        private Navbar Nav { get; set; }

        public void Initialize()
        {
            Container = new QuaverContainer();
            Nav = new Navbar();
            Nav.Initialize(this);
            Slider = new QuaverSlider(ConfigManager.BackgroundBrightness);
            Slider.Initialize(this);
            
            // Pick first map and select it
            Map.ChangeSelected(GameBase.Mapsets[0].Maps[0]);
            BackgroundManager.LoadBackground();
            BackgroundManager.Change(GameBase.CurrentBackground);
            UpdateReady = true;
        }

        public void UnloadContent()
        {
        }

        public void Update(double dt)
        {
            Nav.Update(dt);
            Slider.Update(dt);
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Nav.Draw();
            Slider.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}