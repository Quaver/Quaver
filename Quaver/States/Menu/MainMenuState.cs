using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Commands;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Enums;
using Quaver.States.Options;
using Quaver.States.Select;
using Quaver.States.Tests;

namespace Quaver.States.Menu
{
    internal class MainMenuState : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.MainMenu;

        /// <inheritdoc />
        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     QuaverContainer
        /// </summary>
        private QuaverContainer QuaverContainer { get; set; }

        /// <summary>
        ///     The navbar at the top of the screen.
        /// </summary>
        private Nav Nav { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            GameBase.GameWindow.Title = "Quaver";

            // Remove speed mods upon going to the main menu so songs can be played at normal speed.
            if (GameBase.CurrentGameModifiers.Count > 0)
                ModManager.RemoveSpeedMods();

            // Set Discord RP
            DiscordController.ChangeDiscordPresence("Main Menu", "In the menus");

#if DEBUG
            // Enable console commands (Only applicable if on debug release)
            CommandHandler.HandleConsoleCommand();
#endif

            //Initialize Menu Screen
            QuaverContainer = new QuaverContainer();
            CreateUI();

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            QuaverContainer.Destroy();
            Nav.UnloadContent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            QuaverContainer.Update(dt);
            Nav.Update(dt);
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkSlateBlue);
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, GameBase.GraphicsDevice.RasterizerState);
            
            QuaverContainer.Draw();
            Nav.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Initializes the UI for this state
        /// </summary>
        private void CreateUI()
        {
            // Create navbar
            Nav = new Nav();
            Nav.Initialize(this);
        }
    }
}
