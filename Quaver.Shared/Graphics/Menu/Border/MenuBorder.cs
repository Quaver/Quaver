using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Menu.Border
{
    public class MenuBorder : Sprite
    {
        /// <summary>
        ///     The type of menu border this is, whether a header or footer
        /// </summary>
        private MenuBorderType Type { get; }

        /// <summary>
        /// </summary>
        public static int HEIGHT { get; } = 56;

        /// <summary>
        ///     The line displayed at the top of the footer
        /// </summary>
        public Sprite ForegroundLine { get; private set; }

        /// <summary>
        ///     The line that animates within <see cref="ForegroundLine"/>
        /// </summary>
        public Sprite AnimatedLine { get; private set; }

        /// <summary>
        ///     The items that are aligned from left to right of the footer
        /// </summary>
        protected List<Drawable> LeftAlignedItems { get; }

        /// <summary>
        ///     The items that are aligned from right to left of the footer
        /// </summary>
        protected List<Drawable> RightAlignedItems { get; }

        /// <summary>
        /// </summary>
        public MenuBorder(MenuBorderType type, List<Drawable> leftAligned = null, List<Drawable> rightAligned = null)
        {
            Type = type;

            LeftAlignedItems = leftAligned;
            RightAlignedItems = rightAligned;

            Size = new ScalableVector2(WindowManager.Width, HEIGHT);
            Image = SkinManager.Skin?.MenuBorder?.Background ?? UserInterface.MenuBorderBackground;

            CreateForegroundLine();
            CreateAnimatedLine();

            AlignLeftItems();
            AlignRightItems();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformLineAnimations();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the top line sprite of the footer
        /// </summary>
        private void CreateForegroundLine()
        {
            ForegroundLine = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Alignment = Type == MenuBorderType.Header ? Alignment.BotLeft : Alignment.TopLeft,
                Tint = SkinManager.Skin?.MenuBorder?.BackgroundLineColor ?? Colors.MainBlue
            };
        }

        /// <summary>
        ///     Creates the line that is animated within the top border line
        /// </summary>
        private void CreateAnimatedLine()
        {
            AnimatedLine = new Sprite
            {
                Parent = ForegroundLine,
                Size = new ScalableVector2(150, 2),
                Tint = SkinManager.Skin?.MenuBorder?.ForegroundLineColor ?? Color.White,
            };

            if (Type == MenuBorderType.Header)
                AnimatedLine.X = WindowManager.Width - AnimatedLine.Width;
        }

        /// <summary>
        ///     Aligns the drawables from left to right
        /// </summary>
        protected void AlignLeftItems()
        {
            if (LeftAlignedItems == null || LeftAlignedItems.Count == 0)
                return;

            AlignDrawables(AlignmentDirection.LeftToRight, LeftAlignedItems);
        }

        /// <summary>
        ///     Aligns the drawables from right to left
        /// </summary>
        protected void AlignRightItems()
        {
            if (RightAlignedItems == null || RightAlignedItems.Count == 0)
                return;

            AlignDrawables(AlignmentDirection.RightToLeft, RightAlignedItems);
        }

        /// <summary>
        ///     Aligns drawables based on the direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="items"></param>
        private void AlignDrawables(AlignmentDirection direction, IReadOnlyList<Drawable> items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];

                item.Parent = this;

                item.Y = item is IMenuBorderItem borderItem && borderItem.UseCustomPaddingY ? borderItem.CustomPaddingY : 0;

                if (item.Y == 0 && Type == MenuBorderType.Footer)
                    item.Y = 2;

                item.Alignment = direction == AlignmentDirection.LeftToRight ? Alignment.MidLeft : Alignment.MidRight;

                const int padding = 25;
                var spacing = item is IMenuBorderItem b && b.UseCustomPaddingX ? b.CustomPaddingX : 60;

                if (i == 0)
                    item.X = direction == AlignmentDirection.LeftToRight ? padding : -padding;
                else
                    item.X = direction == AlignmentDirection.LeftToRight ? items[i - 1].X + items[i - 1].Width + spacing
                        : items[i - 1].X - items[i - 1].Width - spacing;
            }
        }

        /// <summary>
        /// </summary>
        private void PerformLineAnimations()
        {
            if (AnimatedLine.Animations.Count != 0)
                return;

            if (AnimatedLine.X > WindowManager.Width / 2f)
                AnimatedLine.MoveToX(0, Easing.Linear, 15000);
            else
                AnimatedLine.MoveToX(WindowManager.Width - AnimatedLine.Width, Easing.Linear, 15000);
        }
    }
}