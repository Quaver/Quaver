using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Theater.UI.Footer;
using Quaver.Shared.Screens.Theater.UI.Players;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Theater
{
    public class TheaterScreenView : ScreenView
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
        private SpriteTextPlus ExperimentalNotice { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus DragReplaysText { get; set; }

        /// <summary>
        /// </summary>
        private List<DrawableReplayPlayer> Players { get; } = new List<DrawableReplayPlayer>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public TheaterScreenView(TheaterScreen screen) : base(screen)
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateExperimentalNotice();
            CreateDragReplaysText();

            screen.ReplayLoaded += OnReplayLoaded;
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
        public override void Destroy()
        {
            var screen = (TheaterScreen) Screen;
            screen.ReplayLoaded -= OnReplayLoaded;

            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.Triangles, 0, false)
            {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new MenuHeaderMain {Parent = Container};

        /// <summary>
        /// </summary>
        private void CreateFooter() => Footer = new TheaterFooter(Screen as TheaterScreen)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateExperimentalNotice()
        {
            ExperimentalNotice = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "This feature is experimental and not complete. Some things may not work properly.", 28)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = Header.Height + 50
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDragReplaysText()
        {
            DragReplaysText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "To begin, drag up to 4 replays into the window...", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = ExperimentalNotice.Y + ExperimentalNotice.Height + 20
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReplayLoaded(object sender, ReplayLoadedEventArgs e)
        {
            var player = new DrawableReplayPlayer(e.Replay)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft
            };

            player.X = (Players.Count) * (player.Width + 20) + 50;
            Players.Add(player);
        }
    }
}