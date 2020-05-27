using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItemCheckbox : OptionsItem
    {
        /// <summary>
        /// </summary>
        private QuaverCheckbox Checkbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="bindable"></param>
        public OptionsItemCheckbox(RectangleF containerRect, string name, Bindable<bool> bindable) : base(containerRect, name)
        {
            var bindedValue = bindable ?? new Bindable<bool>(false);

            Button.Clicked += (sender, args) => bindedValue.Value = !bindedValue.Value;

            Checkbox = new QuaverCheckbox(bindedValue)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                UsePreviousSpriteBatchOptions = true
            };
        }
    }
}