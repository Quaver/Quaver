using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Tests
{
    internal class CheckboxTestState : IGameState
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
        private Nav Nav { get; set; }

        public void Initialize()
        {
            Container = new QuaverContainer();
            Nav = new Nav();
            Nav.Initialize(this);
            
            var ds4k = new QuaverCheckbox(ConfigManager.DownScroll4K, new Vector2(20, 20))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };

            var ds4ktext = new QuaverSpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = ds4k.PosX + 120,
                Text = "Toggle DownScroll for 4K",
                Font = QuaverFonts.Medium12
            };
            
            var ds7k = new QuaverCheckbox(ConfigManager.DownScroll7K, new Vector2(20, 20))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosY = ds4k.PosY + 50
            };

            var ds7ktext = new QuaverSpriteText()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                PosX = ds4k.PosX + 120,
                PosY = ds7k.PosY,
                Text = "Toggle DownScroll for 7K",
                Font = QuaverFonts.Medium12
            };
            
            UpdateReady = true;
        }

        public void UnloadContent()
        {
            Nav.UnloadContent();
            Container.Destroy();
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
            
            Nav.Draw();
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}