using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Quaver.Shared.Screens.Music.Components;
using Quaver.Shared.Screens.Music.UI.Controller;
using Quaver.Shared.Screens.Music.UI.Controller.Search;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music
{
    public class MusicPlayerScreenView : ScreenView
    {
        /// <summary>
        ///     The main menu background.
        /// </summary>
        public BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuBorder Footer { get; set; }

        /// <summary>
        /// </summary>
        private Container ContentContainer { get; }

        /// <summary>
        /// </summary>
        private MusicControllerContainer ControllerContainer { get; set; }

        /// <summary>
        /// </summary>
        private MusicControllerSearchPanel SearchPanel { get; set; }

        /// <summary>
        /// </summary>
        private MusicControllerSongContainer SongContainer { get; set; }

        /// <summary>
        /// </summary>
        private MusicPlayerJukebox Jukebox { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MusicPlayerScreenView(Screen screen) : base(screen)
        {
            Jukebox = new MusicPlayerJukebox {Parent = Container};

            CreateBackground();
            CreateMenuHeader();
            CreateMenuFooter();

            ContentContainer = new Container {Parent = Container};
            CreateMusicControllerContainer();
            CreateSearchPanel();
            CreateSongContainer();

            SearchPanel.Parent = ContentContainer;
            Header.Parent = Container;
            Footer.Parent = Container;

            AnimateContentContainer();
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
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#181818"));
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
        /// </summary>
        private void CreateMenuHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateMenuFooter() => Footer = new TestMenuBorderFooter
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateMusicControllerContainer() => ControllerContainer = new MusicControllerContainer(Jukebox)
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopRight,
            Y = Header.Height
        };

        /// <summary>
        /// </summary>
        private void CreateSearchPanel() => SearchPanel = new MusicControllerSearchPanel(ControllerContainer.Width)
        {
            Parent = ContentContainer,
            Alignment = ControllerContainer.Alignment,
            Y = ControllerContainer.Y + ControllerContainer.Height
        };

        /// <summary>
        /// </summary>
        private void CreateSongContainer() => SongContainer = new MusicControllerSongContainer(new ScalableVector2(ControllerContainer.Width,
            WindowManager.Height - Footer.Height - SearchPanel.Y - SearchPanel.Height))
        {
            Parent = ContentContainer,
            Alignment = ControllerContainer.Alignment,
            Y = SearchPanel.Y + SearchPanel.Height
        };

        /// <summary>
        /// </summary>
        private void AnimateContentContainer()
        {
            ContentContainer.X = ContentContainer.Width;
            ContentContainer.MoveToX(0, Easing.OutQuint, 600);

            var screen = (MusicPlayerScreen) Screen;

            screen.ScreenExiting += (sender, args) =>
            {
                ContentContainer.ClearAnimations();
                ContentContainer.MoveToX(ContentContainer.Width, Easing.OutQuint, 800);
            };
        }
    }
}