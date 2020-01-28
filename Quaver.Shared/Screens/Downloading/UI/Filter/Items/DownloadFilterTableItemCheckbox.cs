using Quaver.Shared.Graphics.Form;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemCheckbox : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private QuaverCheckbox Checkbox { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public DownloadFilterTableItemCheckbox(int width, string name, Bindable<bool> value) : base(width, name)
        {
            Checkbox = new QuaverCheckbox(value)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };
        }
    }
}