using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Multiplayer.UI.List
{
    public class DrawableMultiplayerPlayerButton : Button
    {
        private PlayerList Container { get; }

        private Sprite HoverArea { get; }

        public bool IsDisplayed { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        public DrawableMultiplayerPlayerButton(DrawableMultiplayerPlayer player)
        {
            Container = player.Container as PlayerList;

            Image = player.GetPlayerPanel();
            Size = new ScalableVector2(player.Width, player.HEIGHT * 0.90f);

            HoverArea = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 2, Height - 2),
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HoverArea.Alpha = MathHelper.Lerp(HoverArea.Alpha, IsHovered ? 0.4f : 0f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        ///     In this case, we only want buttons to be clickable if they're in the bounds of the scroll container.
        /// </summary>
        /// <returns></returns>
        protected override bool IsMouseInClickArea()
        {
            var newRect = RectangleF.Intersection(ScreenRectangle, Container.ScreenRectangle);
            return GraphicsHelper.RectangleContains(newRect, MouseManager.CurrentState.Position);
        }

        public bool IsVisibleInContainer()
        {
            return RectangleF.Intersects(ScreenRectangle, Container.ContentContainer.ScreenRectangle);
        }
    }
}