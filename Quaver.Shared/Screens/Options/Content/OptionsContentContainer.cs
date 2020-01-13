using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
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
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                           && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt);

            base.Update(gameTime);
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
        }
    }
}