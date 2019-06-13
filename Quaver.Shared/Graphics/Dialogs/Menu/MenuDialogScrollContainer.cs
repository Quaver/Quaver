using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Dialogs.Menu
{
    public class MenuDialogScrollContainer : PoolableScrollContainer<IMenuDialogOption>
    {
        /// <summary>
        /// </summary>
        private MenuDialog Dialog { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="availableItems"></param>
        public MenuDialogScrollContainer(MenuDialog dialog, List<IMenuDialogOption> availableItems) : base(availableItems,
            int.MaxValue, 0, new ScalableVector2(446, 346), new ScalableVector2(446, 346))
        {
            Dialog = dialog;
            Alpha = 0;
            Scrollbar.Tint = ColorHelper.HexToColor("#eeeeee");
            Scrollbar.Width = 6;
            Scrollbar.X = 14;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<IMenuDialogOption> CreateObject(IMenuDialogOption item, int index)
            => new MenuDialogItem(Dialog, this, item, index);
    }
}