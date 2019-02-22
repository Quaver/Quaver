/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Select.UI.Leaderboard
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
            new ScalableVector2(leaderboard.Width, leaderboard.Height),
            new ScalableVector2(leaderboard.Width, leaderboard.Height))
        {
            Leaderboard = leaderboard;
            Alpha = 0;
            Tint = Color.CornflowerBlue;

            InputEnabled = true;
            Scrollbar.Tint = ColorHelper.HexToColor("#212121");
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
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0;
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
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     Fetches scores to display on this section.
        /// </summary>
        public abstract FetchedScoreStore FetchScores();

        /// <summary>
        ///     Gets a string that is displayed when no scores are available.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public abstract string GetNoScoresAvailableString(Map map);

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
        public void UpdateWithScores(Map map, FetchedScoreStore scoreStore, CancellationToken cancellationToken = default)
        {
            var newScores = new List<DrawableLeaderboardScore>();

            try
            {
                if (map != MapManager.Selected.Value)
                    return;

                cancellationToken.ThrowIfCancellationRequested();

                var scoreCount = scoreStore.PersonalBest != null ? scoreStore.Scores.Count + 1 : scoreStore.Scores.Count;

                // Calculate the height of the scroll container based on how many scores there are.
                var totalUserHeight =  scoreCount * DrawableLeaderboardScore.HEIGHT + 10 * (scoreCount - 1);

                if (totalUserHeight > Height)
                    ContentContainer.Height = totalUserHeight;
                else
                    ContentContainer.Height = Height;

                for (var i = 0; i < scoreCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Score score;

                    if (scoreStore.PersonalBest != null)
                        score = i == 0 ? scoreStore.PersonalBest : scoreStore.Scores[i - 1];
                    else
                        score = scoreStore.Scores[i];

                    var isPersonalBest = scoreStore.PersonalBest != null && i == 0;
                    var rank = scoreStore.PersonalBest != null ? (isPersonalBest ? -1 : i) : i + 1;

                    var drawable = new DrawableLeaderboardScore(score, rank)
                    {
                        Parent = this,
                        Y = i * DrawableLeaderboardScore.HEIGHT + i * 10,
                        X = -DrawableLeaderboardScore.WIDTH,
                    };

                    drawable.MoveToX(0, Easing.OutQuint, 300 + i * 50);
                    newScores.Add(drawable);
                    Scores.Add(drawable);
                    AddContainedDrawable(drawable);
                }

                // This is a hack... It's to place the leaderboard selector on top so that the
                // buttons are technically on top of the leaderboard score ones.
                Leaderboard.View.LeaderboardSelector.Parent = Leaderboard.View.Container;
            }
            catch (Exception e)
            {
                newScores.ForEach(x =>
                {
                    x.Parent = null;
                    x.Destroy();
                });

                newScores = null;
            }
        }
    }
}
