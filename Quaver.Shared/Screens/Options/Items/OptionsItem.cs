using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items
{
    public class OptionsItem : Sprite
    {
        /// <summary>
        /// </summary>
        public SpriteTextPlus Name { get; protected set; }

        /// <summary>
        /// </summary>
        protected RectangleF ContainerRectangle { get; set; }

        /// <summary>
        ///     Any tags to search the item by
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        ///     Used to determine if the options search bar should be focused.
        /// </summary>
        public bool Focused { get; protected set; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItem(RectangleF containerRect, string name)
        {
            ContainerRectangle = containerRect;

            Image = UserInterface.OptionsItemBackground;
            Size = new ScalableVector2(containerRect.Width * 0.96f, 54);

            Tint = ColorHelper.HexToColor("#242424");
            SetChildrenVisibility = true;

            CreateName(name);

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var color = IsHovered() ? ColorHelper.HexToColor("#3F3F3F") : ColorHelper.HexToColor("#242424");

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            FadeToColor(color, dt, 20);

            // Set visibility based on if the options item is visible inside of the container.
            // Helps to raise FPS by not drawing unnecessary items
            bool visible;

            if (Parent is Drawable contentContainer && contentContainer.Parent is ScrollContainer container)
                visible = container.Visible && !RectangleF.Intersection(ScreenRectangle, container.ScreenRectangle).IsEmpty;
            else
                visible = Parent != null;

            ApplyVisibility(visible);

            base.Update(gameTime);
        }

        public void ApplyVisibility(bool visible)
        {
            SetDrawableTreeVisible(this, visible);

            if (visible)
            {
                foreach (var dropdown in GetDropdowns(this))
                    dropdown.ApplyItemVisibilityState();
            }
        }

        private static void SetDrawableTreeVisible(Drawable drawable, bool visible)
        {
            drawable.Visible = visible;

            foreach (var child in drawable.Children)
                SetDrawableTreeVisible(child, visible);
        }

        private static IEnumerable<Dropdown> GetDropdowns(Drawable drawable)
        {
            if (drawable is Dropdown currentDropdown)
                yield return currentDropdown;

            foreach (var child in drawable.Children)
            {
                foreach (var dropdown in GetDropdowns(child))
                    yield return dropdown;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void CreateName(string name)
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name, 21)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 16,
                UsePreviousSpriteBatchOptions = true,
            };
        }
    }
}
