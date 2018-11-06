using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Resources;
using Quaver.Scheduling;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public class LeaderboardContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent select screenview.
        /// </summary>
        public SongSelectScreenView View { get; }

        /// <summary>
        ///     All of the leaderboard sections to display scores.
        /// </summary>
        public Dictionary<LeaderboardType, LeaderboardScoreSection> Sections { get; } = new Dictionary<LeaderboardType, LeaderboardScoreSection>();

        /// <summary>
        ///     The text that displays that there are no scores available.
        /// </summary>
        private SpriteText NoScoresAvailableText { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="view"></param>
        public LeaderboardContainer(SongSelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(View.Banner.Width, 370);
            Alpha = 0;

            CreateNoScoresAvailableText();
            CreateSections();
            SwitchSections(ConfigManager.LeaderboardSection.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the text that displays that there are no scores available.
        /// </summary>
        private void CreateNoScoresAvailableText() => NoScoresAvailableText = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 13)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Visible = false
        };

        /// <summary>
        ///     Creates the text that says PB
        /// </summary>
        private void CreateBestScore(LocalScore score)
        {
            /*BestScore = new DrawableLeaderboardScore(score)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 302
            };

            BestScore.AddBorder(Color.White);*/
        }

        /// <summary>
        ///     Creates all of the leaderboard sections that will be displayed.
        /// </summary>
        private void CreateSections()
        {
            Sections[LeaderboardType.Local] = new LeaderboardScoreSectionLocal(this) {Parent = this};
            Sections[LeaderboardType.Global] = new LeaderboardScoreSectionGlobal(this) {Parent = this};
        }

        /// <summary>
        ///     Switches to a different section on the leaderboards.
        /// </summary>
        /// <param name="type"></param>
        public void SwitchSections(LeaderboardType type)
        {
            ConfigManager.LeaderboardSection.Value = type;

            foreach (var section in Sections)
            {
                if (section.Key == type)
                {
                    section.Value.Visible = true;
                }
                else
                {
                    section.Value.Visible = false;
                }
            }

            LoadScores();
        }

        /// <summary>
        ///     Loads scores for the current map.
        /// </summary>
        public void LoadScores()
        {
            var map = MapManager.Selected.Value;
            var section = Sections[ConfigManager.LeaderboardSection.Value];

            section.IsFetching = true;
            NoScoresAvailableText.Visible = false;

            ThreadScheduler.Run(() =>
            {

            });
        }
    }
}