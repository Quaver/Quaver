using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.GameState;
using Quaver.Graphics.Buttons.Dropdowns;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Enums;

namespace Quaver.States.Tests
{
    internal class DropdownTestState : IGameState
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

        private QuaverDropdown Dropdown { get; set; }

        public void Initialize()
        {
            Container = new QuaverContainer();
            Nav = new Navbar();
            Nav.Initialize(this);

            Dropdown = new QuaverDropdown(new List<string>() {"hi", "bye"}, (o ,e) => Console.WriteLine("MEMES! " + e.ButtonText))
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
            
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
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }
    }
}