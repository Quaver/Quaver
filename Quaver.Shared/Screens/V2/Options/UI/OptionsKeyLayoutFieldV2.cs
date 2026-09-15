using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Input;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     A field that shows one key per lane, for example "A S K L". Clicking it waits for key presses
    ///     and the next different keys pressed, one per lane, become the new layout.
    /// </summary>
    internal sealed class OptionsKeyLayoutFieldV2 : RoundedButton, IViewportCullExempt
    {
        /// <summary>
        ///     Raised once every lane has a new key.
        /// </summary>
        internal event EventHandler LayoutChanged;

        private SkinV2OptionsRowConfig Config { get; }

        private SkinV2SliderConfig SliderConfig { get; }

        private IReadOnlyList<Bindable<GenericKey>> Keys { get; }

        private Color TextColor { get; }

        private Color AlertColor { get; }

        private bool Capturing { get; set; }

        private List<GenericKey> QueuedKeys { get; } = new List<GenericKey>();

        private GenericKeyState PreviousKeyState { get; set; } = new GenericKeyState(new List<GenericKey>());

        private OptionsCaptureInputBlock InputBlock { get; } = new OptionsCaptureInputBlock();

        /// <summary>
        ///     Keeps updating while hidden by scrolling, so a capture or input block is never left stuck.
        /// </summary>
        public bool IsCullExempt => Capturing || InputBlock.IsHeld;

        internal OptionsKeyLayoutFieldV2(WobbleFontStore font, SkinV2OptionsRowConfig config,
            SkinV2SliderConfig sliderConfig, IReadOnlyList<Bindable<GenericKey>> keys) : base(null)
        {
            Config = config;
            SliderConfig = sliderConfig;
            Keys = keys;
            TextColor = SkinV2Color.Parse(config.KeybindFieldTextColor);
            AlertColor = SkinV2Color.Parse(config.KeybindAlertColor);

            Size = new ScalableVector2(config.GameModeKeyLayoutFieldWidth, sliderConfig.ValueHeight);
            CornerRadius = sliderConfig.CornerRadius;
            Tint = SkinV2Color.Parse(sliderConfig.ValueColor);
            PerformHoverFade = true;
            SetChildrenAlpha = false;

            SetLabel(font, "", config.ControlFontSize, TextColor);
            Clicked += (sender, args) => BeginCapture();
            ClickedOutside += (sender, args) => EndCapture();

            RefreshField();
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
            InputBlock.Dispose();
            base.Destroy();
        }

        private void RefreshField()
        {
            SetLabelText(string.Join(" ", Keys.Select(x => x.Value.GetName())));
            Label.Tint = TextColor;
        }

        private void BeginCapture()
        {
            Capturing = true;
            QueuedKeys.Clear();
            PreviousKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            InputBlock.Acquire();
            SetLabelText(LocalizationManager.Get("Screen_Options_PressNKeys", Keys.Count));
            Label.Tint = AlertColor;
        }

        /// <summary>
        ///     Widens the field to fit long text like a 10K layout. It never gets narrower than the
        ///     normal width.
        /// </summary>
        private void SetLabelText(string text)
        {
            Label.Text = text;
            Width = Math.Max(Config.GameModeKeyLayoutFieldWidth, Label.Width + SliderConfig.ValueTextPadding * 2);
        }

        /// <summary>
        ///     Stops waiting for keys and shows the saved layout again.
        /// </summary>
        private void EndCapture()
        {
            if (!Capturing)
                return;

            Capturing = false;
            RefreshField();
        }

        /// <summary>
        ///     Collects newly pressed keys in order. Once there is one key per lane saves them all at once.
        ///     A key that was already collected is ignored.
        /// </summary>
        private void HandleKeyCapture()
        {
            if (!Capturing)
                return;

            var currentKeyState = new GenericKeyState(GenericKeyManager.GetPressedKeys());
            var newKeys = currentKeyState.Pressed.Except(PreviousKeyState.Pressed);
            PreviousKeyState = currentKeyState;

            foreach (var key in newKeys)
            {
                if (QueuedKeys.Contains(key))
                    continue;

                QueuedKeys.Add(key);
                SetLabelText(string.Join(" ", QueuedKeys.Select(x => x.GetName())));

                if (QueuedKeys.Count < Keys.Count)
                    continue;

                for (var i = 0; i < Keys.Count; i++)
                    Keys[i].Value = QueuedKeys[i];

                EndCapture();
                LayoutChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
    }
}
