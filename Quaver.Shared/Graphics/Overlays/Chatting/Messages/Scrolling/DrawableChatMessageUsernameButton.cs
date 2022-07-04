using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Chatting.Messages.Scrolling
{
    public class DrawableChatMessageUsernameButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private Drawable ClickableArea { get; }

        public DrawableChatMessageUsernameButton(Texture2D image, Drawable clickableArea) : base(image)  => ClickableArea = clickableArea;

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