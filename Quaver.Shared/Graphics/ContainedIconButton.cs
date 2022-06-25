using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Input;

namespace Quaver.Shared.Graphics
{
    public class ContainedIconButton : IconButton
    {
        /// <summary>
        /// </summary>
        private Drawable Container { get; }

        public ContainedIconButton(Drawable container, Texture2D image, EventHandler clickAction = null) : base(image, clickAction)
        {
            Container = container;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            if (Container == null)
                return base.IsMouseInClickArea();

            var newRect = RectangleF.Intersection(ScreenRectangle, Container.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }
    }
}