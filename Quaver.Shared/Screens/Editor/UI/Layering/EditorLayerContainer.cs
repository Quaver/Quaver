using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerContainer : ScrollContainer
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public EditorLayerContainer(ScalableVector2 size) : base(size, size)
        {
            Tint = Color.Transparent;
            Scrollbar.Tint = ColorHelper.HexToColor("#CCCCCC");
            Scrollbar.Width = 3;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }
    }
}