using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
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
        private Navbar Nav { get; set; }

        public void Initialize()
        {
            Container = new QuaverContainer();
            Nav = new Navbar();
            Nav.Initialize(this);
            
            var mem = new QuaverCheckbox(ConfigManager.DownScroll4K, new Vector2(40, 40))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
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