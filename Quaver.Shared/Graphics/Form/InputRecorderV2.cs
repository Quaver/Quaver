using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Input;
using Quaver.Shared.Input.Global;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class InputRecorderV2 : Container
    {
        public event EventHandler ValueEdited;

        // Global
        private GlobalInputConfig GlobalInputConfig => ((QuaverGame) GameBase.Game).InputManager.InputConfig;
        private List<Bindable<GenericKey>> BindedKeys { get; } = new List<Bindable<GenericKey>>();
        private List<GenericKey> QueuedKeys { get; } = new List<GenericKey>();
        private GenericKeyState PreviousKeyState { get; set; } = new GenericKeyState(Array.Empty<GenericKey>());
        public bool Focused { get; private set; }

        // Layouts
        private RoundedButton RecorderButton { get; set; }
        private FlexContainer Layout { get; set; }
        private FlexItemOptions RecorderLayoutOptions { get; set; }

        // GlobalKeybindActions specific
        private GlobalKeybindActions? Action { get; }
        private Bindable<bool> FreeModEnabled { get; set; }
        private bool IsSyncingFreeModToggle { get; set; }
        private FlexContainer ModLayout { get; set; }

        /// <summary>
        /// Input recorded for options using more than one keybinds
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="defaults"></param>
        public InputRecorderV2(List<Bindable<GenericKey>> keys) : this(ValidateKeyCount(keys))
        {
            BindedKeys = keys;

            ShowBoundKeys();
        }

        /// <summary>
        /// Input recorder for GlobalKeybindActions (basically shortcut keybinds)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="showMod"></param>
        public InputRecorderV2(GlobalKeybindActions action, bool showMod = true) : this(1)
        {
            Action = action;

            if (showMod)
                CreateModToggle();

            GlobalInputConfig.OnConfigUpdated += OnGlobalConfigUpdated;
            OnGlobalConfigUpdated();
        }

        private InputRecorderV2(int keyCount)
        {
            CreateLayout(keyCount);
        }

        private static int ValidateKeyCount(List<Bindable<GenericKey>> keys)
        {
            if (keys == null || keys.Count == 0 || keys.Any(key => key == null))
                throw new ArgumentException("The recorder needs at least one bound key.", nameof(keys));

            return keys.Count;
        }

        private void CreateLayout(int keyCount)
        {
            var width = 10 * keyCount;
            Size = new ScalableVector2(width + 10, 30);

            Layout = new FlexContainer
            {
                Parent = this,
                Size = Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = 10
            };

            RecorderButton = new RoundedButton
            {
                Parent = Layout,
                Size = new ScalableVector2(100, 30),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#273038")
            };
            RecorderButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), string.Empty, 18, ColorHelper.HexToColor("#8CAFEA"));
            RecorderLayoutOptions = new FlexItemOptions { Basis = width, Shrink = 0 };
            Layout.SetItemOptions(RecorderButton, RecorderLayoutOptions);

            RecorderButton.Clicked += OnRecorderClicked;
            RecorderButton.ClickedOutside += OnRecorderClickedOutside;
        }

        private void CreateModToggle()
        {
            FreeModEnabled = new Bindable<bool>(false);
            FreeModEnabled.ValueChanged += OnModChanged;

            ModLayout = new FlexContainer
            {
                Parent = Layout,
                Size = new ScalableVector2(1, 30),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = 8
            };

            var modLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Editor_Free"), 18)
            {
                Parent = ModLayout,
                Tint = ColorHelper.HexToColor("#8CAFEA")
            };
            ModLayout.SetItemOptions(modLabel, new FlexItemOptions { Basis = modLabel.Width, Shrink = 0 });

            var modToggle = new ToggleV2(FreeModEnabled) { Parent = ModLayout };
            ModLayout.SetItemOptions(modToggle, new FlexItemOptions { Basis = modToggle.Width, Shrink = 0 });

            ModLayout.Width = modLabel.Width + ModLayout.ColumnGap + modToggle.Width;
            Layout.SetItemOptions(ModLayout, new FlexItemOptions { Basis = ModLayout.Width, Shrink = 0, Order = -1 });

            ModLayout.RefreshLayout();
        }

        public override void Update(GameTime gameTime)
        {
            HandleKeySelect();

            ReleaseGlobalInputBlock();

            base.Update(gameTime);
        }

        private void ClearFocusedState(bool waitForInputClear = false)
        {
            Focused = false;
            QueuedKeys.Clear();

            if (!waitForInputClear)
            {
                BlockGlobalInputToken?.Dispose();
                BlockGlobalInputToken = null;
            }

            ShowBoundKeys();
        }

        private void ReleaseGlobalInputBlock()
        {
            if (Focused || BlockGlobalInputToken == null || GenericKeyManager.GetPressedKeys().Count != 0)
                return;

            BlockGlobalInputToken.Dispose();
            BlockGlobalInputToken = null;
        }

        private void HandleKeySelect()
        {
            if (!Focused)
                return;

            if (Action.HasValue)
            {
                HandleGlobalKeySelect();
                return;
            }

            var keys = GenericKeyManager.GetPressedKeys();

            foreach (var key in keys)
            {
                if (QueuedKeys.Contains(key))
                    continue;

                QueuedKeys.Add(key);
                ShowText(string.Join(" ", QueuedKeys.Select(queuedKey => queuedKey.GetName())), Color.Crimson);

                if (QueuedKeys.Count != BindedKeys.Count)
                    continue;

                var changed = false;
                for (var i = 0; i < QueuedKeys.Count; i++)
                {
                    changed |= !Equals(BindedKeys[i].Value, QueuedKeys[i]);
                    BindedKeys[i].Value = QueuedKeys[i];
                }

                Focused = false;
                ShowBoundKeys();
                if (changed)
                    ValueEdited?.Invoke(this, EventArgs.Empty);
                break;
            }
        }
        private void HandleGlobalKeySelect()
        {
            var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var keys = currentKeyState.UniqueKeyPresses(PreviousKeyState);
            PreviousKeyState = currentKeyState;

            if (keys.Count == 0)
                return;

            var keybind = keys.First();
            var includeFree = FreeModEnabled?.Value ?? CurrentKeybind()?.Modifiers.Contains(KeyModifiers.Free) ?? false;
            SaveGlobalKeybind(keybind.Key, includeFree, keybind.Modifiers);

            ClearFocusedState(true);
        }

        private Keybind CurrentKeybind()
        {
            if (!Action.HasValue)
                return null;

            return GlobalInputConfig.GetOrDefault(Action.Value).FirstOrDefault(keybind => !keybind.Equals(Keybind.None));
        }

        private void ShowBoundKeys()
        {
            if (Action.HasValue)
            {
                var text = GlobalInputConfig.GetOrDefault(Action.Value).ToDisplayString();
                if (string.IsNullOrWhiteSpace(text))
                    text = LocalizationManager.Get("Screen_Editor_None");

                var color = GlobalInputConfig.ConflictingActions.Contains(Action.Value) ? Color.Crimson : ColorHelper.HexToColor("#8CAFEA");
                ShowText(text, color);
            }
            else
                ShowText(string.Join(" ", BindedKeys.Select(key => key.Value.GetName())), ColorHelper.HexToColor("#8CAFEA"));
        }

        private void ShowText(string text, Color color)
        {
            RecorderButton.Label.Text = text;
            RecorderButton.Label.Tint = color;

            var buttonWidth = RecorderButton.Label.Width + 32;
            RecorderButton.Width = buttonWidth;
            RecorderLayoutOptions.Basis = buttonWidth;

            Width = buttonWidth + (ModLayout == null ? 0 : Layout.ColumnGap + ModLayout.Width);
            Layout.Width = Width;
            Layout.RefreshLayout();
        }

        private void OnRecorderClicked(object sender, EventArgs e)
        {
            QueuedKeys.Clear();
            Focused = true;

            if (Action.HasValue)
            {
                PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
                BlockGlobalInputToken ??= new BlockGlobalInputScopeToken();
                ShowText(LocalizationManager.Get("Screen_Editor_EnterNewKeybind"), Color.Crimson);
            }
            else
                ShowText(LocalizationManager.Get("Screen_Options_PressKeys", BindedKeys.Count), Color.Crimson);
        }
        private void OnRecorderClickedOutside(object sender, EventArgs e)
        {
            if (!Focused)
                return;

            ClearFocusedState();
        }
        private void OnModChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (IsSyncingFreeModToggle)
                return;

            var current = CurrentKeybind();
            if (current != null)
                SaveGlobalKeybind(current.Key, e.Value, current.Modifiers);
        }
        private void OnGlobalConfigUpdated()
        {
            ShowBoundKeys();

            if (FreeModEnabled == null)
                return;

            IsSyncingFreeModToggle = true;
            FreeModEnabled.Value = CurrentKeybind()?.Modifiers.Contains(KeyModifiers.Free) ?? false;
            IsSyncingFreeModToggle = false;
        }

        private void SaveGlobalKeybind(GenericKey key, bool includeFree, IEnumerable<KeyModifiers> existingModifiers = null)
        {
            var modifiers = existingModifiers?.ToHashSet() ?? new HashSet<KeyModifiers>();

            if (includeFree)
                modifiers.Add(KeyModifiers.Free);
            else
                modifiers.Remove(KeyModifiers.Free);

            var keybinds = new KeybindList(new Keybind(modifiers, key));
            var changed = !GlobalInputConfig.GetOrDefault(Action.Value).SetEquals(keybinds);
            GlobalInputConfig.SetKeybindsForAction(Action.Value, keybinds);
            GlobalInputConfig.SaveToConfig();
            if (changed)
                ValueEdited?.Invoke(this, EventArgs.Empty);
        }

        public override void Destroy()
        {
            BlockGlobalInputToken?.Dispose();

            RecorderButton.Clicked -= OnRecorderClicked;
            RecorderButton.ClickedOutside -= OnRecorderClickedOutside;

            if (Action.HasValue)
                GlobalInputConfig.OnConfigUpdated -= OnGlobalConfigUpdated;

            if (FreeModEnabled != null)
                FreeModEnabled.ValueChanged -= OnModChanged;

            base.Destroy();
            FreeModEnabled?.Dispose();
        }

        private GlobalInputScopeToken BlockGlobalInputToken { get; set; }
        private class BlockGlobalInputScopeToken : GlobalInputScopeToken
        {
            public override GlobalInputScope Scope => GlobalInputScope.Options;

            public override GlobalInputHandleResult Handle(GlobalKeybindActions action, bool isKeyPress = true, bool isRelease = false) => GlobalInputHandleResult.Consumed;
        }
    }
}
