using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Shared.Input.Global;
using Wobble.Bindables;
using Wobble.Input;
using Wobble.Managers;
using ModeHelper = Quaver.API.Helpers.ModeHelper;

namespace Quaver.Shared.Screens.V2.Options.Catalog
{
    /// <summary>
    ///     The kind of control a row shows on its right side.
    /// </summary>
    internal enum OptionsRowKind
    {
        Toggle,
        Dropdown,
        Button,
        Slider,
        Keybind,
        GameModeSettings,
        KeyLayout,
        Divider
    }

    /// <summary>
    ///     Describes one option row: its label and its control. It holds no drawables.
    ///     Values are read through callbacks every time the row is built, so a row always shows
    ///     the current setting. Only the properties for the row's <see cref="Kind"/> are set.
    /// </summary>
    internal sealed class OptionsRowDefinition
    {
        internal OptionsRowKind Kind { get; }

        internal string LabelLocalizationKey { get; }

        /// <summary>
        ///     Values placed into the label text, for example "4K" in "{0} Settings".
        /// </summary>
        internal object[] LabelArgs { get; private init; }

        internal Func<bool> ToggleValueProvider { get; private init; }

        internal Action<bool> OnToggleChanged { get; private init; }

        internal IReadOnlyList<string> DropdownOptions { get; private init; }

        internal Func<int> DropdownIndexProvider { get; private init; }

        /// <summary>
        ///     Called with the picked option. Returns false to reject the change, which puts the
        ///     previous option back.
        /// </summary>
        internal Func<string, bool> OnDropdownChanged { get; private init; }

        internal string ButtonLabelLocalizationKey { get; private init; }

        internal Action OnButtonClicked { get; private init; }

        internal float SliderMinValue { get; private init; }

        internal float SliderMaxValue { get; private init; }

        internal Func<float> SliderValueProvider { get; private init; }

        internal Action<float> OnSliderChanged { get; private init; }

        /// <summary>
        ///     Turns the slider value into the text shown in its value box.
        /// </summary>
        internal Func<float, string> SliderValueFormatter { get; private init; }

        internal GlobalKeybindActions KeybindAction { get; private init; }

        internal GameMode GameModeSettingsMode { get; private init; }

        /// <summary>
        ///     One key per lane: a mode's main, scratch or co-op layout.
        /// </summary>
        internal IReadOnlyList<Bindable<GenericKey>> KeyLayoutKeys { get; private init; }

        private OptionsRowDefinition(OptionsRowKind kind, string labelLocalizationKey)
        {
            Kind = kind;
            LabelLocalizationKey = labelLocalizationKey;
        }

        /// <summary>
        ///     The label in the current language.
        /// </summary>
        internal string GetLabel() =>
            LocalizationManager.Get(LabelLocalizationKey, LabelArgs ?? Array.Empty<object>());

        internal static OptionsRowDefinition Toggle(string labelLocalizationKey, Func<bool> valueProvider,
            Action<bool> onChanged, object[] labelArgs = null) =>
            new OptionsRowDefinition(OptionsRowKind.Toggle, labelLocalizationKey)
            {
                ToggleValueProvider = valueProvider,
                OnToggleChanged = onChanged,
                LabelArgs = labelArgs
            };

        /// <summary>
        ///     A toggle that reads and writes a config value.
        /// </summary>
        internal static OptionsRowDefinition Toggle(string labelLocalizationKey, Bindable<bool> config,
            object[] labelArgs = null) =>
            Toggle(labelLocalizationKey, () => config.Value, value => config.Value = value, labelArgs);

        internal static OptionsRowDefinition Dropdown(string labelLocalizationKey, IReadOnlyList<string> options,
            Func<int> indexProvider, Func<string, bool> onChanged) =>
            new OptionsRowDefinition(OptionsRowKind.Dropdown, labelLocalizationKey)
            {
                DropdownOptions = options,
                DropdownIndexProvider = indexProvider,
                OnDropdownChanged = onChanged
            };

        /// <summary>
        ///     A dropdown that selects the option whose text matches <paramref name="selectedOption"/>,
        ///     or the first option when none match.
        /// </summary>
        internal static OptionsRowDefinition Dropdown(string labelLocalizationKey, IReadOnlyList<string> options,
            Func<string> selectedOption, Func<string, bool> onChanged) =>
            Dropdown(labelLocalizationKey, options, () => IndexOrFirst(options, selectedOption()), onChanged);

        internal static OptionsRowDefinition Button(string labelLocalizationKey, string buttonLabelLocalizationKey,
            Action onClicked) =>
            new OptionsRowDefinition(OptionsRowKind.Button, labelLocalizationKey)
            {
                ButtonLabelLocalizationKey = buttonLabelLocalizationKey,
                OnButtonClicked = onClicked
            };

        internal static OptionsRowDefinition Slider(string labelLocalizationKey, float minValue, float maxValue,
            Func<float> valueProvider, Action<float> onChanged, Func<float, string> valueFormatter,
            object[] labelArgs = null) =>
            new OptionsRowDefinition(OptionsRowKind.Slider, labelLocalizationKey)
            {
                SliderMinValue = minValue,
                SliderMaxValue = maxValue,
                SliderValueProvider = valueProvider,
                OnSliderChanged = onChanged,
                SliderValueFormatter = valueFormatter,
                LabelArgs = labelArgs
            };

        /// <summary>
        ///     A slider that reads and writes a whole-number config value, using the config's own range.
        /// </summary>
        internal static OptionsRowDefinition Slider(string labelLocalizationKey, BindableInt config,
            Func<int, string> valueFormatter, object[] labelArgs = null) =>
            Slider(labelLocalizationKey, config.MinValue, config.MaxValue, () => config.Value,
                value => config.Value = (int) Math.Round(value),
                value => valueFormatter((int) Math.Round(value)), labelArgs);

        internal static OptionsRowDefinition Keybind(string labelLocalizationKey, GlobalKeybindActions action) =>
            new OptionsRowDefinition(OptionsRowKind.Keybind, labelLocalizationKey) { KeybindAction = action };

        /// <summary>
        ///     A mode's scroll direction, scroll speed and key layout, labelled "{mode} Settings".
        /// </summary>
        internal static OptionsRowDefinition GameModeSettings(GameMode mode) =>
            new OptionsRowDefinition(OptionsRowKind.GameModeSettings, "Screen_Options_GameModeSettings")
            {
                GameModeSettingsMode = mode,
                LabelArgs = new object[] { ModeHelper.ToShortHand(mode) }
            };

        internal static OptionsRowDefinition KeyLayout(string labelLocalizationKey, GameMode mode,
            IReadOnlyList<Bindable<GenericKey>> keys) =>
            new OptionsRowDefinition(OptionsRowKind.KeyLayout, labelLocalizationKey)
            {
                KeyLayoutKeys = keys,
                LabelArgs = new object[] { ModeHelper.ToShortHand(mode) }
            };

        /// <summary>
        ///     A thin line between groups of rows, for example between two game modes.
        /// </summary>
        internal static OptionsRowDefinition Divider() => new OptionsRowDefinition(OptionsRowKind.Divider, null);

        private static int IndexOrFirst(IReadOnlyList<string> options, string value)
        {
            for (var i = 0; i < options.Count; i++)
            {
                if (options[i] == value)
                    return i;
            }

            return 0;
        }
    }
}
