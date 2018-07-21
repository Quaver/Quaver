using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Discord;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.BottomBar;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Graphics.UI;
using Quaver.Graphics.UI.Notifications;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Menu;
using Quaver.States.Select;

namespace Quaver.States.Tests
{
    internal class NotificationTestScreen : IGameState
    {
        /// <inheritdoc />
        /// <summary>
        ///     State
        /// </summary>
        public State CurrentState { get; set; } = State.Test;

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
            Background = new Background(UserInterface.MenuBackground, 30) { Parent = Container };

            Toolbar = new Toolbar(new List<ToolbarItem>
                {
                    new ToolbarItem("Home", () => GameBase.GameStateManager.ChangeState(new MainMenuScreen()), true),
                    new ToolbarItem("Error", () => NotificationManager.Show(NotificationLevel.Error, "Oh no! Help! The chicken is burning!")),
                    new ToolbarItem("Warning", () => NotificationManager.Show(NotificationLevel.Warning, "CRIME SCENE! DO NOT CROSS!")),
                    new ToolbarItem("Success", () => NotificationManager.Show(NotificationLevel.Success, "You just won a BRAND NEW CAR!")),
                    new ToolbarItem("Info", () => NotificationManager.Show(NotificationLevel.Info, "Knowledge is Power")),
                    new ToolbarItem("Default", () => NotificationManager.Show(NotificationLevel.Default, "Just a default notification for anything.",
                        (sender, e) =>
                        {
                            Logger.LogImportant("It was clicked! Whoa!", LogType.Runtime);
                        })),
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
