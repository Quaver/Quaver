/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Server.Client.Events;
using Quaver.Server.Client.Events.Disconnnection;
using Quaver.Server.Client.Events.Login;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Online.Username;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Lobby;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Joining;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Multiplayer;
using Quaver.Shared.Screens.Select;
using Steamworks;
using UniversalThreadManagement;
using Wobble;
using Wobble.Bindables;
using Wobble.Discord;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using static Quaver.Shared.Online.OnlineManager;

namespace Quaver.Shared.Online
{
    public static class OnlineManager
    {
        /// <summary>
        ///    The online client that connects to the Quaver servers.
        /// </summary>
        private static OnlineClient _client;
        public static OnlineClient Client
        {
            get => _client;
            private set
            {
                Self = null;
                OnlineUsers = new Dictionary<int, User>();
                MultiplayerGames = new Dictionary<string, MultiplayerGame>();
                SpectatorClients = new Dictionary<int, SpectatorClient>();
                Spectators = new Dictionary<int, User>();

                if (_client != null)
                    return;

                _client = value;
            }
        }

        /// <summary>
        ///     The current online connection status.
        /// </summary>
        public static Bindable<ConnectionStatus> Status { get; } = new Bindable<ConnectionStatus>(ConnectionStatus.Disconnected);

        /// <summary>
        ///     If we're currently connected to the server.
        /// </summary>
        public static bool Connected => Client?.Socket != null && Status.Value == ConnectionStatus.Connected;

        /// <summary>
        ///     The user client for self (the current client.)
        /// </summary>
        public static User Self { get; private set; }

        /// <summary>
        ///     Dictionary containing all of the currently online users.
        /// </summary>
        public static Dictionary<int, User> OnlineUsers { get; private set; }

        /// <summary>
        ///     Dictionary containing all the currently available multiplayer games.
        ///     game_hash:game
        /// </summary>
        public static Dictionary<string, MultiplayerGame> MultiplayerGames { get; private set; }

        /// <summary>
        ///     The current multiplayer game the player is in
        /// </summary>
        public static MultiplayerGame CurrentGame { get; private set; }

        /// <summary>
        ///     The players who the client is currently spectating
        ///
        ///     Note:
        ///         - Only 1 player is allowed if not running a tournament client
        ///         - Otherwise multiple are allowed.
        /// </summary>
        public static Dictionary<int, SpectatorClient> SpectatorClients { get; private set; }

        /// <summary>
        ///     Players who are currently spectating us
        /// </summary>
        public static Dictionary<int, User> Spectators { get; private set; }

        /// <summary>
        ///     If we're currently being spectated by another user
        /// </summary>
        public static bool IsBeingSpectated => Client != null && Status.Value == ConnectionStatus.Connected && Spectators.Count != 0;

        /// <summary>
        ///     If the client is currently spectating someone
        /// </summary>
        public static bool IsSpectatingSomeone => Client != null & Status.Value == ConnectionStatus.Connected && SpectatorClients.Count != 0;

        ///     If the current user is a donator
        /// </summary>
        public static bool IsDonator => Connected && Self.OnlineUser.UserGroups.HasFlag(UserGroups.Donator);

        /// <summary>
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            if (Status.Value != ConnectionStatus.Disconnected)
                return;

            Logger.Important($"Attempting to log into the Quaver server...", LogType.Network);

            if (!SteamManager.AuthSessionTicketValidated)
            {
                Logger.Error($"Could not log in because the steam auth session ticket was not validated.", LogType.Network);
                throw new Exception("Failed to login");
            }

            // Create the new online client and subscribe to all of its online events.
            if (Client == null)
            {
                Client = new OnlineClient();
                SubscribeToEvents();
            }

            // Initiate the connection to the game server.
            Client.Connect(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(),
                SteamManager.PTicket, SteamManager.PcbTicket, false);
        }

        /// <summary>
        ///     Subscribes to all events
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private static void SubscribeToEvents()
        {
            if (Client == null)
                throw new InvalidOperationException("Cannot subscribe to events if there is no OnlineClient.");

            Client.OnConnectionStatusChanged += OnConnectionStatusChanged;
            Client.OnChooseUsername += OnChooseUsername;
            Client.OnChooseUsernameResponse += OnChooseAUsernameResponse;
            Client.OnDisconnection += OnDisconnection;
            Client.OnLoginSuccess += OnLoginSuccess;
            Client.OnUserDisconnected += OnUserDisconnected;
            Client.OnUserConnected += OnUserConnected;
            Client.OnAvailableChatChannel += ChatManager.OnAvailableChatChannel;
            Client.OnJoinedChatChannel += ChatManager.OnJoinedChatChannel;
            Client.OnChatMessageReceived += ChatManager.OnChatMessageReceived;
            Client.OnLeftChatChannel += ChatManager.OnLeftChatChannel;
            Client.OnFailedToJoinChatChannel += ChatManager.OnFailedToJoinChatChannel;
            Client.OnMuteEndTimeReceived += ChatManager.OnMuteEndTimeReceived;
            Client.OnNotificationReceived += OnNotificationReceived;
            Client.OnRetrievedOnlineScores += OnRetrievedOnlineScores;
            Client.OnScoreSubmitted += OnScoreSubmitted;
            Client.OnUserConnected += ChatManager.Dialog.OnlineUsersHeader.OnUserConnected;
            Client.OnUsersOnline += OnUsersOnline;
            Client.OnUsersOnline += ChatManager.Dialog.OnlineUsersHeader.OnUsersOnline;
            Client.OnUserInfoReceived += OnUserInfoReceived;
            Client.OnUserStatusReceived += OnUserStatusReceived;
            Client.OnMultiplayerGameInfoReceived += OnMultiplayerGameInfoReceived;
            Client.OnJoinedMultiplayerGame += OnJoinedMultiplayerGame;
            Client.OnGameHostChanged += OnGameHostChanged;
            Client.OnGameDisbanded += OnGameDisbanded;
            Client.OnJoinGameFailed += OnJoinGameFailed;
            Client.OnDifficultyRangeChanged += OnDifficultyRangeChanged;
            Client.OnMaxSongLengthChanged += OnMaxSongLengthChanged;
            Client.OnAllowedModesChanged += OnAllowedModesChanged;
            Client.OnChangedModifiers += OnChangedModifiers;
            Client.OnFreeModTypeChanged += OnFreeModTypeChanged;
            Client.OnPlayerChangedModifiers += OnPlayerChangedModifiers;
            Client.OnGameKicked += OnGameKicked;
            Client.OnGameNameChanged += OnGameNameChanged;
            Client.OnGameInvite += OnGameInvite;
            Client.OnGameHealthTypeChanged += OnGameHealthTypeChanged;
            Client.OnGameLivesChanged += OnGameLivesChanged;
            Client.OnGameHostRotationChanged += OnGameHostRotationChanged;
            Client.OnGamePlayerTeamChanged += OnGamePlayerTeamChanged;
            Client.OnGameRulesetChanged += OnGameRulesetChanged;
            Client.OnGameLongNotePercentageChanged += OnGameLongNotePercentageChanged;
            Client.OnGameMaxPlayersChanged += OnGameMaxPlayersChanged;
            Client.OnGameTeamWinCount += OnGameTeamWinCountChanged;
            Client.OnGamePlayerWinCount += OnGamePlayerWinCount;
            Client.OnUserStats += OnUserStats;
            Client.OnUserJoinedGame += OnUserJoinedGame;
            Client.OnUserLeftGame += OnUserLeftGame;
            Client.OnGameEnded += OnGameEnded;
            Client.OnGameStarted += OnGameStarted;
            Client.OnGamePlayerNoMap += OnGamePlayerNoMap;
            Client.OnGamePlayerHasMap += OnGamePlayerHasMap;
            Client.OnGameHostSelectingMap += OnGameHostSelectingMap;
            Client.OnGameSetReferee += OnGameSetReferee;
            Client.OnStartedSpectatingPlayer += OnStartedSpectatingPlayer;
            Client.OnStoppedSpectatingPlayer += OnStoppedSpectatingPlayer;
            Client.OnSpectatorJoined += OnSpectatorJoined;
            Client.OnSpectatorLeft += OnSpectatorLeft;
            Client.OnSpectatorReplayFrames += OnSpectatorReplayFrames;
        }

        /// <summary>
        ///     Called when the connection status of the user has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            Status.Value = e.Status;

            if (Status.Value == ConnectionStatus.Connected)
                return;

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen?.Type == QuaverScreenType.Lobby || CurrentGame != null)
            {
                LeaveLobby();
                CurrentGame = null;
                game.CurrentScreen?.Exit(() => new MenuScreen());
            }
        }

        /// <summary>
        ///     Called when the user needs to choose a username.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChooseUsername(object sender, ChooseAUsernameEventArgs e) => DialogManager.Show(new UsernameSelectionDialog(0.75f));

        /// <summary>
        ///     Called when the client receives a response after selecting a username.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChooseAUsernameResponse(object sender, ChooseAUsernameResponseEventArgs e)
        {
            Logger.Important(e.Message, LogType.Network);

            if (e.Status != 200)
            {
                // If it wasn't successful, have them pick another username.
                DialogManager.Show(new UsernameSelectionDialog(0.75f));
            }

            switch (e.Status)
            {
                // Success
                case 200:
                    break;
                // Unauthorized
                case 401:
                    NotificationManager.Show(NotificationLevel.Error, e.Message);
                    break;
                // Username already taken.
                case 409:
                    NotificationManager.Show(NotificationLevel.Error, "The username you have selected is already taken.");
                    break;
                // Invalid username choice.
                case 422:
                    NotificationManager.Show(NotificationLevel.Error, "The username you have chosen is invalid.");
                    break;
                // No server response.
                default:
                    NotificationManager.Show(NotificationLevel.Error, "No response from the server.");
                    break;
            }
        }

        /// <summary>
        ///     When the client disconnects from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDisconnection(object sender, DisconnectedEventArgs e)
        {
            Logger.Important($"Disconnected from the server for reason: {e.CloseEventArgs.Reason} with code: {e.CloseEventArgs.Code}", LogType.Network);

            // If the user can't initially connect to the server (server is down.)
            switch (e.CloseEventArgs.Code)
            {
                // Error ocurred while connecting.
                case 1006:
                    NotificationManager.Show(NotificationLevel.Error, "Unable to connect to the server");
                    return;
                // Authentication Failed
                case 1002:
                    NotificationManager.Show(NotificationLevel.Error, "Failed to authenticate to the server");
                    return;
            }
        }

        /// <summary>
        ///     When the client successfully logs into the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            Self = e.Self;

            ChatManager.MuteTimeLeft = Self.OnlineUser.MuteEndTime - (long) TimeHelper.GetUnixTimestampMilliseconds();
            ChatManager.Dialog.OnlineUserList.ClearAllUsers();

            for (var i = ChatManager.JoinedChatChannels.Count - 1; i >= 0; i--)
            {
                var chan = ChatManager.JoinedChatChannels[i];

                if (chan.IsPrivate)
                    continue;

                if (chan.Name.StartsWith("#multi"))
                    ChatManager.OnLeftChatChannel(null, new LeftChatChannelEventArgs(chan.Name));

                Client?.JoinChatChannel(chan.Name);
            }

            lock (OnlineUsers)
            {
                OnlineUsers.Clear();
                OnlineUsers[e.Self.OnlineUser.Id] = e.Self;
            }

            // Make sure the config username is changed.
            ConfigManager.Username.Value = Self.OnlineUser.Username;

            DiscordHelper.Presence.LargeImageText = GetRichPresenceLargeKeyText(GameMode.Keys4);
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            // Send client status update packet.
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen != null)
                Client?.UpdateClientStatus(game.CurrentScreen.GetClientStatus());

            ChatManager.Dialog.OnlineUserList.HandleNewOnlineUsers(new List<User>() {Self});

            Trace.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
        }

        /// <summary>
        ///     Called when a user connects to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserConnected(object sender, UserConnectedEventArgs e)
        {
            if (OnlineUsers.ContainsKey(e.User.OnlineUser.Id))
                return;

            OnlineUsers[e.User.OnlineUser.Id] = e.User;
            ChatManager.Dialog.OnlineUserList.HandleNewOnlineUsers(new List<User>() { e.User });

            Trace.WriteLine($"User: {e.User.OnlineUser.Username} [{e.User.OnlineUser.SteamId}] (#{e.User.OnlineUser.Id}) has connected to the server.");
            Trace.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
        }

        /// <summary>
        ///     Called when a user disconnects from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
        {
            if (!OnlineUsers.ContainsKey(e.UserId) || e.UserId == Self.OnlineUser.Id)
                return;

            OnlineUsers.Remove(e.UserId);

            Console.WriteLine(Self.OnlineUser.Id);

            Console.WriteLine("User disconnected: " + e.UserId);
            ChatManager.Dialog.OnlineUserList.HandleDisconnectingUser(e.UserId);
            ChatManager.Dialog.OnlineUsersHeader.UpdateOnlineUserCount();

            Trace.WriteLine($"User: #{e.UserId} has disconnected from the server.");
            Trace.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
        }

        /// <summary>
        ///     Called when the user receives a notification from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnNotificationReceived(object sender, NotificationEventArgs e)
        {
            NotificationLevel level;

            switch (e.Type)
            {
                case ServerNotificationType.Error:
                    level = NotificationLevel.Error;
                    break;
                case ServerNotificationType.Success:
                    level = NotificationLevel.Success;
                    break;
                case ServerNotificationType.Info:
                    level = NotificationLevel.Info;
                    break;
                default:
                    level = NotificationLevel.Default;
                    break;
            }

            NotificationManager.Show(level, e.Content);
        }

        /// <summary>
        ///     Called when retrieving online scores or updates about our map.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRetrievedOnlineScores(object sender, RetrievedOnlineScoresEventArgs e)
        {
            Logger.Important($"Retrieved scores and ranked status for: {e.Id} | {e.Md5} | {e.Response.Code}", LogType.Network);

            try
            {
                var mapsets = MapManager.Mapsets.Where(x => x.Maps.Any(y => y.MapId == e.Id && y.Md5Checksum == e.Md5)).ToList();

                if (mapsets.Count == 0)
                    return;

                var map = mapsets.First().Maps.Find(x => x.MapId == e.Id && x.Md5Checksum == e.Md5);

                switch (e.Response.Code)
                {
                    case OnlineScoresResponseCode.NotSubmitted:
                        map.RankedStatus = RankedStatus.NotSubmitted;
                        break;
                    case OnlineScoresResponseCode.NeedsUpdate:
                        break;
                    case OnlineScoresResponseCode.Unranked:
                        map.RankedStatus = RankedStatus.Unranked;
                        break;
                    case OnlineScoresResponseCode.Ranked:
                        map.RankedStatus = RankedStatus.Ranked;
                        break;
                    case OnlineScoresResponseCode.DanCourse:
                        map.RankedStatus = RankedStatus.DanCourse;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                MapDatabaseCache.UpdateMap(map);

                var game = GameBase.Game as QuaverGame;

                // If in song select, update the banner of the currently selected map.
                if (game.CurrentScreen is SelectScreen screen)
                {
                    var view = screen.View as SelectScreenView;

                    if (MapManager.Selected.Value == map)
                        view.Banner.RankedStatus.UpdateMap(map);
                }
            }
            catch (Exception)
            {
                // ignored. This can happen for donor-only maps
            }
        }

        /// <summary>
        ///     Called when we've successfully submitted a score to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnScoreSubmitted(object sender, ScoreSubmissionEventArgs e)
        {
            if (e.Response == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "Failed to submit score! Retrying shortly.");
                return;
            }

            if (e.Response.Status != 200)
                return;

            Self.Stats[e.Response.GameMode] = e.Response.Stats.ToUserStats(e.Response.GameMode);

            // Unlock any achievements
            if (e.Response.Achievements.Count > 0)
                new SteamAchievements(e.Response.Achievements).Unlock();

            DiscordHelper.Presence.LargeImageText = GetRichPresenceLargeKeyText(e.Response.GameMode);
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <summary>
        ///     Gets the large key text for discord rich presence.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static string GetRichPresenceLargeKeyText(GameMode mode)
        {
            DiscordHelper.Presence.LargeImageText = ConfigManager.Username.Value;

            // Don't continue if not connected online. Only set to username.
            if (!Connected)
                return DiscordHelper.Presence.LargeImageText;

            if (Self.Stats.ContainsKey(mode))
            {
                var stats = Self.Stats[mode];

                if (stats.Rank != -1 && stats.CountryRank != -1)
                    DiscordHelper.Presence.LargeImageText += $" - Global: #{stats.Rank} | {Self.OnlineUser.CountryFlag}: #{stats.CountryRank}";
            }

            return DiscordHelper.Presence.LargeImageText;
        }

        /// <summary>
        ///     Called when we've retrieved info about a bundle of new online users.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUsersOnline(object sender, UsersOnlineEventArgs e)
        {
            var newOnlineUsers = new List<User>();

            foreach (var id in e.UserIds)
            {
                if (OnlineUsers.ContainsKey(id))
                    continue;

                // Convert them to users.
                var user = new User()
                {
                    OnlineUser = new OnlineUser
                    {
                        Id = id
                    }
                };

                OnlineUsers[user.OnlineUser.Id] = user;
                newOnlineUsers.Add(user);
            }

            ChatManager.Dialog.OnlineUserList.HandleNewOnlineUsers(newOnlineUsers);
        }

        /// <summary>
        ///     Called when receiving user info from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnUserInfoReceived(object sender, UserInfoEventArgs e)
        {
            ThreadScheduler.Run(() =>
            {
                foreach (var user in e.Users)
                {
                    OnlineUsers[user.Id] = new User(user);
                    ChatManager.Dialog.OnlineUserList?.UpdateUserInfo(OnlineUsers[user.Id]);
                }
            });
        }

        /// <summary>
        ///     Called when receiving user statuses from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserStatusReceived(object sender, UserStatusEventArgs e)
        {
            ThreadScheduler.Run(() =>
            {
                foreach (var user in e.Statuses)
                {
                    if (!OnlineUsers.ContainsKey(user.Key))
                        continue;

                    var onlineUser = OnlineUsers[user.Key];
                    onlineUser.CurrentStatus = user.Value;

                    ChatManager.Dialog.OnlineUserList?.UpdateUserInfo(onlineUser);
                }
            });
        }

        /// <summary>
        ///     Called whenever receiving information about a multiplayer game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnMultiplayerGameInfoReceived(object sender, MultiplayerGameInfoEventArgs e)
        {
            MultiplayerGames[e.Game.Id] = e.Game;

            e.Game.PlayerIds.ForEach(x =>
            {
                if (OnlineUsers.ContainsKey(x))
                    e.Game.Players.Add(OnlineUsers[x].OnlineUser);
            });

            if (OnlineUsers.ContainsKey(e.Game.HostId))
                e.Game.Host = OnlineUsers[e.Game.HostId].OnlineUser;

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type != QuaverScreenType.Lobby || game.CurrentScreen.Exiting)
                return;

            Logger.Important($"Received multiplayer game info: ({MultiplayerGames.Count}) - {e.Game.Id} | {e.Game.Name} | {e.Game.HasPassword} | {e.Game.Password}\n", LogType.Network);

            var screen = (LobbyScreen) game.CurrentScreen;

            screen.AddOrUpdateGame(e.Game);
            var view = screen.View as LobbyScreenView;
            view?.MatchContainer.FilterGames(view.Searchbox.RawText, true);
        }

        /// <summary>
        ///     Called when the player successfully joins a multiplayer game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnJoinedMultiplayerGame(object sender, JoinedGameEventArgs e)
        {
            // In the event that the game doesn't exist in our list.
            // We'll probably want to handle this in a different way, but for now, we should
            // assume that the user always has the game in their list.
            if (!MultiplayerGames.ContainsKey(e.GameId))
            {
                Logger.Warning($"Server tried to place us in game: {e.GameId}, but it doesn't exist!", LogType.Runtime);
                return;
            }

            CurrentGame = MultiplayerGames[e.GameId];
            CurrentGame.Players.Add(Self.OnlineUser);
            CurrentGame.PlayerIds.Add(Self.OnlineUser.Id);
            CurrentGame.PlayerMods.Add(new MultiplayerPlayerMods { UserId = Self.OnlineUser.Id, Modifiers = "0"});

            // Get the current screen
            var game = (QuaverGame) GameBase.Game;

            game.CurrentScreen.Exit(() =>
            {
                Logger.Important($"Successfully joined game: {CurrentGame.Id} | {CurrentGame.Name} | {CurrentGame.HasPassword}", LogType.Network);
                return new MultiplayerScreen(CurrentGame);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameHostChanged(object sender, GameHostChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (!OnlineUsers.ContainsKey(e.UserId))
            {
                Logger.Warning($"Game host changed to user: {e.UserId}, but they are not online!", LogType.Network);
                return;
            }

            CurrentGame.Host = OnlineUsers[e.UserId].OnlineUser;
            CurrentGame.HostId = e.UserId;

            if (CurrentGame.Host == Self.OnlineUser)
                NotificationManager.Show(NotificationLevel.Success, "You are now the host of the game!");

            Logger.Important($"New multiplayer game host: {CurrentGame.Host.Username} (#{CurrentGame.Host.Id})", LogType.Network);
        }

        /// <summary>
        ///     Called when a multiplayer game has been disbanded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameDisbanded(object sender, GameDisbandedEventArgs e)
        {
            if (!MultiplayerGames.ContainsKey(e.GameId))
                return;

            var game = MultiplayerGames[e.GameId];

            // Handle disbanding logic UI wise.
            var quaver = (QuaverGame) GameBase.Game;

            if (quaver.CurrentScreen.Type != QuaverScreenType.Lobby)
                return;

            var screen = (LobbyScreen) quaver.CurrentScreen;
            screen.DeleteGame(game);

            Logger.Important($"Game: {game.Name} <{game.Id}> has been disbanded", LogType.Network);
            MultiplayerGames.Remove(e.GameId);
        }

        /// <summary>
        ///     Called when failing to join a multiplayer game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnJoinGameFailed(object sender, JoinGameFailedEventargs e)
        {
            Logger.Important($"Failed to join multiplayer match with reason: {e.Reason}", LogType.Network);

            string reason;

            switch (e.Reason)
            {
                case JoinGameFailureReason.Password:
                    reason = "Failed to join game because you have entered an incorrect password.";
                    break;
                case JoinGameFailureReason.Full:
                    reason = "The game you have tried to join is full.";
                    break;
                case JoinGameFailureReason.MatchNoExists:
                    reason = "The game you have tried to join no longer exists.";
                    break;
                default:
                    reason = "Failed to join multiplayer game for an unknown reason.";
                    break;
            }

            NotificationManager.Show(NotificationLevel.Error, reason);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDifficultyRangeChanged(object sender, DifficultyRangeChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.MinimumDifficultyRating = e.MinimumDifficulty;
            CurrentGame.MaximumDifficultyRating = e.MaximumDifficulty;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnMaxSongLengthChanged(object sender, MaxSongLengthChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.MaximumSongLength = e.Seconds;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnAllowedModesChanged(object sender, AllowedModesChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.AllowedGameModes = e.Modes;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChangedModifiers(object sender, ChangeModifiersEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.Modifiers = e.Modifiers.ToString();
            CurrentGame.DifficultyRating = e.DifficultyRating;

            if (ModManager.Mods != (ModIdentifier) e.Modifiers)
                MapLoadingScreen.AddModsFromIdentifiers(GetSelfActivatedMods());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnFreeModTypeChanged(object sender, FreeModTypeChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.FreeModType = e.Type;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPlayerChangedModifiers(object sender, PlayerChangedModifiersEventArgs e)
        {
            if (CurrentGame == null)
                return;

            var playerMods = CurrentGame.PlayerMods.Find(x => x.UserId == e.UserId);

            if (playerMods == null)
                CurrentGame.PlayerMods.Add(new MultiplayerPlayerMods { UserId = e.UserId, Modifiers = e.Modifiers.ToString()});
            else
            {
                playerMods.Modifiers = e.Modifiers.ToString();

                if (playerMods.UserId == Self.OnlineUser.Id)
                    MapLoadingScreen.AddModsFromIdentifiers(GetSelfActivatedMods());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnGameKicked(object sender, KickedEventArgs e)
        {
            LeaveGame(true);
            NotificationManager.Show(NotificationLevel.Error, "You have been kicked from the game!");

            var game = (QuaverGame) GameBase.Game;
            game.GlobalUserInterface.Cursor.Alpha = 1;
            game.CurrentScreen.Exit(() => new LobbyScreen());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnGameNameChanged(object sender, GameNameChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.Name = e.Name;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameInvite(object sender, GameInviteEventArgs e)
            => NotificationManager.Show(NotificationLevel.Info, $"{e.Sender} invited you to a game. Click here to join!",
            (o, args) =>
            {
                if (CurrentGame != null)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You already in a multiplayer game. Please leave it before joining another.");
                    return;
                }

                var game = (QuaverGame) GameBase.Game;
                var screen = game.CurrentScreen;

                switch (screen.Type)
                {
                    case QuaverScreenType.Menu:
                    case QuaverScreenType.Results:
                    case QuaverScreenType.Select:
                    case QuaverScreenType.Download:
                    case QuaverScreenType.Lobby:
                        DialogManager.Show(new JoiningGameDialog(JoiningGameDialogType.Joining));
                        ThreadScheduler.RunAfter(() => Client?.AcceptGameInvite(e.MatchId), 800);
                        break;
                    default:
                        NotificationManager.Show(NotificationLevel.Error, "Finish what you're doing before accepting this game invite.");
                        break;
                }
            });

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameHealthTypeChanged(object sender, HealthTypeChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.HealthType = e.HealthType;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameLivesChanged(object sender, LivesChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.Lives = e.Lives;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameHostRotationChanged(object sender, HostRotationChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.HostRotation = e.HostRotation;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnGamePlayerTeamChanged(object sender, PlayerTeamChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            lock (CurrentGame.RedTeamPlayers)
            lock (CurrentGame.BlueTeamPlayers)
            {
                switch (e.Team)
                {
                    case MultiplayerTeam.Red:
                        CurrentGame.BlueTeamPlayers.Remove(e.UserId);
                        CurrentGame.RedTeamPlayers.Add(e.UserId);
                        break;
                    case MultiplayerTeam.Blue:
                        CurrentGame.RedTeamPlayers.Remove(e.UserId);
                        CurrentGame.BlueTeamPlayers.Add(e.UserId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameRulesetChanged(object sender, RulesetChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.Ruleset = e.Ruleset;

            if (CurrentGame.Ruleset != MultiplayerGameRuleset.Team)
            {
                CurrentGame.RedTeamPlayers.Clear();
                CurrentGame.BlueTeamPlayers.Clear();

                // Leave the team chat if we're in one
                for (var i = ChatManager.JoinedChatChannels.Count - 1; i >= 0; i--)
                {
                    var chan = ChatManager.JoinedChatChannels[i];

                    if (chan.IsPrivate)
                        continue;

                    if (chan.Name.StartsWith("#multi_team"))
                        ChatManager.OnLeftChatChannel(null, new LeftChatChannelEventArgs(chan.Name));
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameLongNotePercentageChanged(object sender, LongNotePercentageChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.MinimumLongNotePercentage = e.Minimum;
            CurrentGame.MaximumLongNotePercentage = e.Maximum;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameMaxPlayersChanged(object sender, MaxPlayersChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            Console.WriteLine($"NEW PLAYHER COUJNT: " + e.MaxPlayers);
            CurrentGame.MaxPlayers = e.MaxPlayers;
        }

        private static void OnGameTeamWinCountChanged(object sender, TeamWinCountEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.RedTeamWins = e.RedTeamWins;
            CurrentGame.BlueTeamWins = e.BlueTeamWins;

            Logger.Important($"Team Win Count Updated: Red: {e.RedTeamWins} | Blue: {e.BlueTeamWins}", LogType.Network);
        }

        private static void OnGamePlayerWinCount(object sender, PlayerWinCountEventArgs e)
        {
            if (CurrentGame == null)
                return;

            Logger.Important($"Received updated win count for #{e.UserId}: {e.Wins}", LogType.Network);

            var playerWins = CurrentGame.PlayerWins.Find(x => x.UserId == e.UserId);

            if (playerWins != null)
            {
                playerWins.Wins = e.Wins;
                return;
            }

            CurrentGame.PlayerWins.Add(new MultiplayerPlayerWins() { UserId = e.UserId, Wins = e.Wins});
        }

        private static void OnUserStats(object sender, UserStatsEventArgs e)
        {
            foreach (var user in e.Stats)
            {
                foreach (var stats in user.Value)
                    OnlineUsers[user.Key].Stats[(GameMode) stats.Key] = stats.Value;
            }
        }

        private static void OnUserJoinedGame(object sender, UserJoinedGameEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (CurrentGame.Players.Any(x => x.Id != e.UserId))
                CurrentGame.Players.Add(OnlineUsers[e.UserId].OnlineUser);

            if (!CurrentGame.PlayerIds.Contains(e.UserId))
                CurrentGame.PlayerIds.Add(e.UserId);

            if (CurrentGame.PlayerMods.All(x => x.UserId != e.UserId))
                CurrentGame.PlayerMods.Add(new MultiplayerPlayerMods { UserId = e.UserId, Modifiers = "0"});
        }

        private static void OnUserLeftGame(object sender, UserLeftGameEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.PlayerIds.Remove(e.UserId);
            CurrentGame.PlayersWithoutMap.Remove(e.UserId);
            CurrentGame.PlayersReady.Remove(e.UserId);
            CurrentGame.PlayerMods.RemoveAll(x => x.UserId == e.UserId);
            CurrentGame.RedTeamPlayers.Remove(e.UserId);
            CurrentGame.BlueTeamPlayers.Remove(e.UserId);
            CurrentGame.Players.Remove(OnlineUsers[e.UserId].OnlineUser);
        }

        private static void OnGameEnded(object sender, GameEndedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.InProgress = false;
        }

        private static void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.InProgress = true;
        }

        private static void OnGamePlayerNoMap(object sender, PlayerGameNoMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (!CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                CurrentGame.PlayersWithoutMap.Add(e.UserId);
        }

        private static void OnGamePlayerHasMap(object sender, GamePlayerHasMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                CurrentGame.PlayersWithoutMap.Remove(e.UserId);
        }

        private static void OnGameHostSelectingMap(object sender, GameHostSelectingMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.HostSelectingMap = e.IsSelecting;
        }

        private static void OnGameSetReferee(object sender, GameSetRefereeEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.RefereeUserId = e.UserId;
            CurrentGame.BlueTeamPlayers.Remove(e.UserId);
            CurrentGame.RedTeamPlayers.Remove(e.UserId);
        }

        private static void OnStartedSpectatingPlayer(object sender, StartSpectatePlayerEventArgs e)
        {
            if (SpectatorClients.ContainsKey(e.UserId))
                return;

            if (!OnlineUsers.ContainsKey(e.UserId))
            {
                NotificationManager.Show(NotificationLevel.Warning, $"Tried to spectate user: {e.UserId}, but they are not online");
                return;
            }

            SpectatorClients[e.UserId] = new SpectatorClient(OnlineUsers[e.UserId]);

            // We don't have the player's information yet, so it needs to be requested from the server.
            if (!SpectatorClients[e.UserId].Player.HasUserInfo)
                Client?.RequestUserInfo(new List<int>() { e.UserId });

            if (SpectatorClients.Count == 1)
                NotificationManager.Show(NotificationLevel.Info, $"You are now spectating {SpectatorClients[e.UserId].Player.OnlineUser.Username}!");

            Logger.Important($"Starting spectating player: {e.UserId}", LogType.Network);
        }

        private static void OnStoppedSpectatingPlayer(object sender, StopSpectatePlayerEventArgs e)
        {
            if (!SpectatorClients.ContainsKey(e.UserId))
                return;

            SpectatorClients.Remove(e.UserId);

            NotificationManager.Show(NotificationLevel.Info, $"You are no longer spectating anymore!");

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type == QuaverScreenType.Gameplay && SpectatorClients.Count == 0)
                game.CurrentScreen.Exit(() => new MenuScreen());

            Logger.Important($"Stopped spectating player: {e.UserId}", LogType.Network);
        }

        private static void OnSpectatorJoined(object sender, SpectatorJoinedEventArgs e)
        {
            if (Spectators.ContainsKey(e.UserId))
                return;

            if (!OnlineUsers.ContainsKey(e.UserId))
                return;

            Spectators.Add(e.UserId, OnlineUsers[e.UserId]);

            if (!OnlineUsers[e.UserId].HasUserInfo)
                Client?.RequestUserInfo(new List<int> { e.UserId });

            Logger.Important($"Spectator Joined: {e.UserId}", LogType.Network);
        }

        private static void OnSpectatorLeft(object sender, SpectatorLeftEventArgs e)
        {
            if (!Spectators.ContainsKey(e.UserId))
                return;

            Spectators.Remove(e.UserId);

            Logger.Important($"Spectator Left: {e.UserId}", LogType.Network);
        }

        private static void OnSpectatorReplayFrames(object sender, SpectatorReplayFramesEventArgs e)
        {
            if (!SpectatorClients.ContainsKey(e.UserId))
                return;

            SpectatorClients[e.UserId].AddFrames(e);
        }

        /// <summary>
        ///     Leaves the current multiplayer game if any
        /// </summary>
        public static void LeaveGame(bool dontSendPacket = false)
        {
            if (CurrentGame == null)
                return;

            if (!dontSendPacket)
                Client.LeaveGame();

            MultiplayerGames.Clear();
            CurrentGame = null;
        }

        /// <summary>
        /// </summary>
        public static void JoinLobby() => Client?.JoinLobby();

        /// <summary>
        /// </summary>
        public static void LeaveLobby()
        {
            Client?.LeaveLobby();
            MultiplayerGames.Clear();
        }

        /// <summary>
        ///     Gets a user's activated mods in the current game.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ModIdentifier GetUserActivatedMods(int userId)
        {
            if (CurrentGame == null)
                return 0;

            var currMods = (ModIdentifier) long.Parse(CurrentGame.Modifiers);

            if (currMods < 0)
                currMods = 0;

            // Console.WriteLine("GAME MODS: " + currMods);

            var playerMods = CurrentGame.PlayerMods.Find(x => x.UserId == userId);

            if (playerMods != null)
            {
                var pm =  (ModIdentifier) long.Parse(playerMods.Modifiers);

                if (pm < 0)
                    pm = 0;

                currMods |= pm;

                // Console.WriteLine("PLAYER MODS: " + pm);
            }

            // Console.WriteLine("CURRENT MODS COMBINED: " + currMods);
            return currMods;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ModIdentifier GetSelfActivatedMods() => GetUserActivatedMods(Self.OnlineUser.Id);

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static MultiplayerTeam GetTeam(int userId)
        {
            if (CurrentGame == null || CurrentGame.Ruleset != MultiplayerGameRuleset.Team)
                return MultiplayerTeam.Red;

            if (CurrentGame.RedTeamPlayers.Contains(userId))
                return MultiplayerTeam.Red;

            if (CurrentGame.BlueTeamPlayers.Contains(userId))
                return MultiplayerTeam.Blue;

            return MultiplayerTeam.Red;
        }
    }
}
