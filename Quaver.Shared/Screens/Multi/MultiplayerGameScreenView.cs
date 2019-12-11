using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Multi.UI.Chat;
using Quaver.Shared.Screens.Multi.UI.Players;
using Quaver.Shared.Screens.Multi.UI.Settings;
using Quaver.Shared.Screens.Multi.UI.Status;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Multi
{
    public class MultiplayerGameScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private MultiplayerGameScreen GameScreen => (MultiplayerGameScreen) Screen;

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
        private MultiplayerGameStatusPanel StatusPanel { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerMatchSettings MatchSettings { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerPlayerList PlayerList { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerChatBox Chat { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MultiplayerGameScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateStatusPanel();
            CreateMatchSettings();
            CreatePlayerList();
            CreateChat();

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
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.Triangles, 0, false)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new TestMenuBorderFooter()
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateStatusPanel() => StatusPanel = new MultiplayerGameStatusPanel(GameScreen.Game)
        {
            Parent = Container,
            Y = Header.Y + Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateMatchSettings() => MatchSettings = new MultiplayerMatchSettings(GameScreen.Game)
        {
            Parent = Container,
            Y = StatusPanel.Y + StatusPanel.Height + 20,
            X = 50
        };

        /// <summary>
        /// </summary>
        private void CreatePlayerList()
        {
            PlayerList = new MultiplayerPlayerList(GameScreen.Game)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MatchSettings.Y,
                X = -MatchSettings.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateChat()
        {
            Chat = new MultiplayerChatBox(GameScreen.Game)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = PlayerList.Y + PlayerList.Height + 20,
                X = -MatchSettings.X
            };
        }
    }
}