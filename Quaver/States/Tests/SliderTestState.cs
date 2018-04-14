using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
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


        private Navbar Nav { get; set; }

        public void Initialize()
        {
            Container = new QuaverContainer();
            Nav = new Navbar();
            Nav.Initialize(this);
            var sliderBg = new QuaverSlider(ConfigManager.BackgroundBrightness, new Vector2(600, 3), new Color(165, 223, 255), Color.White)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = -10
            };
            
            var sliderVol = new QuaverSlider(ConfigManager.VolumeGlobal, new Vector2(600, 3), new Color(165, 223, 255), Color.White, FontAwesome.Volume)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = -10,
                PosY = 50
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
            Container.Update(dt);
        }

        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkOliveGreen);
            GameBase.SpriteBatch.Begin();
            
            BackgroundManager.Draw();
            Nav.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}