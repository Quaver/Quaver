using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Lobby.UI.Dialogs.Create
{
    public class CreateGameDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private CreateGameInterface CreateGameInterface { get; set; }

        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CreateGameDialog() : base(0.85f) => CreateContent();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!OnlineManager.Connected)
                Close();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateGameInterface = new CreateGameInterface(this)
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            CreateGameInterface.X = -CreateGameInterface.Width;
            CreateGameInterface.MoveToX(0, Easing.OutQuint, 600);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) && !IsClosing)
            {
                Close();
                IsClosing = true;
            }
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;

            CreateGameInterface.Animations.Clear();
            CreateGameInterface.MoveToX(-CreateGameInterface.Width - 2, Easing.OutQuint, 400);
            Alpha = 0f;
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }
    }
}