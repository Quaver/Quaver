using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Input;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemKeybindGeneric : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Bindable<GenericKey> BindedKey { get; }

        /// <summary>
        /// </summary>
        private GlobalKeybindActions? Action { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Text { get; set; }
        
        /// <summary>
        /// </summary>
        private GenericKeyState PreviousKeyState { get; set; } =
            new GenericKeyState(new List<GenericKey>());

        /// <summary>
        /// </summary>
        private GlobalInputScopeToken BlockGlobalInputToken { get; set; }

        /// <summary>
        /// </summary>
        private GlobalInputConfig GlobalInputConfig => ((QuaverGame) GameBase.Game).InputManager.InputConfig;

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="bindedKeys"></param>
        /// <param name="bindedKey"></param>
        /// <param name="isOptionFocused"></param>
        public OptionsItemKeybindGeneric(RectangleF containerRect, string name, Bindable<GenericKey> bindedKey) : base(containerRect, name)
        {
            BindedKey = bindedKey;
            CreateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public OptionsItemKeybindGeneric(RectangleF containerRect, string name, GlobalKeybindActions action) : base(containerRect, name)
        {
            Action = action;
            CreateContent();
        }

        /// <summary>
        /// </summary>
        private void CreateContent()
        {
            Button = new IconButton(UserInterface.DropdownClosed)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(250, 35),
                Tint = ColorHelper.HexToColor("#181818"),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) => SetFocusedText();

            Button.ClickedOutside += (sender, args) =>
            {
                ClearFocusedState();
            };

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), "", 22)
            {
                Parent = Button,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
                X = 16,
                Tint = Colors.MainAccent
            };

            InitializeText();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleKeySelect();
            ReleaseGlobalInputBlockWhenKeysClear();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void InitializeText()
        {
            Text.Text = Action.HasValue ? GlobalInputConfig.GetOrDefault(Action.Value).ToString() : BindedKey.Value.GetName();

            if (string.IsNullOrWhiteSpace(Text.Text))
                Text.Text = "None";
        }

        /// <summary>
        /// </summary>
        private void SetFocusedText()
        {
            Focused = true;
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            BlockGlobalInputToken ??= new BlockGlobalInputScopeToken();
            Text.Text = "Press a key...";
            Text.Tint = Color.Crimson;
        }

        /// <summary>
        /// </summary>
        private void ClearFocusedState(bool waitForInputClear = false)
        {
            Focused = false;

            if (!waitForInputClear)
            {
                BlockGlobalInputToken?.Dispose();
                BlockGlobalInputToken = null;
            }

            InitializeText();
            Text.Tint = Colors.MainAccent;
        }

        /// <summary>
        /// </summary>
        private void ReleaseGlobalInputBlockWhenKeysClear()
        {
            if (Focused || BlockGlobalInputToken == null || GenericKeyManager.GetPressedKeys().Count != 0)
                return;

            BlockGlobalInputToken.Dispose();
            BlockGlobalInputToken = null;
        }

        /// <summary>
        /// </summary>
        private void HandleKeySelect()
        {
            if (!Focused)
                return;

            var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var keys = currentKeyState.UniqueKeyPresses(PreviousKeyState);
            PreviousKeyState = currentKeyState;

            if (keys.Count == 0)
                return;

            var keybind = keys.First();
            if (Action.HasValue)
            {
                GlobalInputConfig.SetKeybindsForAction(Action.Value, new KeybindList(keybind));
                GlobalInputConfig.SaveToConfig();
            }
            else
            {
                BindedKey.Value = keybind.Key;
            }

            ClearFocusedState(true);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            BlockGlobalInputToken?.Dispose();
            BlockGlobalInputToken = null;
            base.Destroy();
        }

        private class BlockGlobalInputScopeToken : GlobalInputScopeToken
        {
            /// <inheritdoc />
            public override GlobalInputScope Scope => GlobalInputScope.Options;

            /// <inheritdoc />
            public override GlobalInputHandleResult Handle(GlobalKeybindActions action, bool isKeyPress = true,
                bool isRelease = false) => GlobalInputHandleResult.Consumed;
        }
    }
}
