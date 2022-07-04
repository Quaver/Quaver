using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class DrawableLeaderboardScoreButton : ImageButton
    {
        /// <summary>
        /// </summary>
        private LeaderboardScoresContainer Container { get; }

        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="image"></param>
        public DrawableLeaderboardScoreButton(LeaderboardScoresContainer container, Texture2D image) : base(image)
            => Container = container;

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