/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Result.UI
{
    public class ResultOnlineStats : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent Results Screen
        /// </summary>
        private ResultScreen Screen { get; }

        /// <summary>
        /// </summary>
        private ResultScoreContainer Container { get; }

        /// <summary>
        ///     The text that displays that the user is currently submitting their score
        /// </summary>
        private SpriteText TextSubmittingScore { get; set; }

        /// <summary>
        ///     The loading wheel that shows the user is submitting a score
        /// </summary>
        private Sprite SubmittingLoadingWheel { get; set; }

        /// <summary>
        ///     The list of online stats that are displayed after score submission
        /// </summary>
        private List<ResultKeyValueItem> Stats { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="container"></param>
        public ResultOnlineStats(ResultScreen screen, ResultScoreContainer container) : base(new ScalableVector2(0, 0), new ScalableVector2(0,0))
        {
            Screen = screen;
            Container = container;

            Size = new ScalableVector2(Container.Width - Container.Border.Thickness * 2,
                Container.Height - Container.BottomHorizontalDividerLine.Y - Container.Border.Thickness);

            ContentContainer.Size = Size;
            Alpha = 0;

            if (Screen.ResultsType == ResultScreenType.Gameplay)
                CreateStats();

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnScoreSubmitted += OnScoreSubmitted;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (SubmittingLoadingWheel != null)
                PerformLoadingWheelRotation();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (OnlineManager.Client != null)
                OnlineManager.Client.OnScoreSubmitted -= OnScoreSubmitted;

            base.Destroy();
        }

        /// <summary>
        ///     Creates all of the online statistics that are shown for this score.
        /// </summary>
        private void CreateStats()
        {
            if (Screen.Gameplay.InReplayMode || Screen.Gameplay.HasQuit)
                return;

            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    break;
                case ConnectionStatus.Connecting:
                case ConnectionStatus.Connected:
                case ConnectionStatus.Reconnecting:
                    CreateSubmittingText();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Creates the text that lets the user know the score is submitting.
        /// </summary>
        private void CreateSubmittingText()
        {
            // Scores aren't submitted if the user has all misses
            if (Screen.ScoreProcessor.TotalJudgementCount == Screen.ScoreProcessor.CurrentJudgements[Judgement.Miss])
                return;

            TextSubmittingScore = new SpriteText(Fonts.SourceSansProSemiBold, "SUBMITTING SCORE", 14)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
            };

            SubmittingLoadingWheel = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(TextSubmittingScore.Height, TextSubmittingScore.Height),
                Image = UserInterface.LoadingWheel,
                X = TextSubmittingScore.Width / 2f + 20
            };

            AddContainedDrawable(TextSubmittingScore);
            AddContainedDrawable(SubmittingLoadingWheel);
        }

        /// <summary>
        ///     Rotates the loading wheel endlessly
        /// </summary>
        private void PerformLoadingWheelRotation()
        {
            if (SubmittingLoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(SubmittingLoadingWheel.Rotation);
            SubmittingLoadingWheel.ClearAnimations();
            SubmittingLoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        /// <summary>
        ///     Called when a score has successfully submitted on the client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScoreSubmitted(object sender, ScoreSubmissionEventArgs e)
        {
            SubmittingLoadingWheel.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, SubmittingLoadingWheel.Alpha, 0, 100));
            TextSubmittingScore.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, TextSubmittingScore.Alpha, 0, 100));

            if (e.Response == null)
                return;

            switch (e.Response.Status)
            {
                case 200:
                    AddStatsAfterSubmission(e.Response);
                    break;
                case 400:
                    Logger.Warning($"Map unranked", LogType.Network);
                    break;
            }
        }

        /// <summary>
        ///     Adds all of the online stats after submitting the score
        /// </summary>
        /// <param name="score"></param>
        private void AddStatsAfterSubmission(ScoreSubmissionResponse score)
        {
            if (score.Map.Md5 != MapManager.Selected.Value.Md5Checksum)
                return;

            Stats = new List<ResultKeyValueItem>()
            {
                new ResultKeyValueItem(ResultKeyValueItemType.Horizontal, "MAP RANK:", score.Score.Rank == -1 ? "N/A" : $"#{score.Score.Rank:n0}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Horizontal, "GLOBAL RANK:", $"#{score.Stats.NewGlobalRank:n0}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Horizontal, "COUNTRY RANK:", $"#{score.Stats.NewCountryRank:n0}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Horizontal, "OVL. RATING:",
                    $"{score.Stats.OverallPerformanceRating:0.00}"),
                new ResultKeyValueItem(ResultKeyValueItemType.Horizontal, "OVL. ACCURACY:",
                    $"{StringHelper.AccuracyToString((float) score.Stats.OverallAccuracy)}"),
            };

            var totalWidth = 0f;

            for (var i = 0; i < Stats.Count; i++)
            {
                var item = Stats[i];
                item.Parent = this;
                item.Alignment = Alignment.MidLeft;
                totalWidth += item.Width;

                item.TextKey.Alpha = 0;
                item.TextValue.Alpha = 0;
                item.TextKey.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, item.TextKey.Alpha, 1, 300));
                item.TextValue.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, item.TextValue.Alpha, 1, 300));

                if (i == 0)
                    continue;

                var last = Stats[i - 1];
                item.Parent = last;
                item.X = last.Width + 50;
                totalWidth += 50;
            }

            Stats.First().X = Width / 2f - totalWidth / 2f;
        }
    }
}
