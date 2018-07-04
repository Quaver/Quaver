using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Overlays.BottomBar;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Options;
using Quaver.States.Select;
using Quaver.States.Tests;

namespace Quaver.States.Menu
{
    internal class MainMenuScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.Menu;

        /// <inheritdoc />
        /// <summary>
        ///     Update Ready
        /// </summary>
        public bool UpdateReady { get; set; }

        /// <summary>
        ///     QuaverContainer
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        ///     The main menu background.
        /// </summary>
        private Background Background { get; set; }

        /// <summary>
        ///     The toolbar for this screen.
        /// </summary>
        private Toolbar Toolbar { get; set; }

        /// <summary>
        ///     The bottom bar for this screen.
        /// </summary>
        private BottomBar BottomBar { get; set; }

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
            DiscordManager.Presence.Details = "Main Menu";
            DiscordManager.Presence.State = "In the menus";
            DiscordManager.Presence.Timestamps = null;
            DiscordManager.Client.SetPresence(DiscordManager.Presence);

            //Initialize Menu Screen
            Container = new Container();
            CreateUI();

            UpdateReady = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Unload
        /// </summary>
        public void UnloadContent()
        {
            Container.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double dt)
        {
            Container.Update(dt);
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.DarkSlateBlue);
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, GameBase.GraphicsDevice.RasterizerState);
            
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Initializes the UI for this state
        /// </summary>
        private void CreateUI()
        {
            // TODO: Use an actual background instead of loading from file.
            Background = new Background(GraphicsHelper.LoadTexture2DFromFile(@"c:\users\admin\desktop\aaaddd.png"), 30) { Parent = Container };
            
            Toolbar = new Toolbar(new List<ToolbarItem>
                {
                    new ToolbarItem("Home", () => GameBase.GameStateManager.ChangeState(new MainMenuScreen()), true),
                    new ToolbarItem("Play", () => GameBase.GameStateManager.ChangeState(new SongSelectState())),
                    new ToolbarItem("Edit", () => GameBase.GameStateManager.ChangeState(new MainMenuScreen())),
                    new ToolbarItem("Leaderboard", () => { })
                },
                new List<ToolbarItem>
                {
                    new ToolbarItem(FontAwesome.PowerOff, QuaverGame.Quit),
                    new ToolbarItem(FontAwesome.Cog, () => {}),               
                }
            ) { Parent = Container };

            BottomBar = new BottomBar { Parent = Container };
        }
    }
}
