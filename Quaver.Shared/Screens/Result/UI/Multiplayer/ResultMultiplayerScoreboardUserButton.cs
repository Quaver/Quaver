using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.Gameplay.UI.Scoreboard;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerScoreboardUserButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private PoolableScrollContainer<ScoreboardUser> Container { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="container"></param>
        public ResultMultiplayerScoreboardUserButton(ScoreboardUser user, PoolableScrollContainer<ScoreboardUser> container) : base(UserInterface.BlankBox)
        {
            Container = container;
            Alpha = 0;

            Clicked += (o, e) =>
            {
                var game = (QuaverGame) GameBase.Game;
                var results = (ResultScreen) game.CurrentScreen;
                var view = (ResultScreenView) results.View;
                view.SelectedMultiplayerUser.Value = user;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Alpha = MathHelper.Lerp(Alpha, IsHovered ? 0.55f : 0f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
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