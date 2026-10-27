using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.UI.Filters
{
    /// <summary>
    ///     Screen-supplied visual values for a reusable V2 filter text field.
    /// </summary>
    internal sealed class V2FilterFieldStyle
    {
        public float Height { get; init; }
        public float SearchIconSize { get; init; }
        public float SearchIconInset { get; init; }
        public float CornerRadius { get; init; }
        public Color BackgroundColor { get; init; }
        public Color TextColor { get; init; }
        public Color PlaceholderColor { get; init; }
        public Color CursorColor { get; init; }
    }

    /// <summary>
    ///     Screen-supplied visual values for a reusable V2 range slider.
    /// </summary>
    internal sealed class V2FilterRangeStyle
    {
        public float Width { get; init; }
        public float TrackHeight { get; init; }
        public float ThumbWidth { get; init; }
        public float ThumbHeight { get; init; }
        public float TrackCornerRadius { get; init; }
        public float ThumbCornerRadius { get; init; }
        public Color TrackColor { get; init; }
        public Color SelectedTrackColor { get; init; }
        public Color ThumbColor { get; init; }
    }

    internal abstract class V2FilterTextbox : Textbox
    {
        private V2FilterFieldStyle Style { get; }

        protected V2FilterTextbox(ScalableVector2 size, WobbleFontStore font,
            V2FilterFieldStyle style, int fontSize, string initialText, string placeholder)
            : base(size, font, fontSize, initialText, placeholder)
        {
            Style = style;
            Tint = style.BackgroundColor;
            Cursor.Tint = style.CursorColor;
            Scrollbar.Visible = false;
            InputEnabled = false;
            StoppedTypingActionCalltime = 250;
            ApplySize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            InputText.Tint = string.IsNullOrEmpty(RawText)
                ? Style.PlaceholderColor
                : Style.TextColor;
            InputText.Alpha = 1;
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Style != null)
                ApplySize();
        }

        private void ApplySize()
        {
            Button.Size = Size;
            ContentContainer.Size = Size;

            if (Width <= 0 || Height <= 0 || float.IsNaN(Width) || float.IsNaN(Height) ||
                float.IsInfinity(Width) || float.IsInfinity(Height))
                return;

            var texture = RoundedRectTextureCache.Get(Width, Height, Style.CornerRadius);
            if (Image != texture)
                Image = texture;
        }
    }

    internal sealed class V2FilterSearchTextbox : V2FilterTextbox
    {
        private Bindable<string> Query { get; }

        private HorizontalClippingContainer InputClip { get; }

        private SpriteTextPlus ResultsText { get; }

        private float ResultsTextInset { get; }

        public V2FilterSearchTextbox(Bindable<string> query, string placeholder,
            WobbleFontStore font, int fontSize, V2FilterFieldStyle style, float width)
            : base(new ScalableVector2(width, style.Height), font, style, fontSize,
                query.Value, placeholder)
        {
            Query = query;
            ResultsTextInset = style.SearchIconInset;
            InputText.X = style.SearchIconInset * 2 + style.SearchIconSize;
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = style.SearchIconInset,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass),
                Size = new ScalableVector2(style.SearchIconSize, style.SearchIconSize),
                Tint = style.PlaceholderColor,
                UsePreviousSpriteBatchOptions = true
            };

            InputClip = new HorizontalClippingContainer
            {
                Parent = this,
                Size = Size
            };
            ContentContainer.Parent = InputClip;

            ResultsText = new SpriteTextPlus(font, "0", fontSize)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -ResultsTextInset,
                Tint = style.PlaceholderColor,
                UsePreviousSpriteBatchOptions = true
            };
            UpdateInputClip();

            OnStoppedTyping += OnQueryChanged;
            Query.ValueChanged += OnBoundQueryChanged;
        }

        public void SetResultsText(string text)
        {
            ResultsText.Text = text;
            UpdateInputClip();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (InputClip != null && ResultsText != null)
                UpdateInputClip();
        }

        protected override void CalculateContainerX()
        {
            base.CalculateContainerX();
            if (InputClip == null)
                return;

            var caretRight = ContentContainer.X + Cursor.X + Cursor.Width;
            var visibleRight = InputClip.Width - ResultsTextInset / 2;
            if (caretRight > visibleRight)
                ContentContainer.X -= caretRight - visibleRight;
        }

        private void UpdateInputClip()
        {
            var reservedWidth = ResultsText.Width + ResultsTextInset * 2;
            InputClip.Size = new ScalableVector2(Math.Max(1, Width - reservedWidth), Height);
            CalculateContainerX();
        }

        public override void Destroy()
        {
            OnStoppedTyping -= OnQueryChanged;
            Query.ValueChanged -= OnBoundQueryChanged;
            base.Destroy();
        }

        private void OnQueryChanged(string value)
        {
            if (Query.Value != value)
                Query.Value = value;
        }

        private void OnBoundQueryChanged(object sender, BindableValueChangedEventArgs<string> args)
        {
            var value = args.Value ?? string.Empty;
            if (RawText != value)
                RawText = value;
        }
    }

    internal sealed class V2FilterNumericTextbox : V2FilterTextbox
    {
        private const string InfinitySymbol = "∞";

        private static readonly Regex NumericCharacters =
            new Regex(@"^(?!.*\..*\.)[.\d]*$", RegexOptions.Compiled);

        private BindableFloat Value { get; }
        private Func<float, float> Normalize { get; }
        private string Format { get; }
        private Func<float, bool> ShowInfinity { get; }
        private bool HasValue { get; set; }
        private bool WasFocused { get; set; }

        public V2FilterNumericTextbox(BindableFloat value, string placeholder,
            WobbleFontStore font, int fontSize, V2FilterFieldStyle style, float width,
            string format = "0.##", bool showInitialValue = false,
            Func<float, float> normalize = null, Func<float, bool> showInfinity = null)
            : base(new ScalableVector2(width, style.Height), font, style, fontSize,
                showInitialValue ? FormatValue(value.Value, format, showInfinity) : string.Empty,
                placeholder)
        {
            Value = value;
            Normalize = normalize;
            Format = format;
            ShowInfinity = showInfinity;
            HasValue = showInitialValue;
            AllowedCharacters = NumericCharacters;
            MaxCharacters = 8;

            OnStoppedTyping += OnTextChanged;
            Value.ValueChanged += OnBoundValueChanged;
        }

        public override void Update(GameTime gameTime)
        {
            if (Focused && !WasFocused && ShowInfinity?.Invoke(Value.Value) == true &&
                RawText == InfinitySymbol)
            {
                RawText = string.Empty;
                HasValue = false;
            }
            else if (!Focused && WasFocused && ShowInfinity != null &&
                     (string.IsNullOrEmpty(RawText) || RawText == "."))
            {
                HasValue = true;
                SetFormattedText(Value.Value);
            }

            WasFocused = Focused;
            base.Update(gameTime);
        }

        public override void Destroy()
        {
            OnStoppedTyping -= OnTextChanged;
            Value.ValueChanged -= OnBoundValueChanged;
            base.Destroy();
        }

        private void OnTextChanged(string text)
        {
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return;

            parsed = Normalize?.Invoke(parsed) ?? parsed;
            Value.Value = parsed;
            HasValue = true;
            SetFormattedText(Value.Value);
        }

        private void OnBoundValueChanged(object sender, BindableValueChangedEventArgs<float> args)
        {
            if (HasValue || ShowInfinity != null)
                SetFormattedText(args.Value);
        }

        private void SetFormattedText(float value)
        {
            var formatted = FormatValue(value, Format, ShowInfinity);
            if (RawText != formatted)
                RawText = formatted;
        }

        private static string FormatValue(float value, string format, Func<float, bool> showInfinity) =>
            showInfinity?.Invoke(value) == true
                ? InfinitySymbol
                : value.ToString(format, CultureInfo.InvariantCulture);
    }

    internal sealed class V2FilterRangeSlider : Container
    {
        private BindableFloat Minimum { get; }
        private BindableFloat Maximum { get; }
        private V2FilterRangeStyle Style { get; }
        private Sprite Track { get; }
        private Sprite SelectedTrack { get; }
        private RangeThumb MinimumThumb { get; }
        private RangeThumb MaximumThumb { get; }

        public V2FilterRangeSlider(BindableFloat minimum, BindableFloat maximum,
            V2FilterRangeStyle style)
        {
            Minimum = minimum;
            Maximum = maximum;
            Style = style;
            Size = new ScalableVector2(style.Width, style.ThumbHeight);

            Track = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(style.Width, style.TrackHeight),
                Image = RoundedRectTextureCache.Get(style.Width, style.TrackHeight,
                    style.TrackCornerRadius),
                Tint = style.TrackColor
            };
            SelectedTrack = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Height = style.TrackHeight,
                Image = RoundedRectTextureCache.Get(style.Width, style.TrackHeight,
                    style.TrackCornerRadius),
                Tint = style.SelectedTrackColor
            };

            MinimumThumb = CreateThumb(value => SetMinimum(value));
            MaximumThumb = CreateThumb(value => SetMaximum(value));
            Minimum.ValueChanged += OnValueChanged;
            Maximum.ValueChanged += OnValueChanged;
            RefreshPositions();
        }

        public override void Destroy()
        {
            Minimum.ValueChanged -= OnValueChanged;
            Maximum.ValueChanged -= OnValueChanged;
            base.Destroy();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Track != null)
                RefreshPositions();
        }

        private RangeThumb CreateThumb(Action<float> dragged) => new RangeThumb(dragged)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Size = new ScalableVector2(Style.ThumbWidth, Style.ThumbHeight),
            CornerRadius = Style.ThumbCornerRadius,
            Tint = Style.ThumbColor,
            PerformHoverFade = true,
            Depth = 50
        };

        private void SetMinimum(float normalized)
        {
            var value = Minimum.MinValue + normalized * (Minimum.MaxValue - Minimum.MinValue);
            Minimum.Value = Math.Min(value, Maximum.Value);
        }

        private void SetMaximum(float normalized)
        {
            var value = Maximum.MinValue + normalized * (Maximum.MaxValue - Maximum.MinValue);
            Maximum.Value = Math.Max(value, Minimum.Value);
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<float> args) =>
            RefreshPositions();

        private void RefreshPositions()
        {
            if (Track == null || MinimumThumb == null || MaximumThumb == null)
                return;

            Track.Width = Width;
            Track.Image = RoundedRectTextureCache.Get(Width, Style.TrackHeight,
                Style.TrackCornerRadius);

            var usableWidth = Math.Max(1, Width - Style.ThumbWidth);
            var minimumPosition = Normalize(Minimum.Value, Minimum.MinValue, Minimum.MaxValue) * usableWidth;
            var maximumPosition = Normalize(Maximum.Value, Maximum.MinValue, Maximum.MaxValue) * usableWidth;
            MinimumThumb.X = minimumPosition;
            MaximumThumb.X = maximumPosition;
            SelectedTrack.X = minimumPosition + Style.ThumbWidth / 2f;
            SelectedTrack.Width = Math.Max(1, maximumPosition - minimumPosition);
            SelectedTrack.Image = RoundedRectTextureCache.Get(SelectedTrack.Width,
                Style.TrackHeight, Style.TrackCornerRadius);
        }

        private static float Normalize(float value, float minimum, float maximum) =>
            maximum <= minimum ? 0 : MathHelper.Clamp((value - minimum) / (maximum - minimum), 0, 1);

        private sealed class RangeThumb : RoundedButton
        {
            private Action<float> Dragged { get; }

            public RangeThumb(Action<float> dragged) => Dragged = dragged;

            protected override void OnHeld(GameTime gameTime)
            {
                base.OnHeld(gameTime);
                var parent = Parent;
                if (parent == null)
                    return;

                var usableWidth = Math.Max(1, parent.Width - Width);
                var localX = MouseManager.CurrentState.X - parent.ScreenRectangle.X - Width / 2f;
                Dragged(MathHelper.Clamp(localX / usableWidth, 0, 1));
            }
        }
    }

    /// <summary>
    ///     Keeps a filter panel's visibility fade separate from RoundedButton's hover fade.
    /// </summary>
    internal sealed class V2FilterButton : RoundedButton
    {
        private float HoverAlpha { get; set; } = 1;
        private float ExpansionAlpha { get; set; } = 1;

        public V2FilterButton(EventHandler clickAction = null) : base(clickAction)
        {
        }

        public void SetExpansionAlpha(float alpha)
        {
            ExpansionAlpha = MathHelper.Clamp(alpha, 0, 1);
            ApplyCombinedAlpha();
        }

        public override void Update(GameTime gameTime)
        {
            Alpha = HoverAlpha;
            base.Update(gameTime);
            HoverAlpha = Alpha;
            ApplyCombinedAlpha();
        }

        private void ApplyCombinedAlpha() => Alpha = HoverAlpha * ExpansionAlpha;
    }
}
