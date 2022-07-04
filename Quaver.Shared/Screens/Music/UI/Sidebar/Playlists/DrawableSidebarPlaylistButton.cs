using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Music.UI.Sidebar.Playlists
{
    public class DrawableSidebarPlaylistButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private Drawable ClickableArea { get; }

        public DrawableSidebarPlaylistButton(Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = RectangleF.Intersection(ScreenRectangle, ClickableArea.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}