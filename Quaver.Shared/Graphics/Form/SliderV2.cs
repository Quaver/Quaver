using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form
{
    public class SliderV2 : Container
    {
        public event EventHandler ValueEdited;

        private BindableFloat BindedValue { get; }

        private BindableInt IntegerValue { get; set; }

        private float IntegerDisplayScale { get; set; }

        private bool OwnsFloatValue { get; set; }

        private int DecimalPrecision { get; }

        private Container SliderArea { get; }

        private Sprite Fill { get; }

        private RoundedButton Input { get; }

        private ValuePickerTextbox ValuePicker { get; }

        public SliderV2(BindableInt bindedValue, ScalableVector2 sliderSize, int decimalPrecision = 0,
            float displayScale = 1f, bool pickerOnRight = true)
            : this(CreateFloatValue(bindedValue, displayScale), sliderSize, decimalPrecision, pickerOnRight)
        {
            IntegerValue = bindedValue;
            IntegerDisplayScale = displayScale;
            OwnsFloatValue = true;
            IntegerValue.ValueChanged += OnIntegerValueChanged;
            BindedValue.ValueChanged += OnFloatValueChanged;
        }

        private static BindableFloat CreateFloatValue(BindableInt value, float scale)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0)
                throw new ArgumentOutOfRangeException(nameof(scale));

            return new BindableFloat(value.Value / scale, value.MinValue / scale, value.MaxValue / scale);
        }

        public SliderV2(BindableFloat bindedValue, ScalableVector2 sliderSize, int decimalPrecision, bool pickerOnRight = true)
        {
            BindedValue = bindedValue ?? new BindableFloat(0f, 0f, 100f);

            if (decimalPrecision < 0 || decimalPrecision > 9)
                throw new ArgumentOutOfRangeException(nameof(decimalPrecision), "Decimal precision must be between 0 and 9.");

            DecimalPrecision = decimalPrecision;

            if (float.IsNaN(BindedValue.MinValue) || float.IsInfinity(BindedValue.MinValue) ||
                float.IsNaN(BindedValue.MaxValue) || float.IsInfinity(BindedValue.MaxValue) ||
                BindedValue.MaxValue <= BindedValue.MinValue)
                throw new ArgumentException("The slider needs a finite range with maximum greater than minimum.", nameof(bindedValue));

            if (float.IsNaN(sliderSize.X.Value) || float.IsInfinity(sliderSize.X.Value) || sliderSize.X.Value <= 0 ||
                float.IsNaN(sliderSize.Y.Value) || float.IsInfinity(sliderSize.Y.Value) || sliderSize.Y.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(sliderSize));

            Size = new ScalableVector2(sliderSize.X.Value + 10 + 72, Math.Max(sliderSize.Y.Value, 30));

            var layout = new FlexContainer
            {
                Parent = this,
                Size = Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = 10
            };

            SliderArea = new Container
            {
                Parent = layout,
                Size = sliderSize
            };
            layout.SetItemOptions(SliderArea, new FlexItemOptions { Basis = sliderSize.X.Value, Shrink = 0, Order = pickerOnRight ? 0 : 1 });

            var track = new Sprite
            {
                Parent = SliderArea,
                Size = sliderSize,
                Image = RoundedRectTextureCache.Get(SliderArea.Width, SliderArea.Height, 6),
                Tint = ColorHelper.FromHex("#273038")
            };

            Fill = new Sprite
            {
                Parent = SliderArea,
                Size = sliderSize,
                Tint = ColorHelper.FromHex("#6B83B2")
            };

            Input = new RoundedButton
            {
                Parent = SliderArea,
                Size = sliderSize,
                CornerRadius = SliderArea.Height / 2,
                Tint = Color.Transparent,
                PerformHoverFade = false
            };

            var defaultValue = FormatValue(BindedValue.Value);
            ValuePicker = new ValuePickerTextbox(new ScalableVector2(72, 30), defaultValue)
            {
                Parent = layout,
                Image = RoundedRectTextureCache.Get(72, 30, 6),
                Tint = ColorHelper.FromHex("#273038"),
                AllowSubmission = false,
                InputEnabled = false,
                MaxCharacters = Math.Max(FormatValue(BindedValue.MinValue).Length, FormatValue(BindedValue.MaxValue).Length),
                AllowedCharacters = new Regex(CreateValuePattern(), RegexOptions.Compiled)
            };
            ValuePicker.InputText.Tint = ColorHelper.FromHex("#8CAFEA");
            ValuePicker.Cursor.Tint = ColorHelper.FromHex("#FFFFFF");
            layout.SetItemOptions(ValuePicker, new FlexItemOptions { Basis = 72, Shrink = 0, Order = pickerOnRight ? 1 : 0 });
            layout.RefreshLayout();

            BindedValue.ValueChanged += OnValueChanged;
            Input.Clicked += OnClicked;
            ValuePicker.OnStoppedTyping += OnPickerTextChanged;
            ValuePicker.Button.ClickedOutside += OnPickerClickedOutside;
            UpdateFill();
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<float> e)
        {
            UpdateFill();
            if (!ValuePicker.Focused)
                ValuePicker.RawText = FormatValue(e.Value);
        }

        private void OnIntegerValueChanged(object sender, BindableValueChangedEventArgs<int> e) =>
            BindedValue.Value = e.Value / IntegerDisplayScale;

        private void OnFloatValueChanged(object sender, BindableValueChangedEventArgs<float> e)
        {
            var previousValue = IntegerValue.Value;
            IntegerValue.Value = (int)Math.Round(e.Value * IntegerDisplayScale, MidpointRounding.AwayFromZero);
            if (IntegerValue.Value != previousValue)
                ValueEdited?.Invoke(this, EventArgs.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsHeld && Input.MouseButtonClicked == MouseButton.Left)
            {
                if (ValuePicker.Focused)
                {
                    CommitPickerText();
                    ValuePicker.Focused = false;
                }

                UpdateValue();
            }

            if (ValuePicker.Focused && KeyboardManager.IsUniqueKeyPress(Keys.Enter))
            {
                CommitPickerText();
                ValuePicker.Focused = false;
            }

            if (ValuePicker.Focused || !Input.IsHovered)
                return;

            var step = (BindedValue.MaxValue - BindedValue.MinValue) / 100f;
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                BindedValue.Value -= step;
            if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                BindedValue.Value += step;
        }

        private void OnClicked(object sender, EventArgs e) => UpdateValue();

        private void OnPickerTextChanged(string text)
        {
            if (ValuePicker.Focused && TryParseValue(text, out var value))
            {
                BindedValue.Value = value;

                if (value < BindedValue.MinValue || value > BindedValue.MaxValue)
                    ValuePicker.RawText = FormatValue(BindedValue.Value);
            }
        }

        private void OnPickerClickedOutside(object sender, EventArgs e) => CommitPickerText();

        private void CommitPickerText()
        {
            if (TryParseValue(ValuePicker.RawText, out var value))
                BindedValue.Value = value;

            ValuePicker.RawText = FormatValue(BindedValue.Value);
        }

        private static bool TryParseValue(string text, out float value) =>
            float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
            !float.IsNaN(value) && !float.IsInfinity(value);

        private string FormatValue(float value)
        {
            var format = $"F{DecimalPrecision}";
            var text = value.ToString(format, CultureInfo.InvariantCulture);

            if (TryParseValue(text, out var displayed) &&
                displayed >= BindedValue.MinValue && displayed <= BindedValue.MaxValue)
                return text;

            var scale = Math.Pow(10, DecimalPrecision);
            var insideValue = displayed < BindedValue.MinValue ? Math.Ceiling(BindedValue.MinValue * scale) / scale : Math.Floor(BindedValue.MaxValue * scale) / scale;
            text = insideValue.ToString(format, CultureInfo.InvariantCulture);

            if (!TryParseValue(text, out displayed) ||
                displayed < BindedValue.MinValue || displayed > BindedValue.MaxValue)
                throw new ArgumentException("Decimal precision is too low to display a value within the slider range.");

            return text;
        }

        private string CreateValuePattern()
        {
            var sign = BindedValue.MinValue < 0 ? "-?" : string.Empty;
            return DecimalPrecision == 0 ? $"^{sign}\\d*$" : $"^{sign}\\d*(?:\\.\\d{{0,{DecimalPrecision}}})?$";
        }

        private void UpdateValue()
        {
            var amount = MathHelper.Clamp((MouseManager.CurrentState.X - Input.AbsolutePosition.X) / Input.AbsoluteSize.X, 0, 1);
            BindedValue.Value = BindedValue.MinValue + amount * (BindedValue.MaxValue - BindedValue.MinValue);
        }

        private void UpdateFill()
        {
            var amount = (BindedValue.Value - BindedValue.MinValue) / (BindedValue.MaxValue - BindedValue.MinValue);
            var fillWidth = SliderArea.Width * MathHelper.Clamp(amount, 0, 1);

            Fill.Visible = fillWidth > 0;
            if (!Fill.Visible)
                return;

            Fill.Width = fillWidth;
            Fill.Image = RoundedRectTextureCache.Get(fillWidth, SliderArea.Height, 6);
        }

        public override void Destroy()
        {
            if (IntegerValue != null)
            {
                IntegerValue.ValueChanged -= OnIntegerValueChanged;
                BindedValue.ValueChanged -= OnFloatValueChanged;
            }
            BindedValue.ValueChanged -= OnValueChanged;
            Input.Clicked -= OnClicked;
            ValuePicker.OnStoppedTyping -= OnPickerTextChanged;
            ValuePicker.Button.ClickedOutside -= OnPickerClickedOutside;
            base.Destroy();
            if (OwnsFloatValue)
                BindedValue.Dispose();
        }

        private sealed class ValuePickerTextbox : Textbox
        {
            public ValuePickerTextbox(ScalableVector2 size, string initialText) : base(size, FontManager.GetWobbleFont(Fonts.InterSemiBold), 18, initialText)
            {
            }

            protected override void CalculateContainerX()
            {
                base.CalculateContainerX();

                if (InputText != null && InputText.Width + 20 <= Width)
                    ContentContainer.X = (Width - InputText.Width) / 2 - InputText.X;
            }
        }
    }
}
