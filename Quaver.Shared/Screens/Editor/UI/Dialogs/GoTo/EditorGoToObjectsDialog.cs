using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.GoTo
{
    public class EditorGoToObjectsDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        public EditorGoToObjectsPanel Panel { get; }

        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorGoToObjectsDialog(EditorGoToObjectsPanel panel) : base(0)
        {
            Panel = panel;

            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 100));

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Panel.Update(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Panel.Shown)
                Panel.Draw(gameTime);

            GameBase.Game.SpriteBatch.End();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
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
                Panel.Hide();
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0f, 200));
            ThreadScheduler.RunAfter(() =>
            {
                lock (DialogManager.Dialogs)
                    DialogManager.Dismiss(this);

                ButtonManager.Remove(this);
                IsClosing = false;
            }, 450);
        }
    }
}