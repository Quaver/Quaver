using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared V2 slider: a bar you click or drag, with a value box on its right.
    ///     The whole bar can be dragged, and a fill grows from its left edge to show the value.
    ///     Clicking the value box lets you type a number between the minimum and maximum.
    ///     While the mouse is on the slider, the left and right arrow keys change the value.
    /// </summary>
    public sealed class V2Slider : Container, IViewportCullExempt
    {
        /// <summary>
        ///     Finds the number in the shown text ("80%", "-5 ms"), so editing starts from what the box shows.
        /// </summary>
        private static readonly Regex DisplayedNumber = new Regex(@"-?\d+(\.\d+)?", RegexOptions.Compiled);

        /// <summary>
        ///     The value box being typed into, if any. Only one can be edited at a time.
        /// </summary>
        private static ValueTextbox ActiveEditor { get; set; }

        public static bool IsEditingValue => ActiveEditor != null;

        public static void CancelValueEdit() => ActiveEditor?.CancelEditing();

        /// <summary>
        ///     Set while another control uses the arrow keys (like a search box), so sliders ignore them.
        /// </summary>
        public static bool SuspendKeyboardAdjustment { get; set; }

        /// <summary>
        ///     How long an arrow key is held before the value starts repeating.
        /// </summary>
        private const double AdjustmentRepeatDelay = 400;

        /// <summary>
        ///     How often the value changes while an arrow key is held.
        /// </summary>
        private const double AdjustmentRepeatInterval = 40;

        private SkinV2SliderConfig Config { get; }

        /// <summary>
        ///     The height of the row the slider is in, so it centers in the row.
        /// </summary>
        private float RowHeight { get; }

        private float MinValue { get; }

        private float MaxValue { get; }

        private Func<float, string> Formatter { get; }

        private SliderBar Bar { get; }

        private RoundedPanel Fill { get; }

        private RoundedPanel ValueBackground { get; }

        private SpriteTextPlus ValueText { get; }

        private ValueTextbox ValueEditor { get; }

        /// <summary>
        ///     Keeps updating while being typed into, even when scrolled out of view, so the edit can finish.
        /// </summary>
        public bool IsCullExempt => ReferenceEquals(ActiveEditor, ValueEditor);

        /// <summary>
        ///     The direction of the held arrow key: -1 for left, 1 for right, 0 for none.
        /// </summary>
        private int HeldDirection { get; set; }

        private double TimeArrowHeld { get; set; }

        private double TimeSinceRepeat { get; set; }

        /// <summary>
        ///     Whether the bar was held last frame, to notice when a drag ends.
        /// </summary>
        private bool WasBarHeld { get; set; }

        /// <summary>
        ///     Whether the mouse is on the bar or the value box.
        /// </summary>
        private bool IsPointedAt => Bar.IsHovered || ValueEditor.IsBadgeHovered;

        public float Value { get; private set; }

        public event EventHandler<float> ValueChanged;

        public V2Slider(WobbleFontStore font, SkinV2SliderConfig config, float rowHeight, float minValue,
            float maxValue, float initialValue, Func<float, string> formatter)
        {
            Config = config;
            RowHeight = rowHeight;
            MinValue = minValue;
            MaxValue = Math.Max(maxValue, minValue + 0.0001f);
            Formatter = formatter;
            Value = MathHelper.Clamp(initialValue, MinValue, MaxValue);

            Size = new ScalableVector2(config.TrackWidth + config.ValueGap + config.ValueWidth, rowHeight);

            Bar = new SliderBar(normalized => SetValue(MinValue + normalized * (MaxValue - MinValue), true))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(config.TrackWidth, config.TrackHeight),
                CornerRadius = config.CornerRadius,
                Tint = SkinV2Color.Parse(config.BackgroundColor),
                PerformHoverFade = true
            };

            Fill = new RoundedPanel(config.CornerRadius)
            {
                Parent = Bar,
                Alignment = Alignment.MidLeft,
                Height = config.TrackHeight,
                Tint = SkinV2Color.Parse(config.BarColor)
            };

            ValueBackground = new RoundedPanel(config.CornerRadius)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(config.ValueWidth, config.ValueHeight),
                Tint = SkinV2Color.Parse(config.ValueColor)
            };

            ValueText = new SpriteTextPlus(font, Formatter(Value), config.FontSize)
            {
                Parent = ValueBackground,
                Alignment = Alignment.MidRight,
                X = -config.ValueTextPadding,
                Tint = SkinV2Color.Parse(config.ValueTextColor),
                UsePreviousSpriteBatchOptions = true
            };

            ValueEditor = new ValueTextbox(this, font, config)
            {
                Parent = ValueBackground,
                Alignment = Alignment.MidCenter
            };

            RefreshVisualPosition();
            RefreshValueWidth();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            HandleKeyboardAdjustment(gameTime);

            if (WasBarHeld && !Bar.IsHeld)
                RefreshValueWidth();

            WasBarHeld = Bar.IsHeld;
        }

        public void SetValue(float value, bool invokeEvent)
        {
            value = MathHelper.Clamp(value, MinValue, MaxValue);

            if (Math.Abs(Value - value) < 0.0001f)
                return;

            Value = value;
            RefreshVisualPosition();
            ValueText.Text = Formatter(Value);

            if (!Bar.IsHeld)
                RefreshValueWidth();

            if (invokeEvent)
                ValueChanged?.Invoke(this, Value);
        }

        /// <summary>
        ///     Changes the value with the left and right arrow keys while the mouse is on the slider.
        ///     Holding a key repeats the change.
        /// </summary>
        private void HandleKeyboardAdjustment(GameTime gameTime)
        {
            if (!IsPointedAt || IsEditingValue || SuspendKeyboardAdjustment)
            {
                HeldDirection = 0;
                return;
            }

            var left = KeyboardManager.CurrentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left);
            var right = KeyboardManager.CurrentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right);

            var direction = left == right ? 0 : right ? 1 : -1;

            if (direction == 0)
            {
                HeldDirection = 0;
                return;
            }

            if (direction != HeldDirection)
            {
                HeldDirection = direction;
                TimeArrowHeld = 0;
                TimeSinceRepeat = 0;
                Step(direction);
                return;
            }

            TimeArrowHeld += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeArrowHeld < AdjustmentRepeatDelay)
                return;

            TimeSinceRepeat += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (TimeSinceRepeat < AdjustmentRepeatInterval)
                return;

            TimeSinceRepeat = 0;
            Step(direction);
        }

        /// <summary>
        ///     Moves the value by one, starting from the rounded value. A value of 63.4 goes up to 64.
        /// </summary>
        private void Step(int direction) => SetValue((float) Math.Round(Value) + direction, true);

        /// <summary>
        ///     Widens the value box (and the text box on it) to fit the text. It never gets narrower than
        ///     the normal width, and nothing changes when the width is the same.
        /// </summary>
        private void RefreshValueWidth()
        {
            var badgeWidth = Math.Max(Config.ValueWidth, ValueText.Width + Config.ValueTextPadding * 2);

            if (Math.Abs(ValueBackground.Width - badgeWidth) < 0.001f)
                return;

            var badgeSize = new ScalableVector2(badgeWidth, Config.ValueHeight);
            ValueBackground.Size = badgeSize;
            ValueEditor.Size = badgeSize;

            ValueEditor.Button.Size = badgeSize;

            Size = new ScalableVector2(Config.TrackWidth + Config.ValueGap + badgeWidth, RowHeight);
        }

        private void RefreshVisualPosition()
        {
            var normalized = MathHelper.Clamp((Value - MinValue) / (MaxValue - MinValue), 0, 1);
            Fill.Width = normalized * Config.TrackWidth;
        }

        /// <summary>
        ///     The text an edit starts with: the number shown in the value box, without units.
        /// </summary>
        private string GetEditableText()
        {
            var match = DisplayedNumber.Match(ValueText.Text ?? string.Empty);

            return match.Success ? match.Value : Value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Applies a typed value, limited to the slider's range. Text that is not a number is ignored.
        /// </summary>
        private void CommitEditedText(string text)
        {
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                SetValue(parsed, true);
        }

        /// <summary>
        ///     The bar. Clicking or dragging anywhere on it sets the value.
        /// </summary>
        private sealed class SliderBar : RoundedButton
        {
            private Action<float> Dragged { get; }

            internal SliderBar(Action<float> dragged) : base(null) => Dragged = dragged;

            protected override void OnHeld(GameTime gameTime)
            {
                base.OnHeld(gameTime);
                var localX = MouseManager.CurrentState.X - ScreenRectangle.X;
                Dragged(MathHelper.Clamp(localX / Math.Max(1, Width), 0, 1));
            }
        }

        /// <summary>
        ///     The text box on top of the value box. It is invisible and empty until the value box is
        ///     clicked. It only accepts numbers within the slider's range.
        /// </summary>
        private sealed class ValueTextbox : Textbox
        {
            private V2Slider Slider { get; }

            /// <summary>
            ///     Whether the value is being typed, so this box's text is shown instead of the value text.
            /// </summary>
            private bool Editing { get; set; }

            internal bool IsBadgeHovered => Button.IsHovered;

            internal ValueTextbox(V2Slider slider, WobbleFontStore font, SkinV2SliderConfig config)
                : base(new ScalableVector2(config.ValueWidth, config.ValueHeight), font, config.FontSize)
            {
                Slider = slider;

                SpriteBatchOptions = null;
                UsePreviousSpriteBatchOptions = true;

                Image = null;
                Alpha = 0;
                InputEnabled = false;
                Scrollbar.Visible = false;

                MaxCharacters = Math.Max(((int) slider.MinValue).ToString(CultureInfo.InvariantCulture).Length,
                    ((int) slider.MaxValue).ToString(CultureInfo.InvariantCulture).Length) + 3;
                AllowedCharacters = slider.MinValue < 0 ? new Regex(@"^-?\d*\.?\d*$") : new Regex(@"^\d*\.?\d*$");

                InputText.Alignment = Alignment.MidRight;
                InputText.X = -config.ValueTextPadding;
                InputText.Tint = SkinV2Color.Parse(config.ValueTextColor);
                InputText.Visible = false;
                Cursor.Tint = SkinV2Color.Parse(config.ValueTextColor);

                Button.ClickedOutside += (sender, args) => EndEditing(true);

                ScissorPropagation.Apply(this);
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                if (Focused && !Editing)
                    BeginEditing();
                else if (!Focused && Editing)
                    EndEditing(true);

                UpdateBadgeHoverFade(gameTime);
            }

            public override void Destroy()
            {
                if (ActiveEditor == this)
                    ActiveEditor = null;

                base.Destroy();
            }

            /// <summary>
            ///     Throws away the typed text and shows the value again.
            /// </summary>
            internal void CancelEditing() => EndEditing(false);

            private void BeginEditing()
            {
                Editing = true;
                ActiveEditor = this;

                RawText = Slider.GetEditableText();
                CursorPosition = RawText.Length;
                Slider.ValueText.Visible = false;
                InputText.Visible = true;
                DeselectAndReadjust();
            }

            private void EndEditing(bool commit)
            {
                if (!Editing)
                    return;

                var text = RawText;

                Editing = false;
                Focused = false;

                if (ActiveEditor == this)
                    ActiveEditor = null;

                RawText = "";
                InputText.Visible = false;
                Slider.ValueText.Visible = true;
                DeselectAndReadjust();

                if (commit)
                    Slider.CommitEditedText(text);
            }

            /// <summary>
            ///     Fades the value box on hover, like the bar, so it looks clickable.
            /// </summary>
            private void UpdateBadgeHoverFade(GameTime gameTime)
            {
                var background = Slider.ValueBackground;
                var target = Button.IsHovered || Editing ? 0.75f : 1f;

                if (Math.Abs(background.Alpha - target) < 0.001f)
                {
                    background.Alpha = target;
                    return;
                }

                background.Alpha = MathHelper.Lerp(background.Alpha, target,
                    (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));
            }

            /// <summary>
            ///     Enter saves the value. The base text box would submit and clear the text instead.
            /// </summary>
            protected override void HandleEnter()
            {
                if (Wobble.Platform.TextInputManager.IsTextCompositionActive ||
                    Wobble.Platform.TextInputManager.ConsumeTextCompositionCommitPending() ||
                    ConsumeTextInputReceivedThisFrame())
                    return;

                if (Focused && KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.Enter))
                    EndEditing(true);
            }

            /// <summary>
            ///     Rejects a typed character that would put the number out of range.
            /// </summary>
            protected override void OnTextInputEntered(object sender, TextInputEventArgs e)
            {
                var previousText = RawText;
                var previousCursor = CursorPosition;

                base.OnTextInputEntered(sender, e);

                RejectOutOfRangeInsertion(previousText, previousCursor);
            }

            /// <summary>
            ///     Rejects a pasted number that is out of range.
            /// </summary>
            protected override void HandleCtrlInput()
            {
                var previousText = RawText;
                var previousCursor = CursorPosition;

                base.HandleCtrlInput();

                RejectOutOfRangeInsertion(previousText, previousCursor);
            }

            /// <summary>
            ///     Undoes added text that put the number out of range. Deleting is always allowed, because
            ///     the value is limited to the range when it is saved anyway.
            /// </summary>
            private void RejectOutOfRangeInsertion(string previousText, int previousCursor)
            {
                if (!Editing || RawText.Length <= previousText.Length || IsWithinRangeWhileTyping(RawText))
                    return;

                RawText = previousText;
                CursorPosition = previousCursor;
            }

            /// <summary>
            ///     Text that is not a number yet ("", "-", ".") is allowed. A positive number may not go
            ///     over the maximum, and a negative number may not go under the minimum. More digits only
            ///     move a number further from zero, so a rejected number can never become valid again.
            /// </summary>
            private bool IsWithinRangeWhileTyping(string text)
            {
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                    return true;

                return text.StartsWith("-", StringComparison.Ordinal)
                    ? parsed >= Slider.MinValue
                    : parsed <= Slider.MaxValue;
            }

            /// <summary>
            ///     Where the right-aligned text starts. The base text box assumes left-aligned text.
            /// </summary>
            private float TextLeftEdge => ContentContainer.Width - InputText.Width + InputText.X;

            protected override void CalculateContainerX()
            {
                ContentContainer.Width = Width;
                ContentContainer.X = 0;
            }

            protected override void ChangeCursorLocation()
            {
                if (Cursor == null || InputText == null)
                    return;

                Cursor.X = TextLeftEdge + MeasureTextWidth(CursorPosition);
            }

            protected override void UpdateSelectedSprite()
            {
                if (SelectedSprite == null)
                    return;

                SelectedSprite.Visible = Selected;

                if (!Selected)
                    SelectedPart = (0, 0);

                var start = MeasureTextWidth(SelectedPart.start);
                SelectedSprite.X = TextLeftEdge + start;
                SelectedSprite.Width = MeasureTextWidth(SelectedPart.end) - start;
            }
        }
    }
}
