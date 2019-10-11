using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Main.UI.Jukebox;
using Quaver.Shared.Screens.Music.UI.Controller;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Tests.UI.Borders;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
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
        private MenuBorder MenuHeader { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MusicPlayerScreenView(Screen screen) : base(screen)
        {
            CreateBackground();

            MenuHeader = new MenuHeaderMain {Parent = Container};
            new FooterJukebox()
            {
                Parent = Container,
                X = -1000
            };

            var footer = new TestMenuBorderFooter()
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            var cont = new MusicControllerContainer()
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Y = MenuHeader.Height
            };

            var search = new MusicControllerSearchPanel(cont.Width)
            {
                Parent = Container,
                Alignment = cont.Alignment,
                Y = cont.Y + cont.Height
            };

            new MusicControllerSongContainer(new ScalableVector2(cont.Width,
                WindowManager.Height - footer.Height - search.Y - search.Height))
            {
                Parent = Container,
                Alignment = cont.Alignment,
                Y = search.Y + search.Height
            };
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
    }
}