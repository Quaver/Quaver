using System;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSliderGlobalOffset : OptionsSlider
    {
        public OptionsItemSliderGlobalOffset(float containerWidth, string name, BindableInt bindedValue) : base(containerWidth, name, bindedValue,
            i => $"{i} ms")
        {
        }
    }
}