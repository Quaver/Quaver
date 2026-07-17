/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Window;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class Scoreboard : Container
    {
        /// <summary>
        ///     The type of scoreboard this is
        /// </summary>
        public ScoreboardType Type { get; }

        /// <summary>
        ///     The team (if any) this scoreboard represents
        /// </summary>
        public MultiplayerTeam Team { get; }

        /// <summary>
        ///     The list of users on the scoreboard.
        /// </summary>
        public List<ScoreboardUser> Users { get; private set; }

        /// <summary>
        ///     Displays the banner for the scoreboard team
        /// </summary>
        public ScoreboardTeamBanner TeamBanner { get; }

        /// <summary>
        ///     Displays the players left/time left for battle royale
        /// </summary>
        public ScoreboardBattleRoyaleBanner BattleRoyaleBanner { get; }

        /// <summary>
        ///     Displays the win count in a 1v1
        /// </summary>
        public ScoreboardOneVsOneWins OneVsOneWinsBanner { get; }

        /// <summary>
        ///     The amount of players left in the battle royale game
        /// </summary>
        public Bindable<int> BattleRoyalePlayersLeft { get; }

        /// <summary>
        ///     Whether any user still needs to move to its current scoreboard position.
        /// </summary>
        private bool PositionUpdatePending { get; set; } = true;

        /// <summary>
        ///     The visibility state used for the last position update.
        /// </summary>
        private bool LastScoreboardVisibility { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal Scoreboard(ScoreboardType type, IEnumerable<ScoreboardUser> users, MultiplayerTeam team = MultiplayerTeam.Red)
        {
            Type = type;
            Team = team;
            Users = users?.ToList();

            if (OnlineManager.CurrentGame != null)
            {
                OnlineManager.Client.OnGameJudgements += OnGameJudgements;
                OnlineManager.Client.OnPlayerBattleRoyaleEliminated += OnPlayerBattleRoyaleEliminated;

                if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Battle_Royale)
                {
                    BattleRoyalePlayersLeft = new Bindable<int>(0)
                    {
                        Value = Users.Count + 1
                    };
                }
            }

            if (Type == ScoreboardType.Teams)
            {
                TeamBanner = new ScoreboardTeamBanner(this)
                {
                    Parent = this,
                    Y = 235,
                };

                if (Team == MultiplayerTeam.Blue)
                    TeamBanner.X = WindowManager.Width - TeamBanner.Width;
            }

            if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Battle_Royale)
            {
                BattleRoyaleBanner = new ScoreboardBattleRoyaleBanner(this)
                {
                    Parent = this,
                    Y = 235
                };
            }
            else if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Free_For_All && Users.Count <= 2)
            {
                /*OneVsOneWinsBanner = new ScoreboardOneVsOneWins(this)
                {
                    Parent = this,
                    Y = 235
                };*/
            }

            Users = users.OrderBy(x => x.Processor.Health <= 0).ThenByDescending(x => x.RatingProcessor.CalculateRating(x.Processor.Accuracy)).ToList();
            SetTargetYPositions();

            Users.ForEach(x =>
            {
                x.Scoreboard = this;
                x.Y = x.TargetYPosition;

                if (Team == MultiplayerTeam.Blue)
                    x.X = WindowManager.Width;
            });

            LastScoreboardVisibility = ConfigManager.ScoreboardVisible.Value;
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (OnlineManager.CurrentGame != null)
            {
                OnlineManager.Client.OnGameJudgements -= OnGameJudgements;
                OnlineManager.Client.OnPlayerBattleRoyaleEliminated -= OnPlayerBattleRoyaleEliminated;
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnGameJudgements(object sender, GameJudgementsEventArgs e)
        {
            if (OnlineManager.CurrentGame != null && OnlineManager.IsSpectatingSomeone)
                return;

            ScoreboardUser user = null;

            foreach (var u in Users)
            {
                if (u.LocalScore == null)
                    continue;

                if (u.LocalScore.PlayerId == e.UserId)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
                return;

            foreach (var t in e.Judgements)
            {
                user.Judgements.Add(t);
                user.CalculateScoreForNextObject(t == e.Judgements.Last());
            }

            SetTargetYPositions();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnPlayerBattleRoyaleEliminated(object sender, PlayerBattleRoyaleEliminatedEventArgs e)
        {
            ScoreboardUser user = null;

            foreach (var u in Users)
            {
                if (u.Type == ScoreboardUserType.Self && e.UserId == OnlineManager.Self.OnlineUser.Id)
                {
                    user = u;
                    SkinManager.Skin.SoundFailure.CreateChannel().Play();
                    break;
                }

                if (u.LocalScore != null && u.LocalScore.PlayerId == e.UserId)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
                return;

            user.Processor.MultiplayerProcessor.IsBattleRoyaleEliminated = true;
            BattleRoyalePlayersLeft.Value--;

            user.Rank = e.Rank;
            user.SetTintBasedOnHealth();
            SetTargetYPositions();

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen is GameplayScreen screen)
                screen.SetRichPresence();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            var scoreboardVisible = ConfigManager.ScoreboardVisible.Value;

            if (LastScoreboardVisibility != scoreboardVisible)
            {
                LastScoreboardVisibility = scoreboardVisible;
                PositionUpdatePending = true;
            }

            // Tween to target Y positions
            if (PositionUpdatePending)
            {
                PositionUpdatePending = false;

                foreach (var user in Users)
                {
                    user.Y = LerpToTarget(user.Y, user.TargetYPosition, dt / 120, out var yMoving);

                    // Tween X Position based on if the scoreboard is hidden
                    var target = scoreboardVisible
                        ? Team == MultiplayerTeam.Red ? 0 : WindowManager.Width - user.Width
                        : Team == MultiplayerTeam.Red ? -user.Width - 10 : WindowManager.Width + user.Width + 10;

                    user.X = LerpToTarget(user.X, target, dt / (scoreboardVisible ? 120 : 90), out var xMoving);
                    user.Visible = user.X >= -user.Width + 10;

                    PositionUpdatePending |= xMoving || yMoving;
                }
            }

            // Lerp team banner in and out
            if (TeamBanner != null)
            {
                var target = Team == MultiplayerTeam.Red ? 0 : WindowManager.Width - TeamBanner.Width;
                TeamBanner.X = MathHelper.Lerp(TeamBanner.X, target, (float) Math.Min(dt / 120, 1));
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     Lerps a position until it is close enough to settle exactly at its target.
        /// </summary>
        private static float LerpToTarget(float value, float target, double duration, out bool isMoving)
        {
            if (Math.Abs(value - target) <= 0.01f)
            {
                isMoving = false;
                return target;
            }

            var result = MathHelper.Lerp(value, target, (float)Math.Min(duration, 1));

            if (Math.Abs(result - target) <= 0.01f)
            {
                isMoving = false;
                return target;
            }

            isMoving = true;
            return result;
        }

        /// <summary>
        ///     Calculates scores for each user.
        /// </summary>
        internal void CalculateScores()
        {
            foreach (var x in Users)
            {
                if (OnlineManager.CurrentGame != null && x.Type != ScoreboardUserType.Self)
                    return;

                x.CalculateScoreForNextObject();
            }

            // Set each user's target position
            // Set Y positions.
            SetTargetYPositions();
        }

        /// <summary>
        ///     Sets the target y positions (where the scoreboard should move to)
        ///     Based on their rank.
        /// </summary>
        public void SetTargetYPositions()
        {
            if (Users.Count == 0)
                return;

            List<ScoreboardUser> users;

            if (Users.First().Processor.MultiplayerProcessor != null)
            {
               users = Users?
                    .OrderBy(x => x.HasQuit)
                    .ThenBy(x => x.Processor?.MultiplayerProcessor?.IsEliminated)
                    .ThenBy(x => x.Processor?.MultiplayerProcessor?.IsRegeneratingHealth)
                    .ThenByDescending(x => x?.CalculateRating())
                    .ThenByDescending(x => x?.Processor?.Accuracy)
                    .ToList();
            }
            else
            {
                users = Users?
                    .OrderBy(x => x.Processor?.Health <= 0)
                    .ThenByDescending(x => x?.CalculateRating())
                    .ThenByDescending(x => x?.Processor.Accuracy)
                    .ToList();
            }

            for (var i = 0; i < users.Count; i++)
            {
                if (OnlineManager.CurrentGame == null
                    || OnlineManager.CurrentGame.Ruleset != MultiplayerGameRuleset.Battle_Royale
                    || (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Battle_Royale &&
                        !users[i].Processor.MultiplayerProcessor.IsEliminated))
                {
                    users[i].Rank = i + 1;
                }
            }

            users = Users.OrderBy(x => x.Rank).ToList();

            for (var i = 0; i < users.Count; i++)
            {
                users[i].RankText.Text = users[i].Rank + ".";

                // Normalize the position of the first one so that all the rest will be completely in the middle.
                if (i == 0)
                {
                    try
                    {
                        SetTargetYPosition(users[i], (Type == ScoreboardType.FreeForAll && Users.Count != 1)
                                                   && OnlineManager.CurrentGame?.Ruleset != MultiplayerGameRuleset.Battle_Royale
                            ? Math.Min(users.Count, 5) * -users[i].Height / 2f
                            : 4 * -users[i].Height / 2f + 60);

                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    continue;
                }

                SetTargetYPosition(users[i], users[i - 1].TargetYPosition + users[i].Height + 4);
            }
        }

        /// <summary>
        ///     Updates a user's target position and schedules movement only when it changed.
        /// </summary>
        private void SetTargetYPosition(ScoreboardUser user, float target)
        {
            if (Math.Abs(user.TargetYPosition - target) > 0.01f)
                PositionUpdatePending = true;

            user.TargetYPosition = target;
        }
    }
}
