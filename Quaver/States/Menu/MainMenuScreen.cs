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
using Quaver.Graphics.Text;
using Quaver.Graphics.UI;
using Quaver.Graphics.UI.Notifications;
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

        /// <summary>
        ///     
        /// </summary>
        private NavigationButtonContainer NavigationButtonContainer { get; set; }

        /// <inheritdoc />
        /// <summary>  
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
            CreateInterface();

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
        public void Update(double dt)
        {
            GameBase.Navbar.PerformHideAnimation(dt);
            Container.Update(dt);
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Draw
        /// </summary>
        public void Draw()
        {
            GameBase.GraphicsDevice.Clear(Color.Black);
            GameBase.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, GameBase.GraphicsDevice.RasterizerState);
            
            Container.Draw();
            
            GameBase.SpriteBatch.End();
        }

        /// <summary>
        ///     Initializes the UI for this state
        /// </summary>
        private void CreateInterface()
        {
            Background = new Background(GameBase.QuaverUserInterface.MenuBackground, 30) { Parent = Container };
            
            Toolbar = new Toolbar(new List<ToolbarItem>
                {
                    new ToolbarItem("Home", () => GameBase.GameStateManager.ChangeState(new MainMenuScreen()), true),
                    new ToolbarItem("Debug", () => { GameBase.GameStateManager.ChangeState(new DebugScreen()); })
                },
                new List<ToolbarItem>
                {
                    new ToolbarItem(FontAwesome.PowerOff, QuaverGame.Quit),
                    new ToolbarItem(FontAwesome.Cog, () => {}), 
                }
            ) { Parent = Container };

            BottomBar = new BottomBar { Parent = Container };
                
            CreateNavigationButtons();
        }

        /// <summary>
        ///     Initializes the navigation buttons and its container.
        /// </summary>
        private void CreateNavigationButtons()
        {          
            // Competitive
            var competitive = new NavigationButton(new Vector2(325, 230), "Competitive", GameBase.QuaverUserInterface.MenuCompetitive,
                "Compete against the world and rank up!", () => { })
            {
                Alignment = Alignment.TopCenter,
                PosX = 0,
                PosY = Toolbar.PosY + Toolbar.SizeY + 60
            };
            
            // Single Player.
            var singlePlayer = new NavigationButton(new Vector2(325, 230), "Single Player", 
                GameBase.QuaverUserInterface.MenuSinglePlayer, "Play offline and compete for scoreboard ranks!",
                () => GameBase.GameStateManager.ChangeState(new SongSelectState()))
            {
                Alignment = Alignment.TopCenter,
                PosX = competitive.PosX - competitive.SizeX - 30,
                PosY = competitive.PosY
            };

            // Editor
            var edit = new NavigationButton(new Vector2(325, 230), "Map Editor", GameBase.QuaverUserInterface.MenuLock,
                "Create or edit a map to any song you'd like!", () => { })
            {
                Alignment = Alignment.TopCenter,
                PosX = competitive.PosX,
                PosY = competitive.PosY + competitive.SizeY + 30
            };
            
            // Multiplayer
            var multiplayer = new NavigationButton(new Vector2(325, 230), "Multiplayer", GameBase.QuaverUserInterface.MenuMultiplayer,
                "Play casually with your friends online!", () => { })
            {
                Alignment = Alignment.TopCenter,
                PosX = singlePlayer.PosX,
                PosY = singlePlayer.PosY + singlePlayer.SizeY + 30
            };

            // News
            var news = new NavigationButton(new Vector2(250, 490), "Latest News",
                GameBase.QuaverUserInterface.MenuNews, "Keep up-to-date wih Quaver!", () =>
                {
                    Logger.LogImportant($"This is not implemented yet! Check back later!", LogType.Runtime);
                }, true)
            {
                Alignment = Alignment.TopCenter,
                PosY = singlePlayer.PosY,
                PosX = competitive.SizeX + competitive.PosX + 15
            };
            
            // Create a new button container with all of the created buttons.
            NavigationButtonContainer = new NavigationButtonContainer(new List<NavigationButton>
            {
                competitive,
                singlePlayer,
                edit,
                multiplayer,
                news
            }) { Parent = Container };
        }
    }
}
