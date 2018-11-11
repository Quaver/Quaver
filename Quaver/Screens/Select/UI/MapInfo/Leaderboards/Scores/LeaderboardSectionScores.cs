using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Select.UI.MapInfo.Leaderboards.Scores
{
    public abstract class LeaderboardSectionScores : LeaderboardSection
    {
        /// <summary>
        ///     The current leaderboard scores displayed.
        /// </summary>
        public List<LeaderboardScore> LeaderboardScores { get; }

        /// <summary>
        ///     If there are no scores available on the map.
        /// </summary>
        private SpriteText NoScoresSubmittedText { get; set; }

        /// <summary>
        ///     The direction we're animating the no scores text.
        /// </summary>
        private Direction NoScoresSubmittedAnimationDirection { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="sectionType"></param>
        /// <param name="leaderboard"></param>
        /// <param name="name"></param>
        protected LeaderboardSectionScores(LeaderboardSectionType sectionType, Leaderboard leaderboard, string name)
            : base(sectionType, leaderboard, name)
        {
            LeaderboardScores = new List<LeaderboardScore>();

            ScrollContainer.EasingType = Easing.OutQuint;
            ScrollContainer.TimeToCompleteScroll = 1500;
            ScrollContainer.Scrollbar.Tint = Color.White;
            ScrollContainer.Scrollbar.Width = 3;
        }

        /// <summary>
        ///     Fetches scores to use for the leaderboard.
        /// </summary>
        /// <returns></returns>
        protected abstract List<LocalScore> FetchScores();

        /// <inheritdoc />
        /// <summary>
        ///     Updates the section
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (NoScoresSubmittedText != null)
                AnimateNoScoresSubmittedText(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the drawables for leaderboard scores.
        /// </summary>
        /// <returns></returns>
        private void CreateLeaderboardScores(IReadOnlyList<LocalScore> scores)
        {
            if (scores.Count > 0)
            {
                for (var i = 0; i < scores.Count; i++)
                {
                    var score = new LeaderboardScore(this, scores[i], i + 1);
                    score.Y = i * score.Height + i * 5;

                    score.X = -score.Width;

                    var t = new Animation(AnimationProperty.X, Easing.OutQuint, score.X, 0, 600 + 90 * i);
                    score.Animations.Add(t);

                    LeaderboardScores.Add(score);
                }
            }

            // Snap all the way up to the top of the scroll container.
            ScrollContainer.ScrollTo(0, 1);

            // If there are more than 7 scores (only 7 can be displayed at a time),
            // Then calculate the actual size of the scroll container.
            if (LeaderboardScores.Count > 6)
            {
                ScrollContainer.Scrollbar.Visible = true;
                ScrollContainer.ContentContainer.Height = scores.Count * (LeaderboardScores.First().Height + 5);
                return;
            }

            // In the event that there aren't more than 7 scores, we don't need scrolling,
            // So to reset it, set the ContentContainer's side back to the original.
            // the overall container (No need for scrolling)
            ScrollContainer.ContentContainer.Size = ScrollContainer.Size;
            ScrollContainer.Scrollbar.Visible = false;

            // Display text stating that there are no scores available.
            if (LeaderboardScores.Count == 0)
                CreateNoScoresAvailableText();
        }

        /// <summary>
        ///     Clears the leaderboard.
        /// </summary>
        private void ClearLeaderboard()
        {
            NoScoresSubmittedText?.Destroy();
            NoScoresSubmittedText = null;
            LeaderboardScores.ForEach(x => x.Destroy());
            LeaderboardScores.Clear();
        }

        /// <summary>
        ///     Fetches the scores and updates the leaderboards.
        /// </summary>
        public void FetchAndUpdateLeaderboards(List<LocalScore> scores)
        {
            ClearLeaderboard();

            // ReSharper disable once ArrangeMethodOrOperatorBody
            Scheduler.RunThread(() =>
            {
                // If we already have scores cached, then use them.
                if (scores != null && scores.Count != 0)
                {
                    CreateLeaderboardScores(scores);
                    return;
                }

                // Grab the map before fetching the scores, so we can know if to update it or not.
                var mapBeforeFetching = MapManager.Selected.Value;
                mapBeforeFetching.Scores.Value = FetchScores();

                // If the map is still the same after fetching, we'll want to create leaderboard scores.
                if (MapManager.Selected.Value == mapBeforeFetching)
                    CreateLeaderboardScores(mapBeforeFetching.Scores.Value);
                else
                    mapBeforeFetching.ClearScores();
            });
        }

        /// <summary>
        ///     Creates the text that displays if there are no scores available.
        /// </summary>
        private void CreateNoScoresAvailableText()
        {
            string text;

            switch (SectionType)
            {
                case LeaderboardSectionType.Local:
                    text = "No local scores available for this map. Start playing!";
                    break;
                case LeaderboardSectionType.Global:
                    text = "Not implemented yet, check back later. Sorry!";
                    break;
                default:
                    text = "";
                    break;
            }

            NoScoresSubmittedText = new SpriteText(BitmapFonts.Exo2Regular, text, 16)
            {
                Tint = Color.White,
                Alignment = Alignment.MidCenter,
                Visible = false,
            };

            NoScoresSubmittedAnimationDirection = Direction.Forward;
            ScrollContainer.AddContainedDrawable(NoScoresSubmittedText);
        }

        /// <summary>
        ///     Adds a "pulsing" animation to the no scores submitted text.
        /// </summary>
        private void AnimateNoScoresSubmittedText(GameTime gameTime)
        {
            NoScoresSubmittedText.Visible = true;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            switch (NoScoresSubmittedAnimationDirection)
            {
                case Direction.Forward:
                    break;
                case Direction.Backward:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NoScoresSubmittedText.Y = NoScoresSubmittedText.Height;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void HandleInput() => HandleScrollingInput();
    }
}