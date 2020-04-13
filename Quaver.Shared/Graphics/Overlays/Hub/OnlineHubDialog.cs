using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.Hub
{
    public class OnlineHubDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private OnlineHub Hub { get; }

        /// <summary>
        /// </summary>
        private OnlineChat Chat { get; }

        /// <summary>
        /// </summary>
        private Sprite GapFill { get; set; }

        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// </summary>
        public OnlineHubDialog() : base(0)
        {
            var game = (QuaverGame) GameBase.Game;
            Hub = game.OnlineHub;
            Chat = game.OnlineChat;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameStarted += OnMultiplayerGameStarted;

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var game = (QuaverGame) GameBase.Game;

            Hub.Alignment = Alignment.TopRight;

            Alignment = Alignment.TopLeft;

            Width = WindowManager.Width;

            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Editor:
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Importing:
                    Height = WindowManager.Height;
                    Y = 0;
                    GapFill.Visible = true;
                    break;
                default:
                    Height = WindowManager.Height - MenuBorder.HEIGHT;
                    Y = MenuBorder.HEIGHT;
                    Hub.Y = 0;
                    GapFill.Visible = false;
                    break;
            }

            if (!IsClosing && game.CurrentScreen.Type != QuaverScreenType.Editor)
                game.GlobalUserInterface.Cursor.Alpha = 1;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Hub.Parent = null;
            Chat.Parent = null;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnGameStarted -= OnMultiplayerGameStarted;

            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            Height = Hub.Height;
            Alignment = Alignment.MidLeft;

            Hub.Parent = this;
            Hub.Alignment = Alignment.TopRight;

            Hub.ClearAnimations();
            Hub.X = Hub.Width + 10;

            Chat.Parent = this;
            Chat.Alignment = Alignment.BotLeft;
            Chat.ClearAnimations();
            Chat.Y = Chat.Height + 10;

            // TODO: The hub should automatically change it's container size rather than doing this
            GapFill = new Sprite()
            {
                Parent = Hub,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Hub.Width, WindowManager.Height - Hub.Height),
                Tint = ColorHelper.HexToColor("#242424")
            };

            GapFill.Y = GapFill.Height;

            Open();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Hub.IsHovered() && !Chat.IsHovered() && !Chat.IsResizing)
                Close();
        }

        /// <summary>
        /// </summary>
        public void Open()
        {
            Hub.Open();
            Chat.Open();
            FadeTo(0.75f, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            IsClosing = true;

            ClearAnimations();
            FadeTo(0, Easing.Linear, 200);

            Hub.Close();
            Chat.Close();

            if (GameBase.Game is QuaverGame game && game.CurrentScreen.Type == QuaverScreenType.Gameplay)
            {
                if (game.CurrentScreen is GameplayScreen gameplay)
                    game.GlobalUserInterface.Cursor.Alpha = gameplay.InReplayMode && gameplay.SpectatorClient == null ? 1 : 0;
            }

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 300);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMultiplayerGameStarted(object sender, GameStartedEventArgs e) => Close();
    }
}