using Microsoft.Xna.Framework;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Options
{
    public sealed class OptionsDialog : DialogScreen
    {
        private OptionsMenu Menu { get; set; }

        private GlobalInputScopeToken GlobalInputToken { get; }

        private sealed class Token(OptionsDialog dialog) : GlobalInputScopeToken
        {
            public override GlobalInputScope Scope => GlobalInputScope.Options;

            public override GlobalInputHandleResult Handle(GlobalKeybindActions action, bool isKeyPress = true,
                bool isRelease = false) => dialog.HandleGlobalInputAction(action, isKeyPress, isRelease);
        }

        public OptionsDialog() : base(0)
        {
            GlobalInputToken = new Token(this);
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
        }

        public override void Destroy()
        {
            GlobalInputToken.Dispose();
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            base.Destroy();
        }

        private GlobalInputHandleResult HandleGlobalInputAction(GlobalKeybindActions action,
            bool isKeyPress = true, bool isRelease = false)
        {
            if (!IsOnTop || !isKeyPress || isRelease || action.BaseWithLayer() != GlobalKeybindActions.Back)
                return GlobalInputHandleResult.Pass;

            Close();
            return GlobalInputHandleResult.Consumed;
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