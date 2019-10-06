using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Main.UI.Panels;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Input;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Main
{
    public class MainMenuScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Footer { get; set; }

        /// <summary>
        /// </summary>
        private Jukebox Jukebox { get; }

        /// <summary>
        /// </summary>
        private MenuPanelContainer PanelContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MainMenuScreenView(QuaverScreen screen) : base(screen)
        {
            Jukebox = new Jukebox(true);

            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreatePanelContainer();

            Header.Parent = Container;
            Footer.Parent = Container;

            screen.ScreenExiting += OnScreenExiting;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Jukebox.Update(gameTime);
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#242424"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        ///     Creates <see cref="Background"/>
        /// </summary>
        private void CreateBackground()
            => Background = new BackgroundImage(UserInterface.Triangles, 0, false) { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain() { Parent = Container };

        /// <summary>
        ///     Creates <see cref="Footer"/>
        /// </summary>
        private void CreateFooter() => Footer = new MainMenuFooter()
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreatePanelContainer()
        {
            PanelContainer = new MenuPanelContainer((MainMenuScreen) Screen)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = Header.Height
            };

            PanelContainer.X = PanelContainer.Width + 50;
            PanelContainer.MoveToX(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            PanelContainer.MoveToX(PanelContainer.Width + 50, Easing.OutQuint, 600);
        }
    }
}