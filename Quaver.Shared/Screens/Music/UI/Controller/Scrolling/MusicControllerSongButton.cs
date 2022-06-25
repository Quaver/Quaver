using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Music.UI.Controller.Scrolling
{
    public class MusicControllerSongButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private Drawable ClickableArea { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="clickableArea"></param>
        /// <param name="image"></param>
        /// <param name="clickAction"></param>
        public MusicControllerSongButton(Drawable clickableArea, Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
            ClickableArea = clickableArea;
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