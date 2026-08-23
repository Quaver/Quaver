using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Input;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Logging;
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
        private QuaverCheckbox FreeModifierToggle { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus FreeModifierText { get; set; }

        /// <summary>
        /// </summary>
        private bool IsSyncingFreeModifierToggle { get; set; }

        /// <summary>
        /// </summary>
        private Bindable<GenericKey> BindedKey { get; }

        /// <summary>
        /// </summary>
        private GlobalKeybindActions Action { get; }

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
            ResetButton = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(20, 20),
                PerformHoverFade = false,
                Tint = ColorHelper.HexToColor("#ffffff"),
                Alpha = 0f
            };

            ResetButton.SetIcon(UserInterface.HubDownloadRetry, new Vector2(20, 20));
            ResetButton.SetChildrenAlpha = false;

            ResetButton.Clicked += (sender, args) =>
            {

                GlobalInputConfig.SetKeybindsForAction(Action, GlobalInputConfig.DefaultKeybindsFor(Action));
                GlobalInputConfig.SaveToConfig();
                InitializeText();
            };

            Button = new IconButton(UserInterface.DropdownClosed)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -(Name.X * 2 + ResetButton.Width),
                Size = new ScalableVector2(250, 35),
                Tint = ColorHelper.HexToColor("#181818"),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) => SetFocusedText();

            Button.ClickedOutside += (sender, args) =>
            {
                ClearFocusedState();
            };

            FreeModifierToggle = new QuaverCheckbox(new Bindable<bool>(false))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Button.X - Button.Width - 10,
                UsePreviousSpriteBatchOptions = true,
                DisposeBindableOnDestroy = true
            };

            FreeModifierToggle.BindedValue.ValueChanged += OnFreeModifierToggleChanged;

            FreeModifierText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "Mod", 18)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidRight,
                X = FreeModifierToggle.X - FreeModifierToggle.Width - 8,
                Tint = Colors.MainAccent
            };

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "", 18)
            {
                Parent = Button,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
                X = 16,
                Tint = Colors.MainAccent
            };

            InitializeText();
            InitializeFreeModifierToggle();

            GlobalInputConfig.OnConfigUpdated += GlobalInputConfigOnOnConfigUpdated;
        }

        private void GlobalInputConfigOnOnConfigUpdated()
        {
            InitializeText();
            InitializeFreeModifierToggle();
        }

        public RoundedButton ResetButton { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleKeySelect();
            ReleaseGlobalInputBlockWhenKeysClear();

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            ResetButton.Alpha = MathHelper.Lerp(ResetButton.Alpha, ResetButton.IsHovered ? 0.55f : 0f, (float)Math.Min(dt / 60, 1));

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void InitializeText()
        {
            Text.Text = GlobalInputConfig.GetOrDefault(Action).ToDisplayString();

            if (string.IsNullOrWhiteSpace(Text.Text))
                Text.Text = "None";
            // Obvious room for performance improvement, but really not needed
            Text.Tint = GlobalInputConfig.ConflictingActions.Contains(Action)
                ? Color.Crimson
                : Colors.MainAccent;
        }

        /// <summary>
        /// </summary>
        private void InitializeFreeModifierToggle()
        {
            IsSyncingFreeModifierToggle = true;
            FreeModifierToggle.BindedValue.Value = CurrentKeybind()?.Modifiers.Contains(KeyModifiers.Free) ?? false;
            IsSyncingFreeModifierToggle = false;
        }

        /// <summary>
        /// </summary>
        private Keybind? CurrentKeybind() => GlobalInputConfig.GetOrDefault(Action)
            .FirstOrDefault(x => !x.Equals(Keybind.None));

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnFreeModifierToggleChanged(object sender, BindableValueChangedEventArgs<bool> args)
        {
            if (IsSyncingFreeModifierToggle)
                return;

            var current = CurrentKeybind();

            if (current == null)
                return;

            SetKeybind(current.Key, args.Value, current.Modifiers);
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
        }

        /// <summary>
        /// </summary>
        private void ReleaseGlobalInputBlockWhenKeysClear()
        {
            if (Focused || BlockGlobalInputToken == null ||
                GenericKeyManager.GetPressedKeys().Count != 0)
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
            SetKeybind(keybind.Key, FreeModifierToggle.BindedValue.Value, keybind.Modifiers);

            ClearFocusedState(true);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="includeFreeModifier"></param>
        /// <param name="existingModifiers"></param>
        private void SetKeybind(GenericKey key, bool includeFreeModifier,
            IEnumerable<KeyModifiers>? existingModifiers = null)
        {
            var modifiers = existingModifiers?.ToHashSet() ?? new HashSet<KeyModifiers>();

            if (includeFreeModifier)
                modifiers.Add(KeyModifiers.Free);
            else
                modifiers.Remove(KeyModifiers.Free);

            GlobalInputConfig.SetKeybindsForAction(Action, new KeybindList(new Keybind(modifiers, key)));
            GlobalInputConfig.SaveToConfig();
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            BlockGlobalInputToken?.Dispose();
            BlockGlobalInputToken = null;
            FreeModifierToggle.BindedValue.ValueChanged -= OnFreeModifierToggleChanged;
            GlobalInputConfig.OnConfigUpdated -= GlobalInputConfigOnOnConfigUpdated;
            base.Destroy();
        }

        private class BlockGlobalInputScopeToken : GlobalInputScopeToken
        {
            /// <inheritdoc />
            public override GlobalInputScope Scope => GlobalInputScope.Options;

            /// <inheritdoc />
            public override GlobalInputHandleResult Handle(GlobalKeybindActions action,
                bool isKeyPress = true,
                bool isRelease = false) => GlobalInputHandleResult.Consumed;
        }
    }
}
