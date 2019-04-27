using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.AutoMod
{
    public class EditorAutoModDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// </summary>
        private EditorAutoModViewer Viewer { get; }

        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        public EditorAutoModDialog(EditorAutoModViewer viewer) : base(0)
        {
            Viewer = viewer;
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 100));

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        public override void Update(GameTime gameTime)
        {
            Viewer.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (Viewer.Shown)
                Viewer.Draw(gameTime);

            GameBase.Game.SpriteBatch.End();
        }

        public override void Destroy() {}

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Viewer.Hide();
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

                var game = GameBase.Game as QuaverGame;
                var screen = game?.CurrentScreen as EditorScreen;
                var view = screen?.View as EditorScreenView;
                view.ControlBar.Parent = view.Container;
            }, 450);
        }

        public override void CreateContent()
        {
        }
    }
}