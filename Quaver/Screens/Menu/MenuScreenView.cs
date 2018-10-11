using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Screens.Menu.UI.Buttons;
using Quaver.Screens.Menu.UI.Dialogs;
using Quaver.Screens.Menu.UI.Jukebox;
using Quaver.Screens.Menu.UI.Navigation;
using Quaver.Screens.Menu.UI.Panels;
using Quaver.Screens.Menu.UI.Tips;
using Quaver.Screens.Menu.UI.Visualizer;
using Quaver.Screens.Options;
using Quaver.Screens.Select;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Menu
{
    public class MenuScreenView : ScreenView
    {
        /// <summary>
        ///     The main menu background.
        /// </summary>
        public BackgroundImage Background { get; set; }

        /// <summary>
        ///    The navbar at the top of the screen.
        /// </summary>
        public Navbar Navbar { get; set; }

        /// <summary>
        ///     The line on the bottom.
        /// </summary>
        public Line BottomLine { get; set; }

        /// <summary>
        ///     The container for the screen content.
        /// </summary>
        public ScrollContainer MiddleContainer { get; set; }

        /// <summary>
        ///     The menu tip displayed at the bottom.
        /// </summary>
        public MenuTip Tip { get; set; }

        /// <summary>
        ///    The button to exit the game.
        /// </summary>
        public ToolButton PowerButton { get; set; }

        /// <summary>
        ///     The button to access the game settings.
        /// </summary>
        public ToolButton SettingsButton { get; set; }

        /// <summary>
        ///     Contains all the panels for the screen.
        /// </summary>
        public PanelContainer PanelContainer { get; set; }

        /// <summary>
        ///     The text that says "Main Menu"
        /// </summary>
        public SpriteTextBitmap MainMenuText { get; set; }

        /// <summary>
        ///     The jukebox to play music.
        /// </summary>
        public Jukebox Jukebox { get; set; }

        /// <summary>
        ///     The audio visualizer for the music
        /// </summary>
        public MenuAudioVisualizer Visualizer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MenuScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateAudioVisualizer();
            CreateNavbar();
            CreateLines();
            CreateMiddleContainer();
            CreateMenuTip();
            CreateToolButtons();
            CreatePanelContainer();
            CreateJukebox();
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
        ///     Create
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.ConnectingBackground, 15, false)
        {
            Parent = Container,
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied }
        };

        /// <summary>
        ///     Creates the navbar.
        /// </summary>
        private void CreateNavbar() => Navbar = new Navbar(new List<NavbarItem>
        {
            new NavbarItem("Home", true),
            new NavbarItem("Download Maps"),
            new NavbarItem("Leaderboard"),
            new NavbarItem("Challenges"),
        }, new List<NavbarItem>
        {
            new NavbarItemUser()
        }) { Parent = Container };

        /// <summary>
        ///     Creates the top and bottom lines.
        /// </summary>
        private void CreateLines()
        {
            BottomLine = new Line(Vector2.Zero, Color.LightGray, 2)
            {
                Parent = Container,
                Position = new ScalableVector2(65, WindowManager.Height - 65),
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0.65f
            };

            BottomLine.EndPosition = new Vector2(WindowManager.Width - BottomLine.X, BottomLine.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates a ScrollContainer for all of the content int he middle of the screen.
        /// </summary>
        private void CreateMiddleContainer() => MiddleContainer = new ScrollContainer(
                new ScalableVector2(Navbar.Line.EndPosition.X - Navbar.Line.X, BottomLine.Y - Navbar.Line.Y),
                new ScalableVector2(Navbar.Line.EndPosition.X - Navbar.Line.X, BottomLine.Y - Navbar.Line.Y))
        {
            Parent = Container,
            Position = new ScalableVector2(Navbar.Line.X, Navbar.Line.Y),
            Alpha = 0,
            SpriteBatchOptions = { BlendState = BlendState.NonPremultiplied },
        };

        /// <summary>
        ///     Creates the menu tip
        /// </summary>
        private void CreateMenuTip()
        {
            Tip = new MenuTip()
            {
                Alignment = Alignment.BotLeft,
            };

            Tip.Y = Tip.Height;
            Tip.Transformations.Add(new Transformation(TransformationProperty.Y, Easing.EaseOutQuint, Tip.Y, -5, 1100));

            MiddleContainer.AddContainedDrawable(Tip);
        }

        /// <summary>
        ///     Creates tool buttons to the bottom right of the screen.
        /// </summary>
        private void CreateToolButtons()
        {
            const int targetY = -5;
            const int animationTime = 1100;

            PowerButton = new ToolButton(FontAwesome.PowerOff, (o, e) => DialogManager.Show(new QuitDialog()))
            {
                Alignment = Alignment.BotRight,
            };

            PowerButton.Y = PowerButton.Height;

            // Add transformation to move it up.
            PowerButton.Transformations.Add(new Transformation(TransformationProperty.Y, Easing.EaseOutQuint,
                PowerButton.Y, targetY, animationTime));

            MiddleContainer.AddContainedDrawable(PowerButton);

            // Create settings button
            SettingsButton = new ToolButton(FontAwesome.Cog, (o, e) => DialogManager.Show(new OptionsDialog(0.75f)))
            {
                Parent = MiddleContainer,
                Alignment = Alignment.BotRight,
                Y = PowerButton.Y,
                X = PowerButton.X - PowerButton.Width - 5,
                Transformations =
                {
                    new Transformation(TransformationProperty.Y, Easing.EaseOutQuint, PowerButton.Y, targetY, animationTime)
                }
            };

            MiddleContainer.AddContainedDrawable(SettingsButton);
        }

        /// <summary>
        ///     Creates the panels for the screen.
        /// </summary>
        private void CreatePanelContainer() => PanelContainer = new PanelContainer(new List<Panel>()
        {
            // Single Player
            new Panel("Single Player", "Play offline and compete for scoreboard ranks",
                UserInterface.ThumbnailSinglePlayer, OnSinglePlayerPanelClicked),

            // Compettive
            new Panel("Competitive", "Compete against players all over the world",
                UserInterface.ThumbnailCompetitive, OnCompetitivePanelClicked),

            // Custom Games
            new Panel("Custom Games", "Play multiplayer games with your friends",
                UserInterface.ThumbnailCustomGames, OnCustomGamesPanelClicked),

            // Editor
            new Panel("Editor", "Create or edit a map to any song you'd like",
                UserInterface.ThumbnailEditor, OnEditorPanelClicked),
        })
        {
            Parent = Container
        };

        /// <summary>
        ///     Creates the text that says "Main Menu"
        /// </summary>
        private void CreateTextMainMenu()
        {
            var mainMenuBackground = new Sprite()
            {
                Parent = Container,
                X = 62,
                Y = Navbar.Line.Y + 20,
                Tint = Color.Black,
                Alpha = 0.0f
            };

            MainMenuText = new SpriteTextBitmap(BitmapFonts.Exo2BoldItalic, "Main Menu", 32, ColorHelper.HexToColor("#7ebfe0"),
                Alignment.MidLeft, int.MaxValue)
            {
                Parent = mainMenuBackground,
                Alignment = Alignment.MidCenter,
                SpriteBatchOptions = new SpriteBatchOptions()
                {
                    BlendState = BlendState.NonPremultiplied
                },
            };

            mainMenuBackground.Size = new ScalableVector2(MainMenuText.Width + 10, MainMenuText.Height + 10);
        }

        /// <summary>
        ///     Creates and aligns the jukebox sprite.
        /// </summary>
        private void CreateJukebox() => Jukebox = new Jukebox
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Y = Navbar.Line.Y + 20,
            X = -65
        };

        /// <summary>
        ///     Creates the audio visaulizer container for the screen
        /// </summary>12
        private void CreateAudioVisualizer() => Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 400, 150, 5)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        ///     Called when the single player panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSinglePlayerPanelClicked(object sender, EventArgs e)
        {
            if (MapManager.Mapsets.Count == 0 || MapManager.Selected == null || MapManager.Selected.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You have no maps loaded. Try downloading some!");
                return;
            }

            QuaverScreenManager.ChangeScreen(new SelectScreen());
        }

        /// <summary>
        ///     Called when the competitive panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCompetitivePanelClicked(object sender, EventArgs e)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back later.");
        }

        /// <summary>
        ///     Called when the custom games panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCustomGamesPanelClicked(object sender, EventArgs e)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back later.");
        }

        /// <summary>
        ///     Called when the editor panel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnEditorPanelClicked(object sender, EventArgs e)
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet. Check back later.");
        }
    }
}