using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Graphics
{
    public class ContainedCheckbox : Checkbox
    {
        /// <summary>
        /// </summary>
        private Drawable Container { get; }

        public ContainedCheckbox(Drawable container, Bindable<bool> bindedValue, Vector2 size, Texture2D activeImage,
            Texture2D inactiveImage, bool disposeBindableOnDestroy) : base(bindedValue, size, activeImage, inactiveImage, disposeBindableOnDestroy)
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