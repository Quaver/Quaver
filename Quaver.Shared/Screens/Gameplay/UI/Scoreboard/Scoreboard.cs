/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal Scoreboard(ScoreboardType type, IEnumerable<ScoreboardUser> users, MultiplayerTeam team = MultiplayerTeam.Red)
        {
            Type = type;
            Team = team;

            if (OnlineManager.CurrentGame != null)
            {
                OnlineManager.Client.OnGameJudgements += OnGameJudgements;
                OnlineManager.Client.OnPlayerBattleRoyaleEliminated += OnPlayerBattleRoyaleEliminated;

                if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Battle_Royale)
                {
                    BattleRoyalePlayersLeft = new Bindable<int>(0)
                    {
                        Value = MapManager.Selected.Value.Scores.Value.Count + 1
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
            else if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Free_For_All &&
                     MapManager.Selected.Value.Scores.Value.Count == 1)
            {
                OneVsOneWinsBanner = new ScoreboardOneVsOneWins(this)
                {
                    Parent = this,
                    Y = 235
                };
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

            // Tween to target Y positions
            Users.ForEach(user =>
            {
                user.Y = MathHelper.Lerp(user.Y, user.TargetYPosition, (float) Math.Min(dt / 120, 1));

                // Tween X Position based on if the scoreboard is hidden
                if (ConfigManager.ScoreboardVisible.Value)
                {
                    var target = Team == MultiplayerTeam.Red ? 0 : WindowManager.Width - user.Width;
                    user.X = MathHelper.Lerp(user.X, target, (float) Math.Min(dt / 120, 1));
                }
                else
                {
                    var target = Team == MultiplayerTeam.Red ? -user.Width - 10 : WindowManager.Width + user.Width + 10;
                    user.X = MathHelper.Lerp(user.X, target, (float) Math.Min(dt / 90, 1));
                }

                user.Visible = user.X >= -user.Width + 10;
            });

            // Lerp team banner in and out
            if (TeamBanner != null)
            {
                var target = Team == MultiplayerTeam.Red ? 0 : WindowManager.Width - TeamBanner.Width;
                TeamBanner.X = MathHelper.Lerp(TeamBanner.X, target, (float) Math.Min(dt / 120, 1));
            }

            base.Update(gameTime);
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
               users = Users
                    .OrderBy(x => x.HasQuit)
                    .ThenBy(x => x.Processor.MultiplayerProcessor.IsEliminated)
                    .ThenBy(x => x.Processor.MultiplayerProcessor.IsRegeneratingHealth)
                    .ThenByDescending(x => x.RatingProcessor.CalculateRating(x.Processor.Accuracy))
                    .ThenByDescending(x => x.Processor.Accuracy)
                    .ToList();
            }
            else
            {
                users = Users
                    .OrderBy(x => x.Processor.Health <= 0)
                    .ThenByDescending(x => x.RatingProcessor.CalculateRating(x.Processor.Accuracy))
                    .ThenByDescending(x => x.Processor.Accuracy)
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
                    users[i].TargetYPosition = (Type == ScoreboardType.FreeForAll && MapManager.Selected.Value.Scores.Value.Count != 1)
                                               &&  OnlineManager.CurrentGame?.Ruleset != MultiplayerGameRuleset.Battle_Royale
                        ? Math.Min(users.Count, 5) * -users[i].Height / 2f
                        : 4 * -users[i].Height / 2f + 14;

                    continue;
                }

                users[i].TargetYPosition = users[i - 1].TargetYPosition + users[i].Height + 4;
            }
        }
    }
}
