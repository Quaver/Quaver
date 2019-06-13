using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public class MultiplayerPlayerOptionsContainer : PoolableScrollContainer<IMenuDialogOption>
    {
        private MultiplayerPlayerOptionsDialog Dialog { get; }

        public MultiplayerPlayerOptionsContainer(MultiplayerPlayerOptionsDialog dialog, List<IMenuDialogOption> availableItems) : base(availableItems,
            int.MaxValue, 0, new ScalableVector2(446, 350),
            new ScalableVector2(446, 350))
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

        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<IMenuDialogOption> CreateObject(IMenuDialogOption item, int index)
            => new MultiplayerPlayerOptionItem(Dialog, this, item, index);

        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());
            base.Destroy();
        }
    }
}