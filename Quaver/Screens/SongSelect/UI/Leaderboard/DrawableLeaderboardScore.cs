using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public class DrawableLeaderboardScore : Sprite
    {
        /// <summary>
        ///     The parent leaderboard section.
        /// </summary>
        private LeaderboardScoreSection Section { get; }

        /// <summary>
        ///     The height of an inidvidual leaderboard score.
        /// </summary>
        public static int HEIGHT { get; } = 56;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="section"></param>
        public DrawableLeaderboardScore(LeaderboardScoreSection section)
        {
            Section = section;
            Size = new ScalableVector2(Section.Width, HEIGHT);
            Tint = Color.Black;
            Alpha = 0.60f;
        }
    }
}