using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Graphics.Form.Dropdowns.RightClick
{
    public class RightClickOptions : Dropdown
    {
        /// <summary>
        /// </summary>
        protected Dictionary<string, Color> Options { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="size"></param>
        /// <param name="fontSize"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        public RightClickOptions(Dictionary<string, Color> options, ScalableVector2 size, int fontSize, 
            int maxWidth = 0, int maxHeight = 0) 
            : base(options.Keys.ToList(), size, fontSize, Color.White, 0, maxWidth, maxHeight)
        {
            Options = options;

            Chevron.Visible = false;
            SelectedText.Visible = false;
            DividerLine.Visible = false;
            HoverSprite.Visible = false;
            Alpha = 0;
            IsClickable = false;
            DestroyIfParentIsNull = false;
            ItemContainer.Y = 0;

            // Remove the button entirely to prevent depth collisions since the original dropdown opener isn't needed
            ButtonManager.Remove(this);

            var i = 0;

            foreach (var option in options)
            {
                if (i == 0)
                    Items[i].Image = UserInterface.DropdownOpen;

                Items[i].Text.Tint = option.Value;
                i++;
            }

            Open();

            SelectedIndex = -1;
            ItemSelected += (sender, args) => SelectedIndex = -1;
        }
    }
}