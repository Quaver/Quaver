using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection.UI.Leaderboard.Components;
using SQLite;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public class MultiplayerPlayer : MultiplayerSlot
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        private Drawable Container { get; }

        /// <summary>
        /// </summary>
        public User User { get; }

        /// <summary>
        /// </summary>
        private ContainedButton Button { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private List<DrawableModifier> Modifiers { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Ready { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus WinCount { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HostCrown { get; set; }

        private bool AvatarRequested { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerPlayer(Bindable<MultiplayerGame> game, Drawable container, User user)
        {
            Game = game;
            Container = container;
            User = user;

            CreateButton();
            CreateAvatar();
            CreateFlag();
            CreateUsername();
            CreateReadyIcon();
            CreateWinCount();
            CreateHostCrown();
            UpdateModifiers();
            AddBorder(ColorHelper.HexToColor("#0587E5"), 2);

            if (OnlineManager.Client != null)
            {
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
                OnlineManager.Client.OnGameSetReferee += OnGameSetReferee;
                OnlineManager.Client.OnGamePlayerTeamChanged += OnTeamChanged;
                SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;
            }

            // Always request updated user stats
            OnlineManager.Client?.RequestUserStats(new List<int>{ User.OnlineUser.Id });
            UpdateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;

            Button.Alpha = Button.IsHovered ? 0.35f : 0;

            if (game?.CurrentScreen?.ActiveRightClickOptions != null && game.CurrentScreen.ActiveRightClickOptions.Visible)
                Button.Depth = 1;
            else
                Button.Depth = 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (OnlineManager.Client != null)
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
                OnlineManager.Client.OnGameSetReferee -= OnGameSetReferee;
                OnlineManager.Client.OnGamePlayerTeamChanged -= OnTeamChanged;
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        public void UpdateContent() => AddScheduledUpdate(() =>
        {
            // Update username & flag
            if (User.HasUserInfo)
            {
                Username.Text = User.OnlineUser.Username;
                Flag.Image = Flags.Get(User.OnlineUser.CountryFlag ?? "XX");
            }
            else
            {
                // Request the user's information from the server
                OnlineManager.Client?.RequestUserInfo(new List<int>{ User.OnlineUser.Id });
                OnlineManager.Client?.RequestUserStats(new List<int>{ User.OnlineUser.Id });

                Username.Text = "Loading...";
                Flag.Image = Flags.Get("XX");
            }

            if (User.Stats.ContainsKey((GameMode) Game.Value.GameMode))
                Username.Text += $" (#{User.Stats[(GameMode) Game.Value.GameMode].Rank})";

            // Update host crown visibility
            HostCrown.Visible = Game.Value.HostId == User.OnlineUser.Id;
            HostCrown.Position = new ScalableVector2(Username.X + Username.Width + 8, Username.Y + 4);

            // Update ready status
            Ready.Image = Game.Value.PlayersReady.Contains(User.OnlineUser.Id) ? UserInterface.ReadyIcon : UserInterface.NotReadyIcon;
            Ready.Tint = Color.White;

            // If the user doesn't have the map, change the ready status icon to an X.
            if (Game.Value.PlayersWithoutMap.Contains(User.OnlineUser.Id))
            {
                Ready.Image = FontAwesome.Get(FontAwesomeIcon.fa_times);
                Ready.Tint = Color.Crimson;
            }

            // Update player win count
            int wins;

            if (Game.Value.Ruleset == MultiplayerGameRuleset.Team)
                wins = OnlineManager.GetTeam(User.OnlineUser.Id) == MultiplayerTeam.Red ? Game.Value.RedTeamWins : Game.Value.BlueTeamWins;
            else
                wins = Game.Value.PlayerWins.Find(x => x.UserId == User.OnlineUser.Id)?.Wins ?? 0;

            WinCount.Text = Game.Value.RefereeUserId == User.OnlineUser.Id ? "Referee" : $"{wins} W";

            // Referee Color
            if (Game.Value.RefereeUserId == User.OnlineUser.Id)
                Border.Tint = Color.White;
            // Team Color
            else if (Game.Value.Ruleset == MultiplayerGameRuleset.Team)
            {
                if (OnlineManager.GetTeam(User.OnlineUser.Id, Game.Value) == MultiplayerTeam.Red)
                    Border.Tint = Color.Crimson;
                else
                    Border.Tint = ColorHelper.HexToColor("#0587E5");
            }
            // FFA Color
            else
            {
                if (Game.Value.HostId == User.OnlineUser.Id)
                {
                    Border.Tint = ColorHelper.HexToColor("#E8B636");
                }
                else if (Game.Value.PlayersWithoutMap.Contains(User.OnlineUser.Id))
                {
                    Border.Tint = ColorHelper.HexToColor("#F8645D");
                }
                else if (Game.Value.PlayersReady.Contains(User.OnlineUser.Id))
                {
                    Border.Tint = ColorHelper.HexToColor("#27AF6E");
                }
                else
                {
                    Border.Tint = ColorHelper.HexToColor("#0587E5");
                }
            }

            UpdateModifiers();

            Avatar.Border.Tint = Border.Tint;
            Avatar.Border.Tint = new Color(Avatar.Border.Tint.R / 2, Avatar.Border.Tint.G / 2,
                Avatar.Border.Tint.B / 2);

            if (SteamManager.UserAvatars != null && SteamManager.UserAvatars.ContainsKey((ulong) User.OnlineUser.SteamId))
                Avatar.Image = SteamManager.UserAvatars[(ulong) User.OnlineUser.SteamId];
            else
            {
                Avatar.Image = UserInterface.UnknownAvatar;

                if (!AvatarRequested && User.HasUserInfo)
                {
                    SteamManager.SendAvatarRetrievalRequest((ulong) User.OnlineUser.SteamId);
                    AvatarRequested = true;
                }
            }
        });

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ContainedButton(Container, UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(Width - 4, Height - 4),
                Alpha = 0
            };

            Button.Clicked += OpenRightClickOptions;
            Button.RightClicked += OpenRightClickOptions;
        }

        /// <summary>
        ///     Creates <see cref="Avatar"/>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 12,
                Size = new ScalableVector2(Height * 0.65f, Height * 0.65f),
                Image = UserInterface.UnknownAvatar,
                UsePreviousSpriteBatchOptions = true
            };

            Avatar.AddBorder(Color.White, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateFlag() => Flag = new Sprite()
        {
            Parent = this,
            X = Avatar.X + Avatar.Width + 10,
            Y = 8,
            Image = Flags.Get("XX"),
            Size = new ScalableVector2(22, 22),
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        private void CreateUsername() => Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "Loading...", 21)
        {
            Parent = Flag,
            Alignment = Alignment.MidLeft,
            X = Flag.Width + 8,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        private void CreateReadyIcon() => Ready = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            UsePreviousSpriteBatchOptions = true,
            X = -Avatar.X,
            Size = new ScalableVector2(20, 20),
            Image = UserInterface.ReadyIcon
        };

        /// <summary>
        /// </summary>
        private void CreateWinCount() => WinCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "", 20)
        {
            Parent = this,
            Alignment = Alignment.MidRight,
            X = Ready.X - Ready.Width - 12,
            UsePreviousSpriteBatchOptions = true,
            Tint = ColorHelper.HexToColor("#FFE76B")
        };

        /// <summary>
        /// </summary>
        private void CreateHostCrown() => HostCrown = new Sprite
        {
            Parent = Flag,
            Alignment = Alignment.TopLeft,
            Size = new ScalableVector2(14, 14),
            Position = new ScalableVector2(-4, 0),
            UsePreviousSpriteBatchOptions = true,
            Image = UserInterface.HostCrown
        };

        /// <summary>
        ///     Creates and updates <see cref="Modifiers"/>
        /// </summary>
        private void UpdateModifiers()
        {
            Modifiers?.ForEach(x => x.Destroy());
            Modifiers?.Clear();

            Modifiers = new List<DrawableModifier>();

            var modsList = ModManager.GetModsList(OnlineManager.GetUserActivatedMods(User.OnlineUser.Id, Game.Value));

            if (modsList.Count == 0)
                modsList.Add(ModIdentifier.None);

            for (var i = 0; i < modsList.Count; i++)
            {
                const int width = 52;
                const int height = 26;
                const float scale = 0.88f;

                var mod = new DrawableModifier(modsList[i])
                {
                    Parent = this,
                    Alignment = Alignment.BotLeft,
                    X = Flag.X - 6 + (width - 6) * Modifiers.Count,
                    Y = -Flag.Y,
                    UsePreviousSpriteBatchOptions = true,
                    Size = new ScalableVector2(width * scale, height * scale),
                    Alpha = 1
                };

                Modifiers.Add(mod);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenRightClickOptions(object sender, EventArgs e)
        {
            var game = GameBase.Game as QuaverGame;
            game?.CurrentScreen?.ActivateRightClickOptions(new DrawableOnlineUserRightClickOptions(User));
        }

        private void OnGameSetReferee(object sender, GameSetRefereeEventArgs e) => UpdateContent();

        private void OnGameChangedModifiers(object sender, ChangeModifiersEventArgs e) => UpdateContent();

        private void OnPlayerChangedModifiers(object sender, PlayerChangedModifiersEventArgs e) => UpdateContent();

        private void OnGameTeamWinCount(object sender, TeamWinCountEventArgs e) => UpdateContent();

        private void OnGamePlayerWinCount(object sender, PlayerWinCountEventArgs e) => UpdateContent();

        private void OnGamePlayerNotReady(object sender, PlayerNotReadyEventArgs e) => UpdateContent();

        private void OnUserStats(object sender, UserStatsEventArgs e) => UpdateContent();

        private void OnGamePlayerReady(object sender, PlayerReadyEventArgs e) => UpdateContent();

        private void OnGamePlayerHasMap(object sender, GamePlayerHasMapEventArgs e) => UpdateContent();

        private void OnGamePlayerNoMap(object sender, PlayerGameNoMapEventArgs e) => UpdateContent();

        private void OnGameMapChanged(object sender, GameMapChangedEventArgs e) => UpdateContent();

        private void OnGameHostChanged(object sender, GameHostChangedEventArgs e) => UpdateContent();

        private void OnUserInfoReceived(object sender, UserInfoEventArgs e) => UpdateContent();

        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e) => UpdateContent();

        private void OnTeamChanged(object sender, PlayerTeamChangedEventArgs e) => UpdateContent();
    }
}