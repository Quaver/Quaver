using System;
using System.Globalization;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The control of a "{mode} Settings" row. From left to right: scroll direction (dropdown),
    ///     scroll speed (slider) and key layout. The slider and key layout sit together on their own bar.
    /// </summary>
    internal sealed class OptionsGameModeSettingsV2 : Container, IViewportCullExempt
    {
        /// <summary>
        ///     Keeps updating while any of its controls is busy.
        /// </summary>
        public bool IsCullExempt
        {
            get
            {
                foreach (var child in Children)
                {
                    if (child is IViewportCullExempt exempt && exempt.IsCullExempt)
                        return true;
                }

                return false;
            }
        }

        /// <param name="onChanged">Called when any of the controls changes a setting.</param>
        internal OptionsGameModeSettingsV2(WobbleFontStore font, SkinV2OptionsRowConfig config,
            SkinV2SharedConfig shared, GameMode mode, Container dropdownOverlayHost, Action onChanged)
        {
            Height = config.RowHeight;

            var groupBackground = new RoundedPanel(config.CornerRadius)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Tint = SkinV2Color.Parse(config.BackgroundColor)
            };

            var direction = OptionsRowFactory.CreateDropdown(Enum.GetNames(typeof(ScrollDirection)),
                new Bindable<string>(ConfigManager.ScrollDirections[mode].Value.ToString()), font, config,
                dropdownOverlayHost);
            direction.Parent = this;
            direction.Alignment = Alignment.MidLeft;
            direction.OptionSelected += (sender, args) =>
            {
                if (!Enum.TryParse<ScrollDirection>(args.Option.Value, out var parsed))
                    return;

                ConfigManager.ScrollDirections[mode].Value = parsed;
                onChanged();
            };

            var scrollSpeed = ConfigManager.ScrollSpeeds[mode];
            var groupX = direction.X + direction.Width + config.ControlGap;
            var sliderX = groupX + config.HorizontalPadding;
            var slider = new V2Slider(font, shared.Slider, config.RowHeight, scrollSpeed.MinValue, scrollSpeed.MaxValue,
                scrollSpeed.Value, value => (value / 10f).ToString("0.0", CultureInfo.InvariantCulture))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = sliderX
            };
            slider.ValueChanged += (sender, value) =>
            {
                scrollSpeed.Value = (int) Math.Round(value);
                onChanged();
            };

            var keysX = sliderX + slider.Width + config.ControlGap;
            var keys = new OptionsKeyLayoutFieldV2(font, config, shared.Slider, ConfigManager.KeyLayouts[mode])
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = keysX
            };
            keys.LayoutChanged += (sender, args) => onChanged();

            var width = keysX + keys.Width + config.HorizontalPadding;
            groupBackground.X = groupX;
            groupBackground.Size = new ScalableVector2(width - groupX, config.RowHeight);

            Size = new ScalableVector2(width, config.RowHeight);
        }
    }
}
