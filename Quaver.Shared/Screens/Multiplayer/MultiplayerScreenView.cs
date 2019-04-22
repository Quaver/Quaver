using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Lobby;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Create;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Multiplayer
{
    public class MultiplayerScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        public MultiplayerScreen MultiplayerScreen { get; }

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuFooter Footer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MultiplayerScreenView(MultiplayerScreen screen) : base(screen)
        {
            MultiplayerScreen = screen;

            CreateBackground();
            CreateHeader();
            CreateFooter();

            OnlineManager.Client.OnGameHostChanged += OnGameHostchanged;
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
        public override void Destroy()
        {
            if (OnlineManager.Status.Value != ConnectionStatus.Disconnected)
                OnlineManager.Client.OnGameHostChanged -= OnGameHostchanged;

            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameHostchanged(object sender, GameHostChangedEventArgs e)
        {
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackgroundRaw, 30, true)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), "MULTIPLAYER", "GAME",
                "play a match together in real-time with others", ColorHelper.HexToColor("#f95186")) { Parent = Container };

            Header.Y = -Header.Height;
            Header.MoveToY(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            Footer = new MenuFooter(new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "leave game", 14, (o, e) => { }),
                new ButtonText(FontsBitmap.GothamRegular, "game chat", 14, (o, e) => { }),
            }, new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "ready up", 14, (o, e) => { }),
                new ButtonText(FontsBitmap.GothamRegular, "select map", 14, (o, e) => { }),
                new ButtonText(FontsBitmap.GothamRegular, "change modifiers", 14, (o, e) => DialogManager.Show(new CreateGameDialog())),
                new ButtonText(FontsBitmap.GothamRegular, "options menu", 14, (o, e) => DialogManager.Show(new SettingsDialog()))
            }, ColorHelper.HexToColor("#f95186"))
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            Footer.Y = Footer.Height;
            Footer.MoveToY(0, Easing.OutQuint, 600);
        }
    }
}