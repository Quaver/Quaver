using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private EditorScrollVelocityChanger ScrollVelocityChanger { get; set; }

        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorScrollVelocityDialog() : base(0)
        {
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 100));

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!GraphicsHelper.RectangleContains(ScrollVelocityChanger.ScreenRectangle,
                        MouseManager.CurrentState.Position) && !IsClosing)
                {
                    Close();
                    IsClosing = true;
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent() => CreateScrollVelocityChanger();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }

        /// <summary>
        /// </summary>
        private void CreateScrollVelocityChanger()
        {
            ScrollVelocityChanger = new EditorScrollVelocityChanger(this)
            {
                Parent = Container,
                Alignment = Wobble.Graphics.Alignment.MidLeft
            };

            ScrollVelocityChanger.X = -ScrollVelocityChanger.Width;
            ScrollVelocityChanger.MoveToX(0, Easing.OutQuint, 600);
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            ScrollVelocityChanger.Animations.Clear();
            ScrollVelocityChanger.MoveToX(-ScrollVelocityChanger.Width - 2, Easing.OutQuint, 400);

            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0f, 200));
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }
    }
}