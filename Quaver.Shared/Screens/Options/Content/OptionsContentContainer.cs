using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Items;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Content
{
    public class OptionsContentContainer : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private OptionsSection Section { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="section"></param>
        /// <param name="size"></param>
        public OptionsContentContainer(OptionsSection section, ScalableVector2 size) : base(size, size)
        {
            Section = section;
            Size = size;
            Alpha = 0;
            DestroyIfParentIsNull = false;

            Initialize();
        }

        /// <summary>
        /// </summary>
        private void Initialize()
        {
            const float spacing = 26f;

            var totalHeight = spacing;

            var dropdowns = new List<OptionsItemDropdown>();

            foreach (var subcategory in Section.Subcategories)
            {
                // Create header if the subcategory has a valid name
                if (!string.IsNullOrEmpty(subcategory.Name))
                {
                    var header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), subcategory.Name, 22)
                    {
                        Position = new ScalableVector2(22, totalHeight),
                        Tint = ColorHelper.HexToColor("#CACACA")
                    };

                    AddContainedDrawable(header);
                    totalHeight += header.Height + 16;
                }

                foreach (var item in subcategory.Items)
                {
                    if (item is OptionsItemDropdown)
                        dropdowns.Add((OptionsItemDropdown) item);
                    else
                        AddContainedDrawable(item);

                    item.Alignment = Alignment.TopCenter;
                    item.Y = totalHeight;

                    totalHeight += item.Height + spacing;
                }
            }

            // Make sure all dropdowns appear on top
            for (var i = dropdowns.Count - 1; i >= 0; i--)
                AddContainedDrawable(dropdowns[i]);

            ContentContainer.Height = Math.Max(Height, totalHeight);
        }
    }
}