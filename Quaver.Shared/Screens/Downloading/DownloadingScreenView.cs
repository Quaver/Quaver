using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Downloading.UI.Filter;
using Quaver.Shared.Screens.Downloading.UI.Footer;
using Quaver.Shared.Screens.Downloading.UI.Mapsets;
using Quaver.Shared.Screens.Downloading.UI.Search;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Downloading
{
    public class DownloadingScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        public DownloadingScreen DownloadingScreen => (DownloadingScreen)Screen;

        /// <summary>
        /// </summary>
        private BackgroundImage Triangles { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Footer { get; set; }

        /// <summary>
        /// </summary>
        private DownloadSearchPanel SearchPanel { get; set; }

        /// <summary>
        /// </summary>
        private DownloadFilterContainer FilterContainer { get; set; }

        /// <summary>
        /// </summary>
        private DownloadableMapsetContainer MapsetContainer { get; set; }

        /// <summary>
        /// </summary>
        private const int ScreenPaddingX = 50;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public DownloadingScreenView(Screen screen) : base(screen)
        {
            CreateTriangles();
            CreateHeader();
            CreateFooter();
            CreateSearchPanel();
            CreateFilterContainer();
            CreateMapsetContainer();

            Header.Parent = Container;
            Footer.Parent = Container;
            SearchPanel.Parent = Container;

            DownloadingScreen.ScreenExiting += OnScreenExiting;
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
        private void CreateTriangles() => Triangles = new BackgroundImage(UserInterface.Triangles, 0, false) { Parent = Container };

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain { Parent = Container };

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new DownloadingFooter
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateSearchPanel() => SearchPanel = new DownloadSearchPanel(DownloadingScreen.CurrentSearchQuery,
            DownloadingScreen.FilterGameMode, DownloadingScreen.FilterRankedStatus, DownloadingScreen.Mapsets,
            DownloadingScreen.SelectedMapset, DownloadingScreen.SortBy, DownloadingScreen.ReverseSort)
        {
            Parent = Container,
            Y = Header.Y + Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateFilterContainer()
        {
            FilterContainer = new DownloadFilterContainer(DownloadingScreen.MinDifficulty, DownloadingScreen.MaxDifficulty,
                DownloadingScreen.MinBpm, DownloadingScreen.MaxBpm, DownloadingScreen.MinLength, DownloadingScreen.MaxLength,
                DownloadingScreen.MinLongNotePercent, DownloadingScreen.MaxLongNotePercent,
                DownloadingScreen.MinPlayCount, DownloadingScreen.MaxPlayCount, DownloadingScreen.MinUploadDate,
                DownloadingScreen.MaxUploadDate, DownloadingScreen.SelectedMapset, DownloadingScreen.DisplayOwnedMapsets, 
                DownloadingScreen.MinLastUpdateDate, DownloadingScreen.MaxLastUpdateDate,
                DownloadingScreen.MinCombo, DownloadingScreen.MaxCombo, DownloadingScreen.DisplayExplicitMapsets)
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Y = SearchPanel.Y + SearchPanel.Height + 20,
            };

            FilterContainer.X = -FilterContainer.Width - 50;
            FilterContainer.MoveToX(ScreenPaddingX, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        private void CreateMapsetContainer()
        {
            MapsetContainer = new DownloadableMapsetContainer(DownloadingScreen.Mapsets, DownloadingScreen.SelectedMapset,
                DownloadingScreen.Page, DownloadingScreen.ReachedEnd, DownloadingScreen.SearchTask)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = SearchPanel.Y + SearchPanel.Height + 8,
            };

            MapsetContainer.X = MapsetContainer.Width + 50;
            MapsetContainer.MoveToX(-ScreenPaddingX, Easing.OutQuint, 450);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScreenExiting(object sender, ScreenExitingEventArgs e)
        {
            MapsetContainer.ClearAnimations();
            MapsetContainer.MoveToX(MapsetContainer.Width + 50, Easing.OutQuint, 450);

            FilterContainer.ClearAnimations();
            FilterContainer.MoveToX(-FilterContainer.Width - 50, Easing.OutQuint, 450);
        }
    }
}