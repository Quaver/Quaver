using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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

        /// <summary>
        ///     To cancel tasks.
        /// </summary>
        private CancellationTokenSource Source { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="view"></param>
        public LeaderboardContainer(SongSelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(View.Banner.Width, 370);
            Alpha = 0;

            Source = new CancellationTokenSource();
            CreateNoScoresAvailableText();
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

            if (Source != null)
            {
                Source.Dispose();
                Source = null;
            }

            base.Destroy();
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

            Source.Cancel();
            Source.Dispose();
            Source = new CancellationTokenSource();
            ThreadScheduler.Run(() => LoadScores(Source.Token).Start());
        }

        /// <summary>
        ///     Loads scores for the current map.
        ///
        ///     This needs to be reworked heavily. I don't like this entire system of cancelling
        ///     the token everywhere.......................................
        ///
        ///     Lord help me.
        /// </summary>
        public Task LoadScores(CancellationToken cancellationToken = default) => new Task(() =>
        {
            var section = Sections[ConfigManager.LeaderboardSection.Value];

            try
            {
                section.ClearScores();
                section.IsFetching = true;
                NoScoresAvailableText.Visible = false;

                Thread.Sleep(300);
                cancellationToken.ThrowIfCancellationRequested();

                var map = MapManager.Selected.Value;

                var scores = section.FetchScores();
                section.IsFetching = false;

                lock (NoScoresAvailableText)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (scores.Count == 0)
                    {
                        NoScoresAvailableText.Text = section.GetNoScoresAvailableString(map);
                        NoScoresAvailableText.Alpha = 0;
                        NoScoresAvailableText.Visible = true;

                        NoScoresAvailableText.ClearAnimations();
                        NoScoresAvailableText.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 150));
                    }
                    else
                    {
                        NoScoresAvailableText.Visible = false;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                section.UpdateWithScores(map, scores, cancellationToken);
            }
            catch (Exception e)
            {
                // ignored.
                section.IsFetching = true;
                NoScoresAvailableText.Visible = false;
            }
        });

        /// <summary>
        ///     Called whenever the selected map changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChange(object sender, BindableValueChangedEventArgs<Map> e)
        {
            Source.Cancel();
            Source.Dispose();
            Source = new CancellationTokenSource();
            ThreadScheduler.Run(() => LoadScores(Source.Token).Start());
        }
    }
}