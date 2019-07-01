﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Multiplayer.UI.List
{
    public class PlayerList : PoolableScrollContainer<OnlineUser>
    {
        public PlayerList(List<OnlineUser> availableItems, int poolSize, int poolStartingIndex, ScalableVector2 size,
            ScalableVector2 contentSize, bool startFromBottom = false)
            : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
        {
            Alpha = 0;
            Scrollbar.Tint = ColorHelper.HexToColor("#eeeeee");
            Scrollbar.Width = 6;
            Scrollbar.X = 14;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            OnlineManager.Client.OnGameMapChanged += OnGameMapChanged;
            OnlineManager.Client.OnGamePlayerTeamChanged += OnGamePlayerTeamChanged;
            OnlineManager.Client.OnGameRulesetChanged += OnGameRulesetChanged;
            OnlineManager.Client.OnGameSetReferee += OnGameSetReferee;
            CreatePool();

            if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                OrderByTeam(true);
            else
                OrderFreeForAllPlayers();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                           && DialogManager.Dialogs.Count == 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<OnlineUser> CreateObject(OnlineUser item, int index) =>
            new DrawableMultiplayerPlayer(this, item, index);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());

            OnlineManager.Client.OnGameMapChanged -= OnGameMapChanged;
            OnlineManager.Client.OnGamePlayerTeamChanged -= OnGamePlayerTeamChanged;
            OnlineManager.Client.OnGameRulesetChanged -= OnGameRulesetChanged;
            OnlineManager.Client.OnGameSetReferee -= OnGameSetReferee;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        public void AddOrUpdatePlayer(OnlineUser p)
        {
            lock (Pool)
            {
                var index = Pool.FindIndex(x => x.Item.Id == p.Id);

                if (index == -1)
                {
                    AddObjectToBottom(p, false, true);
                    OrderFreeForAllPlayers();
                    return;
                }

                Pool[index].UpdateContent(p, index);
            }
        }

        /// <summary>
        ///     Public method to remove users from the game
        /// </summary>
        /// <param name="u"></param>
        public void RemovePlayer(OnlineUser u) => RemoveObject(u);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        protected override void RemoveObject(OnlineUser obj)
        {
            var drawable = (DrawableMultiplayerPlayer) Pool.Find(x => x.Item.Id == obj.Id);

            lock (AvailableItems)
            lock (Pool)
            {
                AvailableItems.Remove(obj);
                AvailableItems.Remove(drawable.Item);

                drawable.Destroy();

                RemoveContainedDrawable(drawable);
                Pool.Remove(drawable);

                for (var i = 0; i < Pool.Count; i++)
                {
                    Pool[i].MoveToY((PoolStartingIndex + i) * drawable.HEIGHT, Easing.OutQuint, 600);
                    Pool[i].Index = i;
                }

                RecalculateContainerHeight(true);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameMapChanged(object sender, GameMapChangedEventArgs e)
        {
            OnlineManager.CurrentGame.PlayersReady.Clear();

            Pool.ForEach(x =>
            {
                var p = x as DrawableMultiplayerPlayer;
                p.Ready.Alpha = 0.35f;
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerTeamChanged(object sender, PlayerTeamChangedEventArgs e)
        {
            lock (Pool)
            {
                var player = Pool.Find(x => x.Item.Id == e.UserId) as DrawableMultiplayerPlayer;

                if (player == null)
                    return;

                player.Avatar.Border.Tint = player.GetPlayerColor();
                player.Button.Image = player.GetPlayerPanel();
                player.UpdateContent(player.Item, player.Index);
                OrderByTeam(false);
            }
        }

        /// <summary>
        /// </summary>
        private void OrderByTeam(bool orderInstantly)
        {
            Pool = Pool
                .OrderBy(x => OnlineManager.CurrentGame.RefereeUserId == x.Item.Id)
                .ThenBy(x => OnlineManager.GetTeam(x.Item.Id)).ToList();

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].Index = i;

                var targetY = (PoolStartingIndex + i) * Pool[i].HEIGHT;

                if (orderInstantly)
                    Pool[i].Y = targetY;
                else
                {
                    Pool[i].ClearAnimations();
                    Pool[i].MoveToY(targetY, Easing.OutQuint, 400);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameRulesetChanged(object sender, RulesetChangedEventArgs e)
        {
            switch (e.Ruleset)
            {
                case MultiplayerGameRuleset.Free_For_All:
                case MultiplayerGameRuleset.Battle_Royale:
                    OrderFreeForAllPlayers();
                    break;
                case MultiplayerGameRuleset.Team:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnGameSetReferee(object sender, GameSetRefereeEventArgs e)
        {
            if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                OrderByTeam(false);
            else
                OrderFreeForAllPlayers();
        }

        private void OrderFreeForAllPlayers()
        {
            Pool = Pool.OrderBy(x => x.Item.Id == OnlineManager.CurrentGame.RefereeUserId).ThenBy(x => x.Index).ToList();

            foreach (var item in Pool)
            {
                var index = OnlineManager.CurrentGame.PlayerIds.FindIndex(x => x == item.Item.Id);
                item.Index = index;

                var player = item as DrawableMultiplayerPlayer;
                player.Avatar.Border.Tint = player.GetPlayerColor();
                player.Button.Image = player.GetPlayerPanel();
            }

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].ClearAnimations();
                Pool[i].MoveToY((PoolStartingIndex + i) * Pool[i].HEIGHT, Easing.OutQuint, 400);
            }
        }
    }
}