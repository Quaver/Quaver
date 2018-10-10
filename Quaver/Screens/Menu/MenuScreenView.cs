using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Helpers;
using Quaver.Screens.Menu.UI.Buttons;
using Quaver.Screens.Menu.UI.Dialogs;
using Quaver.Screens.Menu.UI.Jukebox;
using Quaver.Screens.Menu.UI.Navigation;
using Quaver.Screens.Menu.UI.Panels;
using Quaver.Screens.Menu.UI.Tips;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MenuScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateNavbar();
            CreateLines();
            CreateMiddleContainer();
            CreateMenuTip();
            CreateToolButtons();
            CreatePanelContainer();
            // CreateTextMainMenu();
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
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackground, 15, false)
        {
            Parent = Container,
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied }
        };

        /// <summary>
        ///     Creates the navbar.
        /// </summary>
        private void CreateNavbar() => Navbar = new Navbar(new List<NavbarItem>()
        {
            new NavbarItem("Home", true),
            new NavbarItem("Leaderboard"),
            new NavbarItem("Challenges"),
            new NavbarItem("Play (Temp)", false, (sender, args) => QuaverScreenManager.ChangeScreen(new SelectScreen()))
        }, new List<NavbarItem>()
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
            SpriteBatchOptions =
            {
                BlendState = BlendState.NonPremultiplied
            },
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
            new Panel("Single Player", "Play offline and compete for scoreboard ranks", UserInterface.ThumbnailSinglePlayer),
            new Panel("Competitive", "Compete against players all over the world", UserInterface.ThumbnailCompetitive),
            new Panel("Custom Games", "Play multiplayer games with your friends", UserInterface.ThumbnailCustomGames),
            new Panel("Editor", "Create or edit a map to any song you'd like", UserInterface.ThumbnailEditor),
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
        private void CreateJukebox()
        {
            Jukebox = new Jukebox()
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                X = 65,
                Y = PanelContainer.Panels.First().Y - PanelContainer.Panels.First().Height / 2f - 38
            };
        }
    }
}