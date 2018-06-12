using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Tests
{
    internal class NavbarTestState : IGameState
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

        /// <summary>
        ///     Navbar sprite
        /// </summary>
        private Nav Nav { get; set; }
        
        public void Initialize()
        {
            Container = new Container();
            Nav = new Nav();
            Nav.Initialize(this);
            
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
            
            Nav.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}