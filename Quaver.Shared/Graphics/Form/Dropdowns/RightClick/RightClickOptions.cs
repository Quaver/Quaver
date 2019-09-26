using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Form.Dropdowns.RightClick
{
    public class RightClickOptions : Dropdown
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="size"></param>
        /// <param name="fontSize"></param>
        public RightClickOptions(Dictionary<string, Color> options, ScalableVector2 size, int fontSize) : base(options.Keys.ToList(), size, fontSize, Color.White)
        {
            Chevron.Visible = false;
            SelectedText.Visible = false;
            DividerLine.Visible = false;
            HoverSprite.Visible = false;
            Alpha = 0;
            ItemContainer.Y = 0;
            IsClickable = false;


            var i = 0;

            foreach (var option in options)
            {
                if (i == 0)
                    Items[i].Image = UserInterface.DropdownOpen;

                Items[i].Text.Tint = option.Value;
                i++;
            }

            Open();
        }
    }
}