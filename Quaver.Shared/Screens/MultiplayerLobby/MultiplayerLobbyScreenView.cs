using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Filter;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Footer;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Games;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Selected;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Scheduling;
using Wobble.Screens;

namespace Quaver.Shared.Screens.MultiplayerLobby
{
    public class MultiplayerLobbyScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private MultiplayerLobbyScreen Lobby => (MultiplayerLobbyScreen) Screen;

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
        private MultiplayerLobbyFilterPanel FilterPanel { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerGameScrollContainer ScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        private SelectedGamePanel SelectedPanel { get; set; }

        /// <summary>
        /// </summary>
        private int ScreenPaddingX = 50;

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MultiplayerLobbyScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateFilterPanel();
            CreateScrollContainer();
            CreateSelectedGamePanel();

            Header.Parent = Container;
            Footer.Parent = Container;
            FilterPanel.Parent = Container;

            FilterPanel.FilterTask.OnCompleted += OnGamesFiltered;
            FilterPanel.StartFilterTask();

            Lobby.ScreenExiting += OnScreenExiting;
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
        private void CreateBackground()
            => Background = new BackgroundImage(UserInterface.Triangles, 0, false) {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new MultiplayerLobbyFooter(Lobby)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateFilterPanel() => FilterPanel = new MultiplayerLobbyFilterPanel(Lobby.VisibleGames)
        {
            Parent = Container,
            Y = Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateScrollContainer()
        {
            ScrollContainer = new MultiplayerGameScrollContainer(Lobby.SelectedGame, Lobby.VisibleGames, FilterPanel.SearchQuery)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Y = FilterPanel.Y + FilterPanel.Height + 4,
                X = ScreenPaddingX
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSelectedGamePanel()
        {
            SelectedPanel = new SelectedGamePanel(Lobby.SelectedGame)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = FilterPanel.Y + FilterPanel.Height + 20
            };

            SelectedPanel.X = SelectedPanel.Width + ScreenPaddingX;
            SelectedPanel.MoveToX(-ScreenPaddingX, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamesFiltered(object sender, TaskCompleteEventArgs<int, int> e)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            ScrollContainer.ClearAnimations();
            ScrollContainer.MoveToX(-ScrollContainer.Width - ScreenPaddingX, Easing.OutQuint, 400);

            SelectedPanel.ClearAnimations();
            SelectedPanel.MoveToX(SelectedPanel.Width + ScreenPaddingX, Easing.OutQuint, 400);
        }
    }
}