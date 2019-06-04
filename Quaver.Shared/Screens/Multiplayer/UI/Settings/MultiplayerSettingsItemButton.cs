using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer.UI.Settings
{
    public class MultiplayerSettingsItemButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private PoolableScrollContainer<IMultiplayerSettingsItem> Container { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="clickAction"></param>
        public MultiplayerSettingsItemButton(PoolableScrollContainer<IMultiplayerSettingsItem> container, EventHandler clickAction)
            : base(UserInterface.BlankBox, clickAction)
        {
            Container = container;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = RectangleF.Intersection(ScreenRectangle, Container.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}