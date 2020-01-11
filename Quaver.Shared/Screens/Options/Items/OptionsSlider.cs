using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsSlider : OptionsItem
    {
        /// <summary>
        /// </summary>
        private Slider Slider { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Value { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="name"></param>
        /// <param name="bindedValue"></param>
        /// <param name="valueModifier"></param>
        public OptionsSlider(float containerWidth, string name, BindableInt bindedValue, Func<int, string> valueModifier = null)
            : base(containerWidth, name)
        {
            if (bindedValue == null)
                bindedValue = new BindableInt(0, 0, 100);

            Value = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "100%", 22)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                UsePreviousSpriteBatchOptions = true,
                X = -Name.X
            };

            Slider = new Slider(bindedValue, new Vector2(350, 4), UserInterface.VolumeSliderProgressBall)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = Value.X - Value.Width - 24,
                Tint = ColorHelper.HexToColor("#5B5B5B"),
                UsePreviousSpriteBatchOptions = true,
                ActiveColor =
                {
                    Tint = ColorHelper.HexToColor("#45D6F5"),
                    UsePreviousSpriteBatchOptions = true
                }
            };

            Slider.BindedValue.ValueChanged += (sender, args)
                => Value.Text = valueModifier != null ? valueModifier?.Invoke(args.Value) : $"{args.Value}%";

            Slider.BindedValue.TriggerChangeEvent();
        }
    }
}