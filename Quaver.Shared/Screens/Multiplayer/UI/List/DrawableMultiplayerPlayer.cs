using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Multiplayer.UI.List
{
    public class DrawableMultiplayerPlayer : PoolableSprite<OnlineUser>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 60;

        /// <summary>
        /// </summary>
        public DrawableMultiplayerPlayerButton Button { get; }

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

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Wins { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Mods { get; }

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

            Button = new DrawableMultiplayerPlayerButton(this)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };

            Ready = new Sprite
            {
                Parent = Button,
                Alignment = Alignment.MidLeft,
                X = 16,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_check_mark),
                Size = new ScalableVector2(18, 18),
                Alpha = OnlineManager.CurrentGame.PlayersReady.Contains(Item.Id) ? 1 :  0.35f,
                Tint = Color.LimeGreen,
                UsePreviousSpriteBatchOptions = true
            };

            Avatar = new Sprite
            {
                Parent = Button,
                Size = new ScalableVector2(36, 36),
                X = Ready.X + Ready.Width + 16,
                Alignment = Alignment.MidLeft,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            Avatar.AddBorder(GetPlayerColor(), 2);

            Flag = new Sprite()
            {
                Parent = Button,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 16,
                Image = item.CountryFlag == null ? Flags.Get("XX") : Flags.Get(item.CountryFlag),
                Size = new ScalableVector2(26, 26),
                UsePreviousSpriteBatchOptions = true
            };

            Username = new SpriteTextBitmap(FontsBitmap.GothamRegular, string.IsNullOrEmpty(item.Username) ? $"Loading..." : item.Username)
            {
                Parent = Flag,
                Alignment = Alignment.MidLeft,
                X = Flag.Width + 6,
                FontSize = 16,
                UsePreviousSpriteBatchOptions = true
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
                Tint = Color.Gold,
                UsePreviousSpriteBatchOptions = true
            };

            NoMapIcon = new Sprite()
            {
                Parent = Button,
                Alignment = Alignment.MidLeft,
                X = Ready.X,
                Size = Ready.Size,
                Tint = Color.Red,
                Visible = OnlineManager.CurrentGame.PlayersWithoutMap.Contains(Item.Id),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_times),
                UsePreviousSpriteBatchOptions = true
            };

            Wins = new SpriteTextBitmap(FontsBitmap.GothamRegular, "0 Wins")
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -16,
                FontSize = 16,
                Tint = Colors.SecondaryAccent,
                Y = -2,
                UsePreviousSpriteBatchOptions = true
            };

            Mods = new SpriteTextBitmap(FontsBitmap.GothamRegular, "")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Avatar.X + Avatar.Width + 16,
                FontSize = 14,
                Y = 8,
                UsePreviousSpriteBatchOptions = true,
                Tint = Colors.MainAccent
            };

            OnlineManager.Client.OnUserInfoReceived += OnUserInfoReceived;
            OnlineManager.Client.OnGameHostChanged += OnGameHostChanged;
            OnlineManager.Client.OnGameMapChanged += OnGameMapChanged;
            OnlineManager.Client.OnGamePlayerNoMap += OnGamePlayerNoMap;
            OnlineManager.Client.OnGamePlayerHasMap += OnGamePlayerHasMap;
            OnlineManager.Client.OnPlayerReady += OnGamePlayerReady;
            OnlineManager.Client.OnPlayerNotReady += OnGamePlayerNotReady;
            OnlineManager.Client.OnUserStats += OnUserStats;
            OnlineManager.Client.OnGamePlayerWinCount += OnGamePlayerWinCount;
            OnlineManager.Client.OnGameTeamWinCount += OnGameTeamWinCount;
            OnlineManager.Client.OnPlayerChangedModifiers += OnPlayerChangedModifiers;
            OnlineManager.Client.OnChangedModifiers += OnGameChangedModifiers;

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

        private void OnGameTeamWinCount(object sender, TeamWinCountEventArgs e)
            => UpdateContent(Item, Index);

        private void OnGameChangedModifiers(object sender, ChangeModifiersEventArgs e)
            => UpdateContent(Item, Index);

        public override void Update(GameTime gameTime)
        {
            Ready.Visible = !NoMapIcon.Visible;

            if (!Button.IsVisibleInContainer())
                return;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Button.IsVisibleInContainer())
                return;

            base.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(OnlineUser item, int index)
        {
            if (!OnlineManager.OnlineUsers.ContainsKey(item.Id))
            {
                Logger.Error($"{item.Id} not found in online users when trying to update content.", LogType.Network);
                return;
            }

            if (OnlineManager.OnlineUsers[item.Id].HasUserInfo)
            {
                var stats = OnlineManager.OnlineUsers[Item.Id].Stats;
                var rank = stats.ContainsKey((GameMode) OnlineManager.CurrentGame.GameMode) ? $" (#{stats[(GameMode) OnlineManager.CurrentGame.GameMode].Rank})" : "";

                Username.Text = item.Username + rank;
                HostCrown.X = Username.Width + 12;

                // Handle getting the amount of wins the player has
                if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                {
                    var team = OnlineManager.GetTeam(Item.Id);
                    int wins;

                    switch (team)
                    {
                        case MultiplayerTeam.Red:
                            wins = OnlineManager.CurrentGame.RedTeamWins;
                            break;
                        case MultiplayerTeam.Blue:
                            wins = OnlineManager.CurrentGame.BlueTeamWins;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Wins.Text = $"{wins} Wins";
                }
                else
                {
                    var mpWins = OnlineManager.CurrentGame.PlayerWins.Find(x => x.UserId == item.Id);
                    Wins.Text = mpWins != null ? $"{mpWins.Wins} Wins" : $"0 Wins";
                }

                var playerMods = OnlineManager.CurrentGame.PlayerMods.Find(x => x.UserId == item.Id);
                var mods = (ModIdentifier) long.Parse(OnlineManager.CurrentGame.Modifiers);

                if (mods == ModIdentifier.None)
                    mods = 0;

                if (playerMods != null)
                    mods |= (ModIdentifier) long.Parse(playerMods.Modifiers);

                Mods.Text = mods <= 0 ? "" : ModHelper.GetModsString(mods);
                Flag.Y = mods <= 0 ? 0 : -8;
            }

            Image = GetPlayerPanel();
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
            OnlineManager.Client.OnGamePlayerWinCount -= OnGamePlayerWinCount;
            OnlineManager.Client.OnGameTeamWinCount -= OnGameTeamWinCount;
            OnlineManager.Client.OnPlayerChangedModifiers -= OnPlayerChangedModifiers;
            OnlineManager.Client.OnChangedModifiers -= OnGameChangedModifiers;

            Button.Destroy();
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

        private void OnGamePlayerWinCount(object sender, PlayerWinCountEventArgs e)
            => UpdateContent(Item, Index);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerChangedModifiers(object sender, PlayerChangedModifiersEventArgs e)
            => UpdateContent(Item, Index);

        public Color GetPlayerColor()
        {
            if (OnlineManager.CurrentGame.RedTeamPlayers.Contains(Item.Id))
                return Color.Crimson;
            if (OnlineManager.CurrentGame.BlueTeamPlayers.Contains(Item.Id))
                return new Color(25, 104, 249);

            return Color.White;
        }

        public Texture2D GetPlayerPanel()
        {
            if (OnlineManager.CurrentGame.RedTeamPlayers.Contains(Item.Id))
                return UserInterface.UserPanelRed;
            if (OnlineManager.CurrentGame.BlueTeamPlayers.Contains(Item.Id))
                return UserInterface.UserPanelBlue;

            return UserInterface.UserPanelFFA;
        }
    }
}