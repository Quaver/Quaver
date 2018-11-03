using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Screens.SongSelect.UI.Leaderboard
{
    public abstract class LeaderboardScoreSection : ScrollContainer
    {
        /// <summary>
        ///     The type of leaderboard section this is.
        /// </summary>
        public abstract LeaderboardType Type { get; }

        /// <summary>
        ///     Reference to the parent leaderboard.
        /// </summary>
        protected LeaderboardContainer Leaderboard { get; }

        /// <summary>
        ///     Dictates if the section is currently fetching.
        /// </summary>
        public bool IsFetching { get; set; }

        /// <summary>
        ///     The wheel that displays the at the section is currently loading.
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        ///     The scores that are displayed.
        /// </summary>
        private List<DrawableLeaderboardScore> Scores { get; } = new List<DrawableLeaderboardScore>();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leaderboard"></param>
        protected LeaderboardScoreSection(LeaderboardContainer leaderboard) : base(
            new ScalableVector2(leaderboard.Width, leaderboard.Height - DrawableLeaderboardScore.HEIGHT - 24f),
            new ScalableVector2(leaderboard.Width, leaderboard.Height - DrawableLeaderboardScore.HEIGHT - 24f))
        {
            Leaderboard = leaderboard;
            Alpha = 0;
            Tint = Color.CornflowerBlue;

            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X += 10;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            CreateLoadingWheel();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleLoadingWheelAnimations();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the loading wheel that is displayed when looking for new scores.
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Image = UserInterface.LoadingWheel,
            Size = new ScalableVector2(50, 50),
            Visible = IsFetching,
            Tint = Color.White
        };

        /// <summary>
        ///     Animates the loading wheel.
        /// </summary>
        private void HandleLoadingWheelAnimations()
        {
            LoadingWheel.Visible = IsFetching;

            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     Fetches scores to display on this section.
        /// </summary>
        public abstract List<LocalScore> FetchScores();

        /// <summary>
        ///     Clears all the scores on the leaderboard.
        /// </summary>
        public void ClearScores()
        {
            Scores.ForEach(x => x.Destroy());
            Scores.Clear();
        }

        /// <summary>
        ///     Updates the leaderboard with new scores.
        /// </summary>
        public void UpdateWithScores(List<LocalScore> scores)
        {
            // Calculate the height of the scroll container based on how many scores there are.
            var totalUserHeight =  scores.Count * DrawableLeaderboardScore.HEIGHT + 10 * (scores.Count - 1);

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;

            for (var i = 0; i < scores.Count; i++)
            {
                var score = scores[i];

                var drawable = new DrawableLeaderboardScore(score, i + 1)
                {
                    Parent = this,
                    Y = i * DrawableLeaderboardScore.HEIGHT + i * 10,
                    X = -DrawableLeaderboardScore.WIDTH,
                };

                drawable.MoveToX(0, Easing.OutQuint, 300 + i * 50);
                Scores.Add(drawable);
                AddContainedDrawable(drawable);
            }
        }
    }
}