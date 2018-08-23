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
using Wobble.Graphics.Transformations;
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
        /// <param name="rankingSection"></param>
        /// <param name="leaderboard"></param>
        /// <param name="name"></param>
        protected LeaderboardSectionScores(LeaderboardRankingSection rankingSection, Leaderboard leaderboard, string name)
            : base(rankingSection, leaderboard, name)
        // ReSharper disable once ArrangeConstructorOrDestructorBody
        {
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            LeaderboardScores = new List<LeaderboardScore>();

            ScrollContainer.EasingType = Easing.EaseOutQuint;
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

            HandleInput();
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

                    var t = new Transformation(TransformationProperty.X, Easing.EaseOutQuint, score.X, 0, 600 + 90 * i);
                    score.Transformations.Add(t);

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

            // In the event that there aren't more than 5 scores, we don't need scrolling,
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
        public void FetchAndUpdateLeaderboards()
        {
            ClearLeaderboard();

            // ReSharper disable once ArrangeMethodOrOperatorBody
            Scheduler.RunThread(() =>
            {
                // If we already have scores cached, then use them.
                if (MapManager.Selected.Value.Scores.Value != null && MapManager.Selected.Value.Scores?.Value?.Count != 0)
                {
                    CreateLeaderboardScores(MapManager.Selected.Value.Scores.Value);
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

            switch (RankingSection)
            {
                case LeaderboardRankingSection.Local:
                    text = "No local scores available for this map. Start playing!";
                    break;
                case LeaderboardRankingSection.Global:
                    text = "Not implemented yet, check back later. Sorry!";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NoScoresSubmittedText = new SpriteText(Fonts.Exo2Regular24, text, 0.50f)
            {
                TextColor = Color.White,
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
                    NoScoresSubmittedText.TextScale = MathHelper.Lerp(NoScoresSubmittedText.TextScale, 0.55f, (float) Math.Min(dt / 240, 1));

                    if (Math.Abs(NoScoresSubmittedText.TextScale - 0.55f) < 0.01)
                        NoScoresSubmittedAnimationDirection = Direction.Backward;
                    break;
                case Direction.Backward:
                    NoScoresSubmittedText.TextScale = MathHelper.Lerp(NoScoresSubmittedText.TextScale, 0.50f, (float) Math.Min(dt / 240, 1));

                    if (Math.Abs(NoScoresSubmittedText.TextScale - 0.50f) < 0.01)
                        NoScoresSubmittedAnimationDirection = Direction.Forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            NoScoresSubmittedText.Y = NoScoresSubmittedText.MeasureString().Y / 2f;
        }

        /// <summary>
        ///     Handles scrolling input for the container.
        /// </summary>
        private void HandleInput()
        {
            var selectScreenView = (SelectScreenView) Leaderboard.Screen.View;

            // If the mouse is in the bounds of the scroll container, then
            // allow that to scroll, and turn off mapset scrolling.
            if (GraphicsHelper.RectangleContains(ScrollContainer.ScreenRectangle, MouseManager.CurrentState.Position))
            {
                ScrollContainer.InputEnabled = true;
                selectScreenView.MapsetContainer.InputEnabled = false;
            }
            // If the mouse is outside the bounds of the scroll container, then turn mapset scrolling back on.
            else
            {
                ScrollContainer.InputEnabled = false;
                selectScreenView.MapsetContainer.InputEnabled = true;
            }
        }
    }
}