using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Resources;
using Quaver.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
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
        ///     The best score achieved on the map.
        /// </summary>
        private DrawableLeaderboardScore BestScore { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="view"></param>
        public LeaderboardContainer(SongSelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(View.Banner.Width, 370);
            Alpha = 0;

            CreateSections();
            SwitchSections(ConfigManager.LeaderboardSection.Value);

            MapManager.Selected.ValueChanged += OnMapChange;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChange;
            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that says PB
        /// </summary>
        private void CreateBestScore(LocalScore score)
        {
            BestScore = new DrawableLeaderboardScore(score)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = 302
            };

            BestScore.AddBorder(Color.White);
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

            UpdateLeaderboardWithScores();
        }

        /// <summary>
        ///     Updates the leaderboard with new scores.
        /// </summary>
        public void UpdateLeaderboardWithScores()
        {
            if (BestScore != null)
            {
                lock (BestScore)
                    BestScore?.Destroy();
            }

            var section = Sections[ConfigManager.LeaderboardSection.Value];

            // Grab the map at the beginning before we fetch.
            var mapAtBeginning = MapManager.Selected.Value;

            section.ClearScores();
            section.IsFetching = true;

            Scheduler.RunThread(() =>
            {
                try
                {
                    var scores = section.FetchScores();
                    section.IsFetching = false;

                    if (mapAtBeginning != MapManager.Selected.Value)
                        return;

                    section.UpdateWithScores(scores);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            });
        }

        /// <summary>
        ///     Called when the selected map has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e) => UpdateLeaderboardWithScores();
    }
}