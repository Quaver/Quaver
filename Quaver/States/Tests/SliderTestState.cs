using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Discord;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Overlays.Volume;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UI;
using Quaver.Main;

namespace Quaver.States.Tests
{
    internal class SliderTestState : IGameState
    {
        /// <summary>
        ///     Test Screen
        /// </summary>
        public State CurrentState { get; set; } = State.Test;
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public bool UpdateReady { get; set; }

        private Container Container { get; set; }

        private Nav Nav { get; set; }

        private VolumeController VolumeController { get; set; }

        public void Initialize()
        {            
            Container = new Container();
            Nav = new Nav();
            Nav.Initialize(this);
            
            VolumeController = new VolumeController();
            VolumeController.Initialize(this);
            
            var sliderBg = new Slider(ConfigManager.BackgroundBrightness, new Vector2(300, 3))
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