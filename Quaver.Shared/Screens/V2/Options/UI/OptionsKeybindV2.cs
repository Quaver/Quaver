using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Input;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The control of a keybind row. From left to right: a "?" help badge, a "Mod" toggle and the
    ///     key field. The toggle sets whether the bind still works while Ctrl, Alt or Shift is held.
    ///     Clicking the key field waits for a key, and the next key pressed becomes the bind.
    /// </summary>
    internal sealed class OptionsKeybindV2 : Container, IViewportCullExempt
    {
        /// <summary>
        ///     Raised when this control changes the bind: a new key, or the "Mod" toggle.
        /// </summary>
        internal event EventHandler Rebound;

        private GlobalKeybindActions KeybindAction { get; }

        private SkinV2OptionsRowConfig Config { get; }

        private SkinV2SliderConfig SliderConfig { get; }

        private RoundedButton Field { get; }

        private V2Toggle FreeModifierToggle { get; }

        /// <summary>
        ///     The width of everything left of the key field.
        /// </summary>
        private float BaseWidth { get; }

        private Color FieldTextColor { get; }

        private Color AlertColor { get; }

        private bool Capturing { get; set; }

        private GenericKeyState PreviousKeyState { get; set; } = new GenericKeyState(new List<GenericKey>());

        private OptionsCaptureInputBlock InputBlock { get; } = new OptionsCaptureInputBlock();

        /// <summary>
        ///     Keeps updating while hidden by scrolling, so a capture or input block is never left stuck.
        /// </summary>
        public bool IsCullExempt => Capturing || InputBlock.IsHeld;

        private static GlobalInputConfig InputConfig => ((QuaverGame) GameBase.Game).InputManager.InputConfig;

        internal OptionsKeybindV2(WobbleFontStore font, SkinV2OptionsRowConfig config, SkinV2SharedConfig shared,
            GlobalKeybindActions action)
        {
            KeybindAction = action;
            Config = config;
            SliderConfig = shared.Slider;
            FieldTextColor = SkinV2Color.Parse(config.KeybindFieldTextColor);
            AlertColor = SkinV2Color.Parse(config.KeybindAlertColor);

            Height = config.RowHeight;

            var help = new RoundedPanel(config.KeybindHelpIconSize / 2f)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(config.KeybindHelpIconSize, config.KeybindHelpIconSize),
                Tint = SkinV2Color.Parse(config.KeybindHelpIconColor)
            };
            help.AddTooltip(new TooltipOptions(LocalizationManager.Get("Screen_Options_ModKeybindTooltip"))
            {
                MaximumWidth = 240
            });
            _ = new SpriteTextPlus(font, "?", config.KeybindHelpIconFontSize)
            {
                Parent = help,
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(config.KeybindHelpIconTextColor)
            };

            var modLabel = new SpriteTextPlus(font, LocalizationManager.Get("Screen_Options_Mod"), config.ControlFontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = config.KeybindHelpIconSize + config.ControlGap,
                Tint = SkinV2Color.Parse(config.ControlTextColor),
                UsePreviousSpriteBatchOptions = true
            };

            var toggleX = modLabel.X + modLabel.Width + config.ControlGap;
            FreeModifierToggle = new V2Toggle(font, shared.Toggle, HasFreeModifier())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = toggleX
            };
            FreeModifierToggle.ToggledChanged += OnFreeModifierToggled;

            BaseWidth = toggleX + shared.Toggle.Width + config.ControlGap;
            Size = new ScalableVector2(BaseWidth + config.KeybindFieldWidth, config.RowHeight);

            Field = new RoundedButton(null)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(config.KeybindFieldWidth, shared.Slider.ValueHeight),
                CornerRadius = shared.Slider.CornerRadius,
                Tint = SkinV2Color.Parse(shared.Slider.ValueColor),
                PerformHoverFade = true,
                SetChildrenAlpha = false
            };
            Field.SetLabel(font, "", config.ControlFontSize, FieldTextColor);
            Field.Clicked += (sender, args) => BeginCapture();
            Field.ClickedOutside += (sender, args) => EndCapture();

            RefreshField();
            InputConfig.OnConfigUpdated += OnInputConfigUpdated;
        }

        public override void Update(GameTime gameTime)
        {
            HandleKeyCapture();

            if (!Capturing)
                InputBlock.ReleaseWhenKeysAreUp();

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            InputConfig.OnConfigUpdated -= OnInputConfigUpdated;
            FreeModifierToggle.ToggledChanged -= OnFreeModifierToggled;
            InputBlock.Dispose();
            base.Destroy();
        }

        private void OnInputConfigUpdated()
        {
            RefreshField();
            FreeModifierToggle.SetOn(HasFreeModifier(), false);
        }

        private Keybind? CurrentKeybind() =>
            InputConfig.GetOrDefault(KeybindAction).FirstOrDefault(x => !x.Equals(Keybind.None));

        private bool HasFreeModifier() => CurrentKeybind()?.Modifiers.Contains(KeyModifiers.Free) ?? false;

        /// <summary>
        ///     Shows the saved bind. It is shown in red when another action uses the same bind.
        /// </summary>
        private void RefreshField()
        {
            var display = InputConfig.GetOrDefault(KeybindAction).ToDisplayString();

            SetFieldText(string.IsNullOrWhiteSpace(display) ? LocalizationManager.Get("Screen_Options_None") : display,
                InputConfig.ConflictingActions.Contains(KeybindAction) ? AlertColor : FieldTextColor);
        }

        /// <summary>
        ///     Widens the field (and this control) to fit long text. It never gets narrower than the
        ///     normal width.
        /// </summary>
        private void SetFieldText(string text, Color tint)
        {
            Field.Label.Text = text;
            Field.Label.Tint = tint;

            var fieldWidth = Math.Max(Config.KeybindFieldWidth, Field.Label.Width + SliderConfig.ValueTextPadding * 2);
            Field.Width = fieldWidth;
            Width = BaseWidth + fieldWidth;
        }

        private void OnFreeModifierToggled(object? sender, bool isOn)
        {
            var current = CurrentKeybind();

            // Nothing to change until a key is bound.
            if (current == null)
                return;

            SetKeybind(current.Key, isOn, current.Modifiers);
        }

        private void BeginCapture()
        {
            Capturing = true;
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            InputBlock.Acquire();
            SetFieldText(LocalizationManager.Get("Screen_Options_PressAKey"), AlertColor);
        }

        /// <summary>
        ///     Stops waiting for a key and shows the saved bind again.
        /// </summary>
        private void EndCapture()
        {
            if (!Capturing)
                return;

            Capturing = false;
            RefreshField();
        }

        private void HandleKeyCapture()
        {
            if (!Capturing)
                return;

            var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var keys = currentKeyState.UniqueKeyPresses(PreviousKeyState);
            PreviousKeyState = currentKeyState;

            if (keys.Count == 0)
                return;

            var keybind = keys.First();
            SetKeybind(keybind.Key, FreeModifierToggle.IsOn, keybind.Modifiers);
            EndCapture();
        }

        private void SetKeybind(GenericKey key, bool includeFreeModifier, IEnumerable<KeyModifiers> existingModifiers)
        {
            var modifiers = existingModifiers.ToHashSet();

            if (includeFreeModifier)
                modifiers.Add(KeyModifiers.Free);
            else
                modifiers.Remove(KeyModifiers.Free);

            InputConfig.SetKeybindsForAction(KeybindAction, new KeybindList(new Keybind(modifiers, key)));
            InputConfig.SaveToConfig();
            Rebound?.Invoke(this, EventArgs.Empty);
        }
    }
}
