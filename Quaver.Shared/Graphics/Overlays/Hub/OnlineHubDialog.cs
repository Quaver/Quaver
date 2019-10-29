using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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
        public OnlineHubDialog() : base(0)
        {
            var game = (QuaverGame) GameBase.Game;
            Hub = game.OnlineHub;

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

            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Editor:
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Importing:
                    Height = WindowManager.Height;
                    Hub.Alignment = Alignment.MidRight;
                    break;
                default:
                    Height = Hub.Height;
                    Hub.Alignment = Alignment.TopRight;
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Hub.Parent = null;
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

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Hub.IsHovered())
                Close();
        }

        /// <summary>
        /// </summary>
        public void Open()
        {
            Hub.Open();
            FadeTo(0.75f, Easing.Linear, 200);
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            ClearAnimations();
            FadeTo(0, Easing.Linear, 200);

            Hub.Close();

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 300);
        }
    }
}