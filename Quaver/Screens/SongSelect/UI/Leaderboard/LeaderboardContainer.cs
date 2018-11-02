using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Database.Maps;
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

                    if (mapAtBeginning != MapManager.Selected.Value)
                        return;

                    section.UpdateWithScores(scores);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
                finally
                {
                    section.IsFetching = false;
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