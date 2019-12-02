using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs
{
    public sealed class CreateGameDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        public CreateGameDialog() : base(0)
        {
            CreateContent();
            FadeTo(0.85f, Easing.Linear, 150);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();
        }

        /// <summary>
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;

            ClearAnimations();
            FadeTo(0, Easing.Linear, 150);

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 175);
        }
    }
}