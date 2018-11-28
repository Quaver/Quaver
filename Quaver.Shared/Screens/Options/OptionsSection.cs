using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Skinning;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options
{
    public class OptionsSection
    {
        /// <summary>
        ///     The icon for the section.
        /// </summary>
        public OptionsSectionIcon Icon { get; }

        /// <summary>
        ///     The list of options items to be displayed.
        /// </summary>
        public List<OptionsItem> Items { get; }

        /// <summary>
        ///     The container for all of the items.
        /// </summary>
        public Container Container { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="iconImage"></param>
        /// <param name="items"></param>
        /// <param name="isSelected"></param>
        public OptionsSection(OptionsDialog dialog, Texture2D iconImage, List<OptionsItem> items, bool isSelected = false)
        {
            Icon = new OptionsSectionIcon(iconImage, isSelected);

            // When the button is clicked, we want to change the selected dialog section.
            Icon.Clicked += (sender, args) =>
            {
                if (Icon.IsSelected)
                    return;

                SkinManager.Skin.SoundClick.CreateChannel().Play();
                dialog.ChangeSection(this);
            };

            Items = items;

            // Create a container for the section to live in.
            Container = new Container(dialog.ContentContainer.Size, new ScalableVector2(0, 0))
            {
                Parent = dialog.ContentContainer
            };
        }
    }
}
