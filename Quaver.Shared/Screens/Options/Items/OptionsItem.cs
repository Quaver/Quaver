using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
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
        ///     So that overlapping items (such as dropdowns) will have their hover/click depth maintained
        /// </summary>
        protected ImageButton Button { get; private set; }

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

            CreateButton();
            CreateName(name);

            UsePreviousSpriteBatchOptions = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var color = Button.IsHovered ? ColorHelper.HexToColor("#3F3F3F") : ColorHelper.HexToColor("#242424");

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            FadeToColor(color, dt, 20);

            // Set visibility based on if the options item is visible inside of the container.
            // Helps to raise FPS by not drawing unnecessary items
            if (Parent is Drawable contentContainer && contentContainer.Parent is ScrollContainer container)
                Visible = !RectangleF.Intersect(ScreenRectangle, container.ScreenRectangle).IsEmpty;
            else
                Visible = true;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateButton() => Button = new ImageButton(UserInterface.BlankBox)
        {
            Parent = this,
            Size = Size,
            Position = Position,
            Alpha = 0
        };

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