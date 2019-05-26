using System;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Settings.Elements
{
    public class SettingsSliderAudioBufferLength : SettingsSlider
    {
        /// <summary>
        ///     The device period bindable.
        /// </summary>
        private BindableInt DevicePeriod { get; }

        /// <summary>
        ///     Converts the bound value and the device period value into a string for display.
        /// </summary>
        private Func<int, int, string> Display { get; }

        public SettingsSliderAudioBufferLength(SettingsDialog dialog, string name, BindableInt bindable,
            BindableInt devicePeriod, Func<int, int, string> display) : base(dialog, name, bindable, x => display(x, devicePeriod.Value))
        {
            DevicePeriod = devicePeriod;
            Display = display;

            DevicePeriod.ValueChanged += OnDevicePeriodChanged;
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            DevicePeriod.ValueChanged -= OnDevicePeriodChanged;

            base.Destroy();
        }

        private void OnDevicePeriodChanged(object sender, BindableValueChangedEventArgs<int> e) =>
            Value.Text = Display(Bindable.Value, e.Value);
    }
}