using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     Builds the drawable for an <see cref="OptionsRowDefinition"/>. Every control reports its
    ///     changes to <see cref="OptionsRecentlyChangedTracker"/>.
    /// </summary>
    internal static class OptionsRowFactory
    {
        /// <summary>
        ///     Builds a divider line or a label with its control.
        /// </summary>
        internal static Drawable CreateRow(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared, Container dropdownOverlayHost)
        {
            if (row.Kind == OptionsRowKind.Divider)
                return new OptionsDividerV2(config);

            var control = CreateControl(row, font, config, shared, dropdownOverlayHost);
            return new OptionsRowV2(font, config, row.GetLabel(), control, SitsOnLabelBar(row.Kind));
        }

        /// <summary>
        ///     A dropdown styled for option rows.
        /// </summary>
        internal static V2Dropdown<string> CreateDropdown(IReadOnlyList<string> options, Bindable<string> value,
            WobbleFontStore font, SkinV2OptionsRowConfig config, Container overlayHost)
        {
            var entries = options
                .Select(option => (DropdownEntry<string>) new DropdownOption<string>(option, option))
                .ToList();
            var chevron = FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_down);

            return new V2Dropdown<string>(config.DropdownWidth, value, entries, font, config.Dropdown, overlayHost,
                chevronIcon: new TextureRegion(chevron, chevron.Bounds))
            {
                Height = config.RowHeight
            };
        }

        /// <summary>
        ///     Toggles, sliders and key fields sit on top of the label bar. Other controls sit next to a
        ///     shorter label bar with a gap between them.
        /// </summary>
        private static bool SitsOnLabelBar(OptionsRowKind kind) =>
            kind == OptionsRowKind.Toggle || kind == OptionsRowKind.Slider ||
            kind == OptionsRowKind.Keybind || kind == OptionsRowKind.KeyLayout;

        private static Drawable CreateControl(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared, Container dropdownOverlayHost) =>
            row.Kind switch
            {
                OptionsRowKind.Toggle => CreateToggle(row, font, shared),
                OptionsRowKind.Button => CreateButton(row, font, config, shared),
                OptionsRowKind.Dropdown => CreateDropdown(row, font, config, dropdownOverlayHost),
                OptionsRowKind.Slider => CreateSlider(row, font, config, shared),
                OptionsRowKind.Keybind => CreateKeybind(row, font, config, shared),
                OptionsRowKind.GameModeSettings => new OptionsGameModeSettingsV2(font, config, shared,
                    row.GameModeSettingsMode, dropdownOverlayHost, () => OptionsRecentlyChangedTracker.Record(row)),
                OptionsRowKind.KeyLayout => CreateKeyLayout(row, font, config, shared),
                _ => throw new ArgumentOutOfRangeException(nameof(row))
            };

        private static V2Toggle CreateToggle(OptionsRowDefinition row, WobbleFontStore font, SkinV2SharedConfig shared)
        {
            var toggle = new V2Toggle(font, shared.Toggle, row.ToggleValueProvider());

            toggle.ToggledChanged += (sender, isOn) =>
            {
                row.OnToggleChanged(isOn);
                OptionsRecentlyChangedTracker.Record(row);
            };

            return toggle;
        }

        private static V2Button CreateButton(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared)
        {
            var button = new V2Button(font, shared.Button, new ScalableVector2(config.DropdownWidth, config.RowHeight),
                LocalizationManager.Get(row.ButtonLabelLocalizationKey));

            button.Clicked += (sender, args) =>
            {
                row.OnButtonClicked();
                OptionsRecentlyChangedTracker.Record(row);
            };

            return button;
        }

        private static V2Dropdown<string> CreateDropdown(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, Container dropdownOverlayHost)
        {
            var selectedIndex = Math.Clamp(row.DropdownIndexProvider(), 0, row.DropdownOptions.Count - 1);
            var value = new Bindable<string>(row.DropdownOptions[selectedIndex]);
            var dropdown = CreateDropdown(row.DropdownOptions, value, font, config, dropdownOverlayHost);
            var lastAcceptedValue = value.Value;

            dropdown.OptionSelected += (sender, args) =>
            {
                if (!row.OnDropdownChanged(args.Option.Value))
                {
                    value.Value = lastAcceptedValue;
                    return;
                }

                lastAcceptedValue = value.Value;
                OptionsRecentlyChangedTracker.Record(row);
            };

            return dropdown;
        }

        private static V2Slider CreateSlider(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared)
        {
            var slider = new V2Slider(font, shared.Slider, config.RowHeight, row.SliderMinValue, row.SliderMaxValue,
                row.SliderValueProvider(), row.SliderValueFormatter);

            slider.ValueChanged += (sender, value) =>
            {
                row.OnSliderChanged(value);
                OptionsRecentlyChangedTracker.Record(row);
            };

            return slider;
        }

        private static OptionsKeybindV2 CreateKeybind(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared)
        {
            var keybind = new OptionsKeybindV2(font, config, shared, row.KeybindAction);
            keybind.Rebound += (sender, args) => OptionsRecentlyChangedTracker.Record(row);
            return keybind;
        }

        private static OptionsKeyLayoutFieldV2 CreateKeyLayout(OptionsRowDefinition row, WobbleFontStore font,
            SkinV2OptionsRowConfig config, SkinV2SharedConfig shared)
        {
            var keyLayout = new OptionsKeyLayoutFieldV2(font, config, shared.Slider, row.KeyLayoutKeys);
            keyLayout.LayoutChanged += (sender, args) => OptionsRecentlyChangedTracker.Record(row);
            return keyLayout;
        }
    }
}
