using System;
using MonoGame.Extended;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSliderGlobalOffset : OptionsSlider
    {
        public OptionsItemSliderGlobalOffset(RectangleF containerRect, string name, BindableInt bindedValue) 
            : base(containerRect, name, bindedValue, i => $"{i} ms")
        {
        }
    }
}