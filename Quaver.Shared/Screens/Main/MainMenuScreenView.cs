using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Main.UI;
using Quaver.Shared.Screens.Main.UI.Panels;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

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
        private MenuPanelContainer PanelContainer { get; set; }

        /// <summary>
        /// </summary>
        private MenuAudioVisualizer Visualizer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MainMenuScreenView(QuaverScreen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateAudioVisualizer();
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
            Container?.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
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
                Y = Header.Height
            };
        }

        /// <summary>
        ///     Creates <see cref="Visualizer"/>
        /// </summary>
        private void CreateAudioVisualizer()
        {
            Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 600, 58, 3, 8)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height
            };

            Visualizer.Bars.ForEach(x => x.Alpha = 0.35f);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            PanelContainer.MoveToX(PanelContainer.Width + 400, Easing.OutQuint, 450);
        }
    }
}