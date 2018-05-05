using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Overlays.Volume;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
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

        private Nav Nav { get; set; }

        private VolumeController VolumeController { get; set; }

        public void Initialize()
        {            
            Container = new QuaverContainer();
            Nav = new Nav();
            Nav.Initialize(this);
            
            VolumeController = new VolumeController();
            VolumeController.Initialize(this);
            
            var sliderBg = new QuaverSlider(ConfigManager.BackgroundBrightness, new Vector2(300, 3))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = -10
            };
                        
            // Pick first map and select it
            UpdateReady = true;
        }

        public void UnloadContent()
        {
        }

        public void Update(double dt)
        {
            Nav.Update(dt);
            VolumeController.Update(dt);
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Nav.Draw();
            VolumeController.Draw();
            Container.Draw();
         
            GameBase.SpriteBatch.End();
        }
    }
}