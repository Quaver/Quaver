using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using TagLib.Ape;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettingsScrollContainer : PoolableScrollContainer<IMultiplayerSettingsItem>
    {
        public MultiplayerSettingsScrollContainer(List<IMultiplayerSettingsItem> availableItems) : base(availableItems, int.MaxValue, 0,
            new ScalableVector2(646, 334), new ScalableVector2(650, 334))
        {
            Scrollbar.Tint = ColorHelper.HexToColor("#eeeeee");
            Scrollbar.Width = 6;
            Scrollbar.X = 14;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;
            InputEnabled = true;
            Alpha = 0;

            CreatePool();

            for (var i = 0; i < Pool.Count; i++)
                Pool[i].Tint = i % 2 == 0 ? ColorHelper.HexToColor("#0f0f0f") : ColorHelper.HexToColor("#2a2a2a");
        }

        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0;

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());
            base.Destroy();
        }

        protected override PoolableSprite<IMultiplayerSettingsItem> CreateObject(IMultiplayerSettingsItem item, int index)
            => new MultiplayerSettingsItem(this, item, index);
    }
}