using System;
using System.Collections.Generic;
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

namespace Quaver.Shared.Screens.V2.Downloading.UI
{
    internal abstract class DownloadingSearchTextbox : Textbox
    {
        private SkinV2DownloadingFieldConfig Config { get; }

        private Color TextColor { get; }

        private Color PlaceholderColor { get; }

        protected DownloadingSearchTextbox(ScalableVector2 size, WobbleFontStore font,
            SkinV2DownloadingFieldConfig config, string initialText, string placeholder)
            : base(size, font, config.FontSize, initialText, placeholder)
        {
            Config = config;
            TextColor = SkinV2Color.Parse(config.TextColor);
            PlaceholderColor = SkinV2Color.Parse(config.PlaceholderColor);
            Tint = SkinV2Color.Parse(config.BackgroundColor);
            Cursor.Tint = SkinV2Color.Parse(config.CursorColor);
            Scrollbar.Visible = false;
            InputEnabled = false;
            StoppedTypingActionCalltime = 250;
            ApplySize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            InputText.Tint = string.IsNullOrEmpty(RawText) ? PlaceholderColor : TextColor;
            InputText.Alpha = 1;
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Config != null)
                ApplySize();
        }

        private void ApplySize()
        {
            var texture = RoundedRectTextureCache.Get(Width, Height, Config.CornerRadius);
            if (Image != texture)
                Image = texture;
            Button.Size = Size;
            ContentContainer.Size = Size;
        }
    }

    internal sealed class DownloadingSearchQueryTextbox : DownloadingSearchTextbox
    {
        private Bindable<string> Query { get; }

        private Sprite SearchIcon { get; }

        public DownloadingSearchQueryTextbox(Bindable<string> query, string placeholder,
            WobbleFontStore font, SkinV2DownloadingFieldConfig config)
            : base(new ScalableVector2(config.SearchWidth, config.Height), font, config,
                query.Value, placeholder)
        {
            Query = query;
            InputText.X = config.SearchIconInset * 2 + config.SearchIconSize;
            SearchIcon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = config.SearchIconInset,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass),
                Size = new ScalableVector2(config.SearchIconSize, config.SearchIconSize),
                Tint = SkinV2Color.Parse(config.PlaceholderColor),
                UsePreviousSpriteBatchOptions = true
            };

            OnStoppedTyping += OnQueryChanged;
            Query.ValueChanged += OnBoundQueryChanged;
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

    internal sealed class DownloadingNumericTextbox : DownloadingSearchTextbox
    {
        private static readonly Regex NumericCharacters =
            new Regex(@"^(?!.*\..*\.)[.\d]*$", RegexOptions.Compiled);

        private BindableFloat Value { get; }

        private Func<float, float> Normalize { get; }

        private string Format { get; }

        private bool HasValue { get; set; }

        public DownloadingNumericTextbox(BindableFloat value, string placeholder,
            WobbleFontStore font, SkinV2DownloadingFieldConfig config, float width,
            string format = "0.##", bool showInitialValue = false,
            Func<float, float> normalize = null)
            : base(new ScalableVector2(width, config.Height), font, config,
                showInitialValue ? value.Value.ToString(format, CultureInfo.InvariantCulture) : string.Empty,
                placeholder)
        {
            Value = value;
            Normalize = normalize;
            Format = format;
            HasValue = showInitialValue;
            AllowedCharacters = NumericCharacters;
            MaxCharacters = 8;

            OnStoppedTyping += OnTextChanged;
            Value.ValueChanged += OnBoundValueChanged;
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
            if (HasValue)
                SetFormattedText(args.Value);
        }

        private void SetFormattedText(float value)
        {
            var formatted = value.ToString(Format, CultureInfo.InvariantCulture);
            if (RawText != formatted)
                RawText = formatted;
        }
    }

    internal sealed class DownloadingRangeSlider : Container
    {
        private BindableFloat Minimum { get; }

        private BindableFloat Maximum { get; }

        private SkinV2DownloadingRangeConfig Config { get; }

        private Sprite Track { get; }

        private Sprite SelectedTrack { get; }

        private RangeThumb MinimumThumb { get; }

        private RangeThumb MaximumThumb { get; }

        public DownloadingRangeSlider(BindableFloat minimum, BindableFloat maximum,
            SkinV2DownloadingRangeConfig config)
        {
            Minimum = minimum;
            Maximum = maximum;
            Config = config;
            Size = new ScalableVector2(config.Width, config.ThumbHeight);

            Track = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(config.Width, config.TrackHeight),
                Image = RoundedRectTextureCache.Get(config.Width, config.TrackHeight,
                    config.TrackCornerRadius),
                Tint = SkinV2Color.Parse(config.TrackColor)
            };
            SelectedTrack = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Height = config.TrackHeight,
                Image = RoundedRectTextureCache.Get(config.Width, config.TrackHeight,
                    config.TrackCornerRadius),
                Tint = SkinV2Color.Parse(config.SelectedTrackColor)
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
            Size = new ScalableVector2(Config.ThumbWidth, Config.ThumbHeight),
            CornerRadius = Config.ThumbCornerRadius,
            Tint = SkinV2Color.Parse(Config.ThumbColor),
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
            Track.Image = RoundedRectTextureCache.Get(Width, Config.TrackHeight,
                Config.TrackCornerRadius);

            var usableWidth = Math.Max(1, Width - Config.ThumbWidth);
            var minimumPosition = Normalize(Minimum.Value, Minimum.MinValue, Minimum.MaxValue) * usableWidth;
            var maximumPosition = Normalize(Maximum.Value, Maximum.MinValue, Maximum.MaxValue) * usableWidth;
            MinimumThumb.X = minimumPosition;
            MaximumThumb.X = maximumPosition;
            SelectedTrack.X = minimumPosition + Config.ThumbWidth / 2f;
            SelectedTrack.Width = Math.Max(1, maximumPosition - minimumPosition);
            SelectedTrack.Image = RoundedRectTextureCache.Get(SelectedTrack.Width,
                Config.TrackHeight, Config.TrackCornerRadius);
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

    internal sealed class DownloadingSearchDropdown<T> : Container
    {
        private Bindable<T> Value { get; }

        private IReadOnlyList<KeyValuePair<T, string>> Options { get; }

        private SkinV2DownloadingButtonConfig ButtonConfig { get; }

        private SkinV2DownloadingDropdownConfig DropdownConfig { get; }

        private WobbleFontStore Font { get; }

        private RoundedButton Trigger { get; }

        private Sprite Menu { get; set; }

        public DownloadingSearchDropdown(float width, Bindable<T> value,
            IReadOnlyList<KeyValuePair<T, string>> options, WobbleFontStore font,
            SkinV2DownloadingButtonConfig buttonConfig,
            SkinV2DownloadingDropdownConfig dropdownConfig)
        {
            Value = value;
            Options = options;
            Font = font;
            ButtonConfig = buttonConfig;
            DropdownConfig = dropdownConfig;
            Size = new ScalableVector2(width, buttonConfig.Height);

            Trigger = new RoundedButton((sender, args) => ToggleMenu())
            {
                Parent = this,
                Size = Size,
                CornerRadius = buttonConfig.CornerRadius,
                Tint = SkinV2Color.Parse(buttonConfig.BackgroundColor),
                PerformHoverFade = true,
                Depth = 20
            };
            Trigger.SetIcon(FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_down),
                new Vector2(buttonConfig.IconSize, buttonConfig.IconSize));
            Trigger.SetLabel(font, GetSelectedLabel(), buttonConfig.FontSize,
                SkinV2Color.Parse(buttonConfig.TextColor));
            LayoutTriggerContent();

            Value.ValueChanged += OnValueChanged;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            LayoutTriggerContent();

            if (Menu != null && MouseManager.IsUniqueClick(MouseButton.Left) &&
                !Contains(Trigger.ScreenRectangle, MouseManager.CurrentState.Position) &&
                !Contains(Menu.ScreenRectangle, MouseManager.CurrentState.Position))
                CloseMenu();
        }

        public override void Destroy()
        {
            Value.ValueChanged -= OnValueChanged;
            base.Destroy();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Trigger == null)
                return;

            Trigger.Size = Size;
            LayoutTriggerContent();
        }

        private void ToggleMenu()
        {
            if (Menu != null)
                CloseMenu();
            else
                OpenMenu();
        }

        private void OpenMenu()
        {
            var padding = DropdownConfig.MenuPadding;
            var itemHeight = DropdownConfig.ItemHeight;
            var spacing = DropdownConfig.ItemSpacing;
            Menu = new Sprite
            {
                Parent = this,
                Position = new ScalableVector2(0, Height + DropdownConfig.MenuGap),
                Size = new ScalableVector2(Width,
                    padding * 2 + Options.Count * itemHeight +
                    Math.Max(0, Options.Count - 1) * spacing),
                Tint = SkinV2Color.Parse(DropdownConfig.MenuColor),
                DrawOrder = 100
            };
            Menu.Image = RoundedRectTextureCache.Get(Menu.Width, Menu.Height,
                DropdownConfig.CornerRadius);

            for (var index = 0; index < Options.Count; index++)
            {
                var option = Options[index];
                var selected = EqualityComparer<T>.Default.Equals(option.Key, Value.Value);
                var row = new RoundedButton((sender, args) =>
                {
                    Value.Value = option.Key;
                    CloseMenu();
                })
                {
                    Parent = Menu,
                    Position = new ScalableVector2(padding, padding + index * (itemHeight + spacing)),
                    Size = new ScalableVector2(Width - padding * 2, itemHeight),
                    CornerRadius = DropdownConfig.CornerRadius,
                    Tint = SkinV2Color.Parse(selected
                        ? DropdownConfig.SelectedItemColor
                        : DropdownConfig.ItemColor),
                    PerformHoverFade = true,
                    Depth = 100
                };
                row.SetLabel(Font, option.Value, ButtonConfig.FontSize,
                    SkinV2Color.Parse(ButtonConfig.TextColor));
            }
        }

        private void CloseMenu()
        {
            Menu?.Destroy();
            Menu = null;
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<T> args)
        {
            Trigger.SetLabel(Font, GetSelectedLabel(), ButtonConfig.FontSize,
                SkinV2Color.Parse(ButtonConfig.TextColor));
            LayoutTriggerContent();
        }

        private string GetSelectedLabel()
        {
            foreach (var option in Options)
            {
                if (EqualityComparer<T>.Default.Equals(option.Key, Value.Value))
                    return option.Value;
            }

            return Options.Count == 0 ? string.Empty : Options[0].Value;
        }

        private void LayoutTriggerContent()
        {
            if (Trigger.Label != null)
            {
                Trigger.Label.Alignment = Alignment.MidLeft;
                Trigger.Label.X = ButtonConfig.HorizontalPadding;
            }

            if (Trigger.Icon != null)
            {
                Trigger.Icon.Alignment = Alignment.MidRight;
                Trigger.Icon.X = -ButtonConfig.HorizontalPadding;
            }
        }

        private static bool Contains(MonoGame.Extended.RectangleF rectangle, Vector2 point) =>
            point.X >= rectangle.Left && point.X <= rectangle.Right &&
            point.Y >= rectangle.Top && point.Y <= rectangle.Bottom;
    }
}
