using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Items;
using Quaver.Shared.Screens.Options.Sections;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
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
            Scrollbar.Width = 4;
            Scrollbar.X = 0;
            Scrollbar.Tint = ColorHelper.HexToColor("#636363");

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1200;
            ScrollSpeed = 320;

            Initialize();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dropdownHovered = ButtonManager.Buttons.Any(x => x is DropdownItem item &&
                                                 GraphicsHelper.RectangleContains(x.ScreenRectangle, MouseManager.CurrentState.Position) &&
                                                 item.Dropdown.Opened);

            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt)
                           && !dropdownHovered;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            foreach (var category in Section.Subcategories)
                category.ScrolledTo -= OnScrolledToCategory;

            base.Destroy();
        }

        public void ReInitialize()
        {
            for (var i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i] is SpriteTextPlus text)
                    text.Destroy();
            }

            Initialize();
        }

        /// <summary>
        /// </summary>
        private void Initialize()
        {
            const float spacing = 24f;

            var totalHeight = spacing;

            var dropdowns = new List<OptionsItemDropdown>();

            foreach (var subcategory in Section.Subcategories)
            {
                // Create header if the subcategory has a valid name
                if (!string.IsNullOrEmpty(subcategory.Name))
                {
                    var header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), subcategory.Name, 24)
                    {
                        Position = new ScalableVector2(28, totalHeight),
                        Tint = ColorHelper.HexToColor("#45D6F5")
                    };

                    AddContainedDrawable(header);
                    totalHeight += header.Height + 20;
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

            foreach (var category in Section.Subcategories)
                category.ScrolledTo += OnScrolledToCategory;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrolledToCategory(object sender, EventArgs e)
        {
            var category = (OptionsSubcategory) sender;

            foreach (var child in ContentContainer.Children)
            {
                if (child is SpriteTextPlus text && text.Text == category.Name)
                {
                    ContentContainer.ClearAnimations();
                    ScrollTo(-text.Y + 22, 450);

                    break;
                }
            }
        }
    }
}