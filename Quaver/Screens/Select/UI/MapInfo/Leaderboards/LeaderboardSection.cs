using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards
{
    public class LeaderboardSection
    {
        /// <summary>
        ///     The section (in enum form)
        /// </summary>
        public LeaderboardRankingSection RankingSection { get; }

        /// <summary>
        ///     Reference to the parent leaderboard.
        /// </summary>
        public Leaderboard Leaderboard { get; }

        /// <summary>
        ///     The button to select this leaderboard section.
        /// </summary>
        public LeaderboardSectionButton Button { get; }

        /// <summary>
        ///     Holds the content of the leaderboard section.
        /// </summary>
        public ScrollContainer ScrollContainer { get; }

        ///  <summary>
        ///
        ///  </summary>
        /// <param name="rankingSection"></param>
        /// <param name="leaderboard"></param>
        ///  <param name="name"></param>
        public LeaderboardSection(LeaderboardRankingSection rankingSection, Leaderboard leaderboard, string name)
        {
            RankingSection = rankingSection;
            Leaderboard = leaderboard;
            Button = new LeaderboardSectionButton(this, name) {Parent = Leaderboard};

            var size = new ScalableVector2(Leaderboard.Width,
                Leaderboard.Height - Leaderboard.DividerLine.Y + Leaderboard.DividerLine.Height);

            ScrollContainer = new ScrollContainer(size, size)
            {
                Parent = Leaderboard,
                Size = size,
                Y = Leaderboard.DividerLine.Y + 5,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Updates the section itself.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
        }
    }
}