using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Replays;
using Quaver.Assets;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Notifications;
using Quaver.Graphics.Overlays.Toolbar;
using Quaver.Parsers.Etterna;
using Quaver.Parsers.Osu;
using Quaver.Screens.Edit;
using Quaver.Screens.Loading;
using Quaver.Screens.Menu.UI.BottomToolbar;
using Quaver.Screens.Menu.UI.Buttons.Navigation;
using Quaver.Screens.Options;
using Quaver.Screens.Results;
using Quaver.Screens.Select;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;
using Screen = Wobble.Screens.Screen;

namespace Quaver.Screens.Menu
{
    public class MainMenuScreenView : ScreenView
    {
        /// <summary>
        ///     The background image for this screen.
        /// </summary>
        private BackgroundImage Background { get; }

        /// <summary>
        ///     The top toolbar for this screen.
        /// </summary>
        private Toolbar Toolbar { get; set;  }

        /// <summary>
        ///     The bottom toolbar for this screen.
        /// </summary>
        private BottomBar BottomToolbar { get; set; }

        /// <summary>
        ///     The container that holds all of the navigation buttons to go to new
        ///     screens.
        /// </summary>
        private NavigationButtonContainer NavigationButtonContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MainMenuScreenView(Screen screen) : base(screen)
        {
            Background = new BackgroundImage(UserInterface.MenuBackground, 30) { Parent = Container };
            CreateToolbar();
            CreateNavigationButtonContainer();
            BottomToolbar = new BottomBar { Parent = Container };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates the toolbar for the screen.
        /// </summary>
        private void CreateToolbar() => Toolbar = new Toolbar(new List<ToolbarItem>
        {
            new ToolbarItem("Home", () => Console.WriteLine("Already Home!"), true),
        }, new List<ToolbarItem>
        {
            new ToolbarItem(FontAwesome.PowerOff, GameBase.Game.Exit),
            new ToolbarItem(FontAwesome.Cog, () => DialogManager.Show(new OptionsDialog(0.75f)))
        }, new ScalableVector2(WindowManager.Width, 80))
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the entire navigation button container along
        ///     with its buttons.
        /// </summary>
        private void CreateNavigationButtonContainer()
        {
            // Editor
            var editor = new NavigationButton(new Vector2(325, 230), "Editor", UserInterface.MenuCompetitive,
                "Create or edit a map to any song you'd like!", EditorScreen.Go)
            {
                Alignment = Alignment.TopCenter,
                X = 0,
                Y = Toolbar.Y + Toolbar.Height + 60
            };

            // Single Player.
            var singlePlayer = new NavigationButton(new Vector2(325, 230), "Single Player",
                UserInterface.MenuSinglePlayer, "Play offline and compete for scoreboard ranks!",
                () =>
                {
                    QuaverScreenManager.ChangeScreen(new SelectScreen());
                })
            {
                Alignment = Alignment.TopCenter,
                X = editor.X - editor.Width - 30,
                Y = editor.Y
            };

            // Competitve
            var competitive = new NavigationButton(new Vector2(325, 230), "Competitive", UserInterface.MenuLock,
                "Compete against the world and rank up!", () =>
                {
                    NotificationManager.Show(NotificationLevel.Info, "This isn't implemented yet. Check back later!");
                }, true)
            {
                Alignment = Alignment.TopCenter,
                X = editor.X,
                Y = editor.Y + editor.Height + 30
            };

            // Multiplayer
            var customGames = new NavigationButton(new Vector2(325, 230), "Custom Games", UserInterface.MenuLock,
                "Play casually with your friends online!", () =>
                {
                    NotificationManager.Show(NotificationLevel.Info, "This isn't implemented yet. Check back later!");
                }, true)
            {
                Alignment = Alignment.TopCenter,
                X = singlePlayer.X,
                Y = singlePlayer.Y + singlePlayer.Height + 30
            };

            // News
            var news = new NavigationButton(new Vector2(250, 490), "Latest News",
                UserInterface.MenuNews, "Keep up-to-date wih Quaver!", () =>
                {
                    NotificationManager.Show(NotificationLevel.Info, "This isn't implemented yet. Check back later!");
                }, true)
            {
                Alignment = Alignment.TopCenter,
                Y = singlePlayer.Y,
                X = editor.Width + editor.X + 15
            };

            // Create a new button container with all of the created buttons.
            NavigationButtonContainer = new NavigationButtonContainer(new List<NavigationButton>
            {
                editor,
                singlePlayer,
                competitive,
                customGames,
                news
            })
            { Parent = Container };
        }
    }
}
