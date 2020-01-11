using Quaver.Shared.Graphics.Form;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItemCheckbox : OptionsItem
    {
        /// <summary>
        /// </summary>
        private QuaverCheckbox Checkbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="name"></param>
        /// <param name="bindable"></param>
        public OptionsItemCheckbox(float containerWidth, string name, Bindable<bool> bindable) : base(containerWidth, name)
        {
            Checkbox = new QuaverCheckbox(bindable ?? new Bindable<bool>(false))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };

            Clicked += (sender, args) => Checkbox.BindedValue.Value = !Checkbox.BindedValue.Value;
        }
    }
}