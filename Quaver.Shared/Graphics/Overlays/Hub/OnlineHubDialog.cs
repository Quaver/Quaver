using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
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

        private Sprite GapFill { get; set; }

        /// <summary>
        /// </summary>
        public OnlineHubDialog() : base(0)
        {
            var game = (QuaverGame) GameBase.Game;
            Hub = game.OnlineHub;
            Chat = game.OnlineChat;

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

            if (game.CurrentScreen.Type != QuaverScreenType.Editor)
                game.GlobalUserInterface.Cursor.Alpha = 1;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Hub.Parent = null;
            Chat.Parent = null;

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

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Hub.IsHovered() && !Chat.IsHovered())
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
            ClearAnimations();
            FadeTo(0, Easing.Linear, 200);

            Hub.Close();
            Chat.Close();

            if (GameBase.Game is QuaverGame game && game.CurrentScreen.Type == QuaverScreenType.Gameplay)
                game.GlobalUserInterface.Cursor.Alpha = 0;

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 300);
        }
    }
}