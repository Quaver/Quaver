using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Multiplayer.UI.List
{
    public class DrawableMultiplayerPlayerButton : Button
    {
        private PlayerList Container { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="player"></param>
        public DrawableMultiplayerPlayerButton(DrawableMultiplayerPlayer player)
        {
            Container = player.Container as PlayerList;

            Tint = Color.Black;
            Alpha = 0.75f;
            Size = new ScalableVector2(player.Width, player.HEIGHT * 0.85f);

            AddBorder(Color.White, 2);
            Border.Alpha = 0.45f;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.20f : 0.75f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            Border.Alpha = MathHelper.Lerp(Border.Alpha, IsHovered ? 0f : 0.45f,
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
    }
}