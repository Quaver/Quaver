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

            MapManager.Selected.ValueChanged += OnMapChange;
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
            section.IsFetching = true;
            NoScoresAvailableText.Visible = false;

            section.ClearScores();

            Scheduler.RunAfter(() =>
            {
                try
                {
                    // If we already have scores catched to use, then just use them.
                    if (mapAtBeginning.Scores.Value != null && mapAtBeginning.Scores.Value.Count > 0)
                    {
                        section.IsFetching = false;
                        section.UpdateWithScores(mapAtBeginning.Scores.Value);
                        return;
                    }

                    var scores = section.FetchScores();
                    MapManager.Selected.Value.Scores.Value = scores;

                    lock (NoScoresAvailableText)
                    {
                        if (scores.Count == 0)
                        {
                            NoScoresAvailableText.Text = section.GetNoScoresAvailableString(mapAtBeginning);
                            NoScoresAvailableText.Alpha = 0;
                            NoScoresAvailableText.Visible = true;
                            NoScoresAvailableText.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
                        }
                        else
                        {
                            NoScoresAvailableText.Visible = false;
                        }
                    }

                    if (mapAtBeginning == MapManager.Selected.Value)
                    {
                        section.IsFetching = false;
                        section.UpdateWithScores(scores);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }, 200);
        }

        /// <summary>
        ///     Called when the selected map has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e) => UpdateLeaderboardWithScores();
    }
}