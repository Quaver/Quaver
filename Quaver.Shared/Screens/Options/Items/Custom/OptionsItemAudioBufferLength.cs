using System;
using MonoGame.Extended;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemAudioBufferLength : OptionsSlider
    {
        /// <summary>
        ///     The device period bindable.
        /// </summary>
        private BindableInt DevicePeriod { get; }

        /// <summary>
        ///     Converts the bound value and the device period value into a string for display.
        /// </summary>
        private Func<int, int, string> Display { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="bindedValue"></param>
        /// <param name="devicePeriod"></param>
        /// <param name="display"></param>
        public OptionsItemAudioBufferLength(RectangleF containerRect, string name, BindableInt bindedValue, BindableInt devicePeriod,
            Func<int, int, string> display) : base(containerRect, name, bindedValue, x => display(x, devicePeriod.Value))
        {
            DevicePeriod = devicePeriod ?? new BindableInt(0,0, 100);
            Display = display;

            DevicePeriod.ValueChanged += OnDevicePeriodChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            DevicePeriod.ValueChanged -= OnDevicePeriodChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDevicePeriodChanged(object sender, BindableValueChangedEventArgs<int> e) =>
            ScheduleUpdate(() => Value.Text = Display(Slider.BindedValue.Value, e.Value));
    }
}