using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Results.UI.Header;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Results
{
    public class ResultsScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private ResultsScreen ResultsScreen => (ResultsScreen) Screen;

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
        private ResultsScreenHeader ScreenHeader { get; set; }

        /// <summary>
        ///     The width of the screen content
        /// </summary>
        public static int CONTENT_WIDTH { get; } = 1670;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateScreenHeader();

            Header.Parent = Container;
            Footer.Parent = Container;
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
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.Triangles, 0,false)
        {
            Parent = Container,
            Y = -10,
            X = -6
        };

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new TestMenuBorderFooter
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateScreenHeader() => ScreenHeader = new ResultsScreenHeader(ResultsScreen.Map, ResultsScreen.Processor,
            ResultsScreen.ActiveTab)
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            Y = Header.Height
        };
    }
}