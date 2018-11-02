using System;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Database.Scores;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public class DrawableLeaderboardScore : Sprite
    {
        /// <summary>
        ///     The score this drawable represents.
        /// </summary>
        public LocalScore Score { get; }

        /// <summary>
        ///     The height of an inidvidual leaderboard score.
        /// </summary>
        public static int HEIGHT { get; } = 56;

        /// <summary>
        ///     The width of the score.
        /// </summary>
        public static int WIDTH { get; } = 622;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableLeaderboardScore(LocalScore score = null, int rank = -1)
        {
            Score = score;
            Size = new ScalableVector2(WIDTH, HEIGHT);
            Tint = Color.Black;
            Alpha = 0.60f;

            // If there is no score, then we'll consider this to be a "No personal Best" score.
            if (score == null)
            {
                CreateNoPersonalBestScore();
                return;
            }


        }

        /// <summary>
        ///
        /// </summary>
        private void CreateNoPersonalBestScore()
        {
            var nopbSet = new SpriteText(BitmapFonts.Exo2Bold, $"No Personal Best {ConfigManager.LeaderboardSection.Value} Score", 13)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }
    }
}