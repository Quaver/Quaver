using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

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