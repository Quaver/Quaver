using System;
using Microsoft.Xna.Framework;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States;

namespace Quaver.Graphics.Overlays.Navbar
{
    /// <summary>
    ///     A navbar overlay
    /// </summary>
    internal class Navbar : IGameStateComponent
    {
        /// <summary>
        ///     The actual navbar sprite
        /// </summary>
        internal QuaverSprite Nav { get; set; }

        /// <summary>
        ///     The container for the navbar
        /// </summary>
        private QuaverContainer Container { get; set; }

         /// <summary>
        ///     Init
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(IGameState state)
        {
            Container = new QuaverContainer();
            
            Nav = new QuaverSprite()
            {
                Size = new UDim2D(0, 50, 1, 0),
                Alignment = Alignment.TopLeft,
                Tint = new Color(0f, 0f, 0f, 0.5f),
                Parent = Container
            };
            
            // Replace with actual sprites
            var home = new NavbarButton(this, GameBase.QuaverUserInterface.BlankBox, "Home", "Go to the main menu")
            {
                Parent = Nav
            };

            var play = new NavbarButton(this, GameBase.QuaverUserInterface.BlankBox, "Play", "Play Quaver")
            {
                Parent = Nav
            };

            var test = new NavbarButton(this, GameBase.QuaverUserInterface.BlankBox, "Meme", "dd")
            {
                Parent = Nav
            };
        }

         /// <summary>
        ///     Unload
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        public void Update(double dt)
        {
            Container.Update(dt);
        }

        public void Draw()
        {
            Container.Draw();
        }
    }
}