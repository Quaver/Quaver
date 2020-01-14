using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Options
{
    public sealed class OptionsDialog : DialogScreen
    {
        public OptionsDialog() : base(0)
        {
            FadeTo(0.75f, Easing.Linear, 200);
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            var quaver = (QuaverGame) GameBase.Game;
            quaver.OptionsMenu.Parent = this;
            quaver.OptionsMenu.Alignment = Alignment.MidCenter;
            quaver.OptionsMenu.Visible = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            var quaver = (QuaverGame) GameBase.Game;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape)
                || MouseManager.IsUniqueClick(MouseButton.Left) && !quaver.OptionsMenu.IsHovered())
            {
                Close();
            }
        }

        /// <summary>
        /// </summary>
        private void Close()
        {
            var quaver = (QuaverGame) GameBase.Game;

            if (quaver.OptionsMenu.IsKeybindFocused.Value)
                return;

            quaver.OptionsMenu.Visible = false;
            quaver.OptionsMenu.Parent = null;
            DialogManager.Dismiss(this);
            Destroy();
            ButtonManager.Remove(this);
        }
    }
}