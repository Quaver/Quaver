using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Form.Checkboxes
{
    public class CheckboxContainer : PoolableScrollContainer<ICheckboxContainerItem>
    {
        /// <summary>
        ///     The maximum possible height the container can be
        /// </summary>
        private int MaxHeight { get; }

        /// <summary>
        /// </summary>
        public bool IsOpen { get; private set; } = true;

        /// <summary>
        /// </summary>
        /// <param name="availableItems"></param>
        /// <param name="size"></param>
        /// <param name="maxHeight"></param>
        public CheckboxContainer(List<ICheckboxContainerItem> availableItems, ScalableVector2 size, int maxHeight)
            : base(availableItems, int.MaxValue, 0, size, size)
        {
            InputEnabled = true;
            Tint = ColorHelper.HexToColor("#181818");
            Image = UserInterface.RoundedPanel;

            Scrollbar.X += 6;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 0;

            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1000;
            ScrollSpeed = 220;

            CreatePool();

            Height = 0;
            MaxHeight = maxHeight;

            ChangeHeightTo((int) Math.Min(ContentContainer.Height, maxHeight), Easing.OutQuint, 450);
            RecalculateContainerHeight();
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
        public void Close()
        {
            IsOpen = false;

            ClearAnimations();
            ChangeHeightTo(0, Easing.OutQuint, 450);

            foreach (var item in Pool)
            {
                if (item is CheckboxContainerItem i)
                    i.Button.Visible = false;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<ICheckboxContainerItem> CreateObject(ICheckboxContainerItem item, int index)
            => new CheckboxContainerItem(this, item, index);
    }
}