using System;
using MonoGame.Extended;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSliderGlobalOffset : OptionsSlider
    {
        public OptionsItemSliderGlobalOffset(RectangleF containerRect, string name, BindableInt bindedValue)
            : base(containerRect, name, bindedValue, i => $"{i} ms", "Use when there is a audio delay to counteract.\nSet negative offset when hitting late and vice versa.")
        {
        }
    }
}