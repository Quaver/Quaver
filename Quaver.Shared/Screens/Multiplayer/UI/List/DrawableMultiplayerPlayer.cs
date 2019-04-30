using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI.List
{
    public class DrawableMultiplayerPlayer : PoolableSprite<OnlineUser>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 48;

        /// <summary>
        /// </summary>
        private DrawableMultiplayerPlayerButton Button { get; }

        /// <summary>
        /// </summary>
        public Sprite Avatar { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Username { get; }

        /// <summary>
        /// </summary>
        private Sprite HostCrown { get; }

        /// <summary>
        /// </summary>
        public Sprite Ready { get; }

        /// <summary>
        /// </summary>
        private Sprite NoMapIcon { get; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableMultiplayerPlayer(PoolableScrollContainer<OnlineUser> container, OnlineUser item, int index) : base(container, item, index)
        {
            Alpha = 0f;
            Tint = Color.White;
            Size = new ScalableVector2(container.Width, HEIGHT);

            Button = new DrawableMultiplayerPlayerButton(this) {Parent = this};

            Avatar = new Sprite
            {
                Parent = Button,
                Size = new ScalableVector2(Button.Height, Button.Height),
                Alpha = 0
            };

            Avatar.AddBorder(GetPlayerColor(), 2);

            Flag = new Sprite()
            {
                Parent = Button,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 12,
                Image = item.CountryFlag == null ? Flags.Get("XX") : Flags.Get(item.CountryFlag),
                Size = new ScalableVector2(26, 26)
            };

            Username = new SpriteTextBitmap(FontsBitmap.GothamRegular, string.IsNullOrEmpty(item.Username) ? $"Loading..." : item.Username)
            {
                Parent = Flag,
                Alignment = Alignment.MidLeft,
                X = Flag.Width + 6,
                FontSize = 16
            };

            HostCrown = new Sprite
            {
                Parent = Username,
                Alignment = Alignment.MidLeft,
                X = Username.Width + 12,
                Y = 1,
                Size = new ScalableVector2(16, 16),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_vintage_key_outline),
                SpriteEffect = SpriteEffects.FlipHorizontally,
                Tint = Color.Gold
            };

            Ready = new Sprite
            {
                Parent = Button,
                Alignment = Alignment.MidRight,
                X = -10,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_check_mark),
                Size = new ScalableVector2(18, 18),
                Alpha = OnlineManager.CurrentGame.PlayersReady.Contains(Item.Id) ? 1 :  0.35f,
                Tint = Color.LimeGreen
            };

            NoMapIcon = new Sprite()
            {
                Parent = Button,
                Alignment = Alignment.MidRight,
                X = Ready.X - Ready.Width - 10,
                Size = Ready.Size,
                Tint = Color.Red,
                Visible = OnlineManager.CurrentGame.PlayersWithoutMap.Contains(Item.Id),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_times)
            };

            OnlineManager.Client.OnUserInfoReceived += OnUserInfoReceived;
            OnlineManager.Client.OnGameHostChanged += OnGameHostChanged;
            OnlineManager.Client.OnGameMapChanged += OnGameMapChanged;
            OnlineManager.Client.OnGamePlayerNoMap += OnGamePlayerNoMap;
            OnlineManager.Client.OnGamePlayerHasMap += OnGamePlayerHasMap;
            OnlineManager.Client.OnPlayerReady += OnGamePlayerReady;
            OnlineManager.Client.OnPlayerNotReady += OnGamePlayerNotReady;
            OnlineManager.Client.OnUserStats += OnUserStats;
            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;

            if (!OnlineManager.OnlineUsers[item.Id].HasUserInfo)
                OnlineManager.Client.RequestUserInfo(new List<int> { item.Id });
            else
            {
                if (SteamManager.UserAvatars.ContainsKey((ulong) Item.SteamId))
                {
                    Avatar.Image = SteamManager.UserAvatars[(ulong) Item.SteamId];
                    Avatar.Alpha = 1;
                }
            }

            // Request user stats if necessary
            if (OnlineManager.OnlineUsers[item.Id].Stats.Count == 0)
                OnlineManager.Client.RequestUserStats(new List<int> { item.Id});
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(OnlineUser item, int index)
        {
            if (OnlineManager.OnlineUsers[item.Id].HasUserInfo)
            {
                var stats = OnlineManager.OnlineUsers[Item.Id].Stats;
                var rank = stats.ContainsKey((GameMode) OnlineManager.CurrentGame.GameMode) ? $" (#{stats[(GameMode) OnlineManager.CurrentGame.GameMode].Rank})" : "";

                Username.Text = item.Username + rank;
                HostCrown.X = Username.Width + 12;
            }

            HostCrown.Visible = OnlineManager.CurrentGame.HostId == Item.Id;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;
            OnlineManager.Client.OnUserInfoReceived -= OnUserInfoReceived;
            OnlineManager.Client.OnGameHostChanged -= OnGameHostChanged;
            OnlineManager.Client.OnGameMapChanged -= OnGameMapChanged;
            OnlineManager.Client.OnGamePlayerNoMap -= OnGamePlayerNoMap;
            OnlineManager.Client.OnGamePlayerHasMap -= OnGamePlayerHasMap;
            OnlineManager.Client.OnPlayerReady -= OnGamePlayerReady;
            OnlineManager.Client.OnUserStats -= OnUserStats;
            OnlineManager.Client.OnPlayerNotReady -= OnGamePlayerNotReady;

            base.Destroy();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserInfoReceived(object sender, UserInfoEventArgs e)
        {
            var user = e.Users.Find(x => x.Id == Item.Id);

            if (user == null)
                return;

            Item = user;
            Flag.Image = Flags.Get(user.CountryFlag);

            SteamManager.SendAvatarRetrievalRequest((ulong) user.SteamId);
            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong) Item.SteamId)
                return;

            Avatar.Image = e.Texture;
            Avatar.FadeTo(1, Easing.Linear, 400);
        }

        private void OnUserStats(object sender, UserStatsEventArgs e)
            => UpdateContent(Item, Index);

        /// <summary>
        ///     Called when the map has changed. Everyone has their no map icon cleared, as we
        ///     will wait for a new update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameMapChanged(object sender, GameMapChangedEventArgs e)
        {
            NoMapIcon.Visible = false;
            UpdateContent(Item, Index);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameHostChanged(object sender, GameHostChangedEventArgs e)
            => HostCrown.Visible = e.UserId == Item.Id;

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerNoMap(object sender, PlayerGameNoMapEventArgs e)
        {
            if (e.UserId != Item.Id)
                return;

            if (!OnlineManager.CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                OnlineManager.CurrentGame.PlayersWithoutMap.Add(e.UserId);

            NoMapIcon.Visible = true;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerHasMap(object sender, GamePlayerHasMapEventArgs e)
        {
            if (e.UserId != Item.Id)
                return;

            if (OnlineManager.CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                OnlineManager.CurrentGame.PlayersWithoutMap.Remove(e.UserId);

            NoMapIcon.Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerReady(object sender, PlayerReadyEventArgs e)
        {
            if (e.UserId != Item.Id)
                return;

            if (!OnlineManager.CurrentGame.PlayersReady.Contains(e.UserId))
                OnlineManager.CurrentGame.PlayersReady.Add(e.UserId);

            Ready.Alpha = 1;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerNotReady(object sender, PlayerNotReadyEventArgs e)
        {
            if (e.UserId != Item.Id)
                return;

            if (OnlineManager.CurrentGame.PlayersReady.Contains(e.UserId))
                OnlineManager.CurrentGame.PlayersReady.Remove(e.UserId);

            Ready.Alpha = 0.35f;
        }

        public Color GetPlayerColor()
        {
            if (OnlineManager.CurrentGame.RedTeamPlayers.Contains(Item.Id))
                return Color.Crimson;
            if (OnlineManager.CurrentGame.BlueTeamPlayers.Contains(Item.Id))
                return ColorHelper.HexToColor($"#4cb0f7");

            return Color.White;
        }
    }
}