using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.Options
{
    public sealed class OptionsDialog : DialogScreen
    {
        private OptionsMenu Menu { get; set; }

        public OptionsDialog() : base(0)
        {
            FadeTo(0.75f, Easing.Linear, 200);
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!Menu.IsHovered())
                    Close();
            };

            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            var quaver = (QuaverGame)GameBase.Game;

            Menu = new OptionsMenu()
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
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

        public override void Destroy()
        {
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void Close()
        {
            if (Menu.IsOptionFocused.Value)
                return;

            Menu.Destroy();
            DialogManager.Dismiss(this);
            Destroy();
            ButtonManager.Remove(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs e)
        {
            Menu.Destroy();
            CreateContent();
        }
    }
}