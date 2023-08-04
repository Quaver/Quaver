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
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Difficulty.Rulesets.Keys;
using Quaver.Server.Client;
using Quaver.Server.Client.Events;
using Quaver.Server.Client.Events.Disconnnection;
using Quaver.Server.Client.Events.Login;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Listening;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs.Online;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Hub;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.Loading;
using Quaver.Shared.Screens.Main;
using Quaver.Shared.Screens.Multi;
using Quaver.Shared.Screens.MultiplayerLobby;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Dialogs;
using Quaver.Shared.Screens.Music;
using Quaver.Shared.Screens.Selection.UI.Leaderboard;
using Quaver.Shared.Screens.Tournament;
using Steamworks;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

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
                ListeningParty = null;
                FriendsList = new List<int>();
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
        ///     The active listening party the user is in
        /// </summary>
        public static ListeningParty ListeningParty { get; private set; }

        /// <summary>
        ///     The players who the client is currently spectating
        ///
        ///     Note:
        ///         - Only 1 player is allowed if not running a tournament client
        ///         - Otherwise multiple are allowed.
        /// </summary>
        public static Dictionary<int, SpectatorClient> SpectatorClients { get; private set; } = new Dictionary<int, SpectatorClient>();

        /// <summary>
        ///     Players who are currently spectating us
        /// </summary>
        public static Dictionary<int, User> Spectators { get; private set; } = new Dictionary<int, User>();

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
        ///     Returns if the user is the host of the listening party and can perform actions
        /// </summary>
        public static bool IsListeningPartyHost => ListeningParty == null || ListeningParty.Host == Self.OnlineUser;

        /// <summary>
        ///     The user's friends list
        /// </summary>
        public static List<int> FriendsList { get; private set; }

        /// <summary>
        ///     Returns if the user is currently wanting to fetch the realtime leaderboards
        /// </summary>
        public static bool ShouldFetchRealtimeLeaderboard => ConfigManager.EnableRealtimeOnlineScoreboard.Value
                                                             && ConfigManager.DisplayUnbeatableScoresDuringGameplay.Value
                                                             && CurrentGame == null;

        /// <summary>
        ///     Event invoked when the user's friends list has changed
        ///     (user added/removed)
        /// </summary>
        public static event EventHandler<FriendsListUserChangedEventArgs> FriendsListUserChanged;

        /// <summary>
        ///     List of currently available song requests
        /// </summary>
        public static List<SongRequest> SongRequests { get; } = new List<SongRequest>();

        /// <summary>
        /// </summary>
        public static string TwitchUsername { get; private set; }

        /// <summary>
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            if (Status.Value != ConnectionStatus.Disconnected)
                return;

            Logger.Important($"Attempting to log into the Quaver server...", LogType.Network);

            // Create the new online client and subscribe to all of its online events.
            if (Client == null)
            {
                Client = new OnlineClient();
                SubscribeToEvents();
            }

            // TODO: Replace with some backend request to check if users have accepted both.
            if (!ConfigManager.AcceptedTermsAndPrivacyPolicy.Value)
            {
                Logger.Important($"Player needs to accept TOS & Privacy Policy...", LogType.Runtime);

                DialogManager.Show(new LoadingDialog("PLEASE WAIT", "Loading Terms of Service. Please wait...", () =>
                {
                    try
                    {
                        var dialog = new TermsOfServiceDialog();
                        DialogManager.Show(dialog);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, LogType.Runtime);
                        NotificationManager.Show(NotificationLevel.Error, "There was an issue while fetching the Terms of Service document.");
                    }
                }));

                return;
            }

            // Initiate the connection to the game server.
            Client.Connect(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(), false);
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
            Client.OnNotificationReceived += OnNotificationReceived;
            Client.OnRetrievedOnlineScores += OnRetrievedOnlineScores;
            Client.OnScoreSubmitted += OnScoreSubmitted;
            Client.OnUsersOnline += OnUsersOnline;
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
            Client.OnStoppedSpectatingPlayer += OnStoppedSpectatingPlayer;
            Client.OnStartedSpectatingPlayer += OnStartedSpectatingPlayer;
            Client.OnSpectatorJoined += OnSpectatorJoined;
            Client.OnSpectatorLeft += OnSpectatorLeft;
            Client.OnSpectatorReplayFrames += OnSpectatorReplayFrames;
            Client.OnSpectateMultiplayerGame += OnSpectateMultiplayerGame;
            Client.OnGameEnded += OnGameEnded;
            Client.OnGameStarted += OnGameStarted;
            Client.OnGamePlayerNoMap += OnGamePlayerNoMap;
            Client.OnGamePlayerHasMap += OnGamePlayerHasMap;
            Client.OnGameHostSelectingMap += OnGameHostSelectingMap;
            Client.OnGameSetReferee += OnGameSetReferee;
            Client.OnGameMapChanged += OnGameMapChanged;
            Client.OnPlayerReady += OnGamePlayerReady;
            Client.OnPlayerNotReady += OnGamePlayerNotReady;
            Client.OnJoinedListeningParty += OnJoinedListeningParty;
            Client.OnLeftListeningParty += OnLeftListeningParty;
            Client.OnListeningPartyStateUpdate += OnListeningPartyStateUpdate;
            Client.OnListeningPartyFellowJoined += OnListeningPartyFellowJoined;
            Client.OnListeningPartyFellowLeft += OnListeningPartyFellowLeft;
            Client.OnListeningPartyChangeHost += OnListeningPartyChangeHost;
            Client.OnListeningPartyUserMissingSong += OnListeningPartyUserMissingSong;
            Client.OnListeningPartyUserHasSong += OnListeningPartyUserHasSong;
            Client.OnUserFriendsListReceived += OnUserFriendsListReceieved;
            Client.OnSongRequestReceived += OnSongRequestReceived;
            Client.OnTwitchConnectionReceived += OnTwitchConnectionReceived;
            Client.OnGameMapsetShared += OnGameMapsetShared;
            Client.OnGameCountdownStart += OnGameCountdownStarted;
            Client.OnGameCountdownStop += OnGameCountdownStopped;
            Client.OnTournamentModeChanged += OnTournamentModeChanged;
            Client.OnGameNeedDifficultyRatings += OnNeedsDifficultyRatings;
            Client.OnAutoHostChanged += OnAutoHostChanged;
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
                game.CurrentScreen?.Exit(() => new MainMenuScreen());
            }

            ListeningParty = null;
        }

        /// <summary>
        ///     Called when the user needs to choose a username.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChooseUsername(object sender, ChooseAUsernameEventArgs e) => DialogManager.Show(new CreateUsernameDialog());

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
                DialogManager.Show(new CreateUsernameDialog());
            }

            switch (e.Status)
            {
                // Success
                case 200:
                    NotificationManager.Show(NotificationLevel.Success, "Account successfully created. You are now being logged in!");
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

            // Remove the active listening party
            ListeningParty = null;
        }

        /// <summary>
        ///     When the client successfully logs into the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            Self = e.Self;

            ListeningParty = null;

            lock (OnlineUsers)
            {
                OnlineUsers.Clear();
                OnlineUsers[e.Self.OnlineUser.Id] = e.Self;
            }

            // Make sure the config username is changed.
            ConfigManager.Username.Value = Self.OnlineUser.Username;

            DiscordHelper.Presence.LargeImageText = GetRichPresenceLargeKeyText(GameMode.Keys4);
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordHelper.Presence.PartyMax = 0;
            DiscordHelper.Presence.PartySize = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            // Send client status update packet.
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen != null)
                Client?.UpdateClientStatus(game.CurrentScreen.GetClientStatus());
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

            // Display notification for online friends
            if (FriendsList.Contains(e.User.OnlineUser.Id) && ConfigManager.DisplayFriendOnlineNotifications != null
                                                           && ConfigManager.DisplayFriendOnlineNotifications.Value)
            {
                NotificationManager.Show(NotificationLevel.Success, $"{e.User.OnlineUser.Username} has logged in!");
            }

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
            Logger.Important($"Retrieved scores and ranked status for: {e.Id} | {e.Md5}", LogType.Network);

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
                        map.RankedStatus = RankedStatus.NotSubmitted;
                        break;
                }

                if ((int) e.Response.Code == -1)
                    map.RankedStatus = map.MapId == -1 ? RankedStatus.NotSubmitted : RankedStatus.Unranked;

                // Update online grade
                if (ConfigManager.LeaderboardSection.Value != LeaderboardType.Rate && e.Response.PersonalBest != null)
                {
                    var onlineGrade = GradeHelper.GetGradeFromAccuracy((float) e.Response.PersonalBest.Accuracy);

                    if (GradeHelper.GetGradeImportanceIndex(onlineGrade) > GradeHelper.GetGradeImportanceIndex(map.OnlineGrade))
                        map.OnlineGrade = onlineGrade;
                }

                map.DateLastUpdated = e.Response.DateLastUpdated;
                map.OnlineOffset = e.Response.OnlineOffset;
                MapDatabaseCache.UpdateMap(map);

                var game = GameBase.Game as QuaverGame;

                // // If in song select, update the banner of the currently selected map.
                // if (game.CurrentScreen is SelectScreen screen)
                // {
                //     var view = screen.View as SelectScreenView;
                //
                //     if (MapManager.Selected.Value == map)
                //         view.Banner.RankedStatus.UpdateMap(map);
                // }
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
            if (e.Response.Achievements != null && e.Response.Achievements.Count > 0)
                new SteamAchievements(e.Response.Achievements).Unlock();

            try
            {
                DiscordHelper.Presence.LargeImageText = GetRichPresenceLargeKeyText(e.Response.GameMode);
                DiscordHelper.Presence.EndTimestamp = 0;
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);
            }
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

            // ChatManager.Dialog.OnlineUserList.HandleNewOnlineUsers(newOnlineUsers);
        }

        /// <summary>
        ///     Called when receiving user info from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnUserInfoReceived(object sender, UserInfoEventArgs e)
        {
            foreach (var user in e.Users)
                OnlineUsers[user.Id].OnlineUser = user;
        }

        /// <summary>
        ///     Called when receiving user statuses from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserStatusReceived(object sender, UserStatusEventArgs e)
        {
            foreach (var user in e.Statuses)
            {
                if (!OnlineUsers.ContainsKey(user.Key))
                    continue;

                var onlineUser = OnlineUsers[user.Key];
                onlineUser.CurrentStatus = user.Value ?? new UserClientStatus(ClientStatus.InMenus, -1, "", 1, "", 0);

                // ChatManager.Dialog.OnlineUserList?.UpdateUserInfo(onlineUser);
            }
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
            
            Logger.Important($"Received multiplayer game info: ({MultiplayerGames.Count}) - {e.Game.Id} | {e.Game.Name} " +
                             $"| {e.Game.HasPassword} | {e.Game.Password}", LogType.Network);
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
                return new MultiplayerGameScreen();
            });
        }

        /// <summary>
        ///     Called when joining a game to spectate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSpectateMultiplayerGame(object sender, SpectateMultiplayerGameEventArgs e)
        {
            if (!MultiplayerGames.ContainsKey(e.GameId))
            {
                Logger.Warning($"Server tried to place us in game: {e.GameId}, but it doesn't exist!", LogType.Runtime);
                return;
            }

            CurrentGame = MultiplayerGames[e.GameId];
            CurrentGame.IsSpectating = true;

            var game = (QuaverGame) GameBase.Game;

            game.CurrentScreen.Exit(() =>
            {
                Logger.Important($"Successfully joined game to spectate: {CurrentGame.Id} | {CurrentGame.Name} | {CurrentGame.HasPassword}", LogType.Network);
                return new MultiplayerGameScreen();
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
            game.CurrentScreen.Exit(() => new MultiplayerLobbyScreen());
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
            Logger.Important($"Received new multiplayer game name: {CurrentGame.Name}", LogType.Runtime);
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
                        DialogManager.Show(new JoinGameDialog(null, null, true));
                        Client?.AcceptGameInvite(e.MatchId);
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
            CurrentGame.RedTeamPlayers.Clear();
            CurrentGame.BlueTeamPlayers.Clear();
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameTeamWinCountChanged(object sender, TeamWinCountEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.RedTeamWins = e.RedTeamWins;
            CurrentGame.BlueTeamWins = e.BlueTeamWins;

            Logger.Important($"Team Win Count Updated: Red: {e.RedTeamWins} | Blue: {e.BlueTeamWins}", LogType.Network);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserStats(object sender, UserStatsEventArgs e)
        {
            foreach (var user in e.Stats)
            {
                foreach (var stats in user.Value)
                    OnlineUsers[user.Key].Stats[(GameMode) stats.Key] = stats.Value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (CurrentGame.PlayerIds.Count == 0)
            {
                var quaver = (QuaverGame) GameBase.Game;

                if (quaver.CurrentScreen.Type == QuaverScreenType.Multiplayer)
                    quaver.CurrentScreen.Exit(() => new MultiplayerLobbyScreen());
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameEnded(object sender, GameEndedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.InProgress = false;
            CurrentGame.CountdownStartTime = -1;
            CurrentGame.PlayersReady.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameStarted(object sender, GameStartedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.InProgress = true;

            CurrentGame.PlayersReady.Clear();
            CurrentGame.CountdownStartTime = -1;

            // User doesn't have the map
            if (CurrentGame.PlayersWithoutMap.Contains(Self.OnlineUser.Id))
            {
                NotificationManager.Show(NotificationLevel.Warning, "The match was started, but you do not have the map!");
                return;
            }

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen is MultiplayerGameScreen screen)
                screen.DontLeaveGameUponScreenSwitch = true;

            if (CurrentGame.IsSpectating || CurrentGame.RefereeUserId == Self.OnlineUser.Id)
            {
                MultiplayerGameScreen.SelectMultiplayerMap();

                if (MapManager.Selected.Value == null)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Cannot start tournament viewer, as you do not have the map!");
                    return;
                }

                BackgroundHelper.Load(MapManager.Selected.Value);

                if (!game.CurrentScreen.Exiting)
                {
                    foreach (var spect in SpectatorClients.Values)
                        spect.WatchUserImmediately();

                    game.CurrentScreen.Exit(() => new TournamentScreen(CurrentGame, SpectatorClients.Values.ToList()));
                }

                return;
            }

            game.CurrentScreen.Exit(() => new MapLoadingScreen(GetScoresFromMultiplayerUsers()));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGamePlayerNoMap(object sender, PlayerGameNoMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (!CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                CurrentGame.PlayersWithoutMap.Add(e.UserId);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGamePlayerHasMap(object sender, GamePlayerHasMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            if (CurrentGame.PlayersWithoutMap.Contains(e.UserId))
                CurrentGame.PlayersWithoutMap.Remove(e.UserId);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameHostSelectingMap(object sender, GameHostSelectingMapEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.HostSelectingMap = e.IsSelecting;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameMapChanged(object sender, GameMapChangedEventArgs e)
        {
            // Make sure to clear all the players that don't have the map, as this information is
            // now outdated.
            CurrentGame.PlayersWithoutMap.Clear();
            CurrentGame.PlayersReady.Clear();

            CurrentGame.MapMd5 = e.MapMd5;
            CurrentGame.AlternativeMd5 = e.AlternativeMd5;
            CurrentGame.MapId = e.MapId;
            CurrentGame.MapsetId = e.MapsetId;
            CurrentGame.Map = e.Map;
            CurrentGame.DifficultyRating = e.DifficultyRating;
            CurrentGame.AllDifficultyRatings = e.AllDifficultyRatings;
            CurrentGame.GameMode = e.GameMode;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameSetReferee(object sender, GameSetRefereeEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.RefereeUserId = e.UserId;
            CurrentGame.BlueTeamPlayers.Remove(e.UserId);
            CurrentGame.RedTeamPlayers.Remove(e.UserId);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGamePlayerReady(object sender, PlayerReadyEventArgs e)
        {
            if (!CurrentGame.PlayersReady.Contains(e.UserId))
                CurrentGame.PlayersReady.Add(e.UserId);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGamePlayerNotReady(object sender, PlayerNotReadyEventArgs e)
        {
            if (CurrentGame.PlayersReady.Contains(e.UserId))
                CurrentGame.PlayersReady.Remove(e.UserId);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (SpectatorClients.Count == 1 && CurrentGame == null)
                NotificationManager.Show(NotificationLevel.Info, $"You are now spectating {SpectatorClients[e.UserId].Player.OnlineUser.Username}!");

            Logger.Important($"Starting spectating player: {e.UserId}", LogType.Network);
        }

        private static void OnStoppedSpectatingPlayer(object sender, StopSpectatePlayerEventArgs e)
        {
            if (!SpectatorClients.ContainsKey(e.UserId))
                return;

            SpectatorClients.Remove(e.UserId);

            if (CurrentGame == null)
                NotificationManager.Show(NotificationLevel.Info, $"You are no longer spectating anymore!");

            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type == QuaverScreenType.Gameplay && SpectatorClients.Count == 0)
            {
                if (!game.CurrentScreen.Exiting)
                {
                    if (game.CurrentScreen is TournamentScreen)
                        game.CurrentScreen.Exit(() => new MultiplayerLobbyScreen());
                    else
                    {
                        game.CurrentScreen.Exit(() => new MainMenuScreen());
                    }
                }
            }

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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSpectatorReplayFrames(object sender, SpectatorReplayFramesEventArgs e)
        {
            if (!SpectatorClients.ContainsKey(e.UserId))
                return;

            SpectatorClients[e.UserId].AddFrames(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnJoinedListeningParty(object sender, JoinedListeningPartyEventArgs e)
        {
            ListeningParty = e.Party;
            ListeningParty.Host = OnlineUsers[e.Party.HostId].OnlineUser;

            // Give the most up to date state for the listening party
            if (ListeningParty.Host == Self.OnlineUser)
                UpdateListeningPartyState(ListeningPartyAction.ChangeSong);

            // Make sure the listeners list is all up to date
            foreach (var userId in e.Party.ListenerIds)
            {
                if (!OnlineUsers.ContainsKey(userId))
                    continue;

                var listener = OnlineUsers[userId].OnlineUser;

                if (!ListeningParty.Listeners.Contains(listener))
                    ListeningParty.Listeners.Add(listener);
            }

            Logger.Important($"Successfully joined {ListeningParty.HostId}'s listening party.", LogType.Runtime);

            // Go to the music player screen if not already in it
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type == QuaverScreenType.Music)
                return;

            game.CurrentScreen.Exit(() => new MusicPlayerScreen());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLeftListeningParty(object sender, ListeningPartyLeftEventArgs e)
        {
            ListeningParty = null;

            Logger.Important($"Server informed us that we've left the current listening party.", LogType.Runtime);

            // The user is still currently on the screen, so kick them out and head over to the main menu
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen.Type != QuaverScreenType.Music)
                return;

            game.CurrentScreen.Exit(() => new MainMenuScreen());
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnListeningPartyStateUpdate(object sender, ListeningPartyStateUpdateEventArgs e)
        {
            if (ListeningParty == null)
                return;

            ListeningParty.MapMd5 = e.MapMd5;
            ListeningParty.MapId = e.MapId;
            ListeningParty.LastActionTime = e.LastActionTime;
            ListeningParty.SongTime = e.SongTime;
            ListeningParty.IsPaused = e.IsPaused;
            ListeningParty.SongArtist = e.SongArtist;
            ListeningParty.SongTitle = e.SongTitle;

            // Clear the listeners who don't have the active song
            if (e.Action == ListeningPartyAction.ChangeSong)
            {
                ListeningParty.ListenersWithoutSong.Clear();
                ListeningParty.ListenerIdsWithoutSong.Clear();
            }

            try
            {
                Logger.Important($"Received listening party state update: {e.Action} | {e.MapMd5} | {e.MapId} | {e.LastActionTime} " +
                                 $"| {e.SongTime} | {e.IsPaused} | {e.SongArtist} | {e.SongTitle}", LogType.Runtime);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnListeningPartyFellowJoined(object sender, ListeningPartyFellowJoinedEventArgs e)
        {
            if (ListeningParty == null)
                return;

            // Add to listener object list
            if (OnlineUsers.ContainsKey(e.UserId))
            {
                var listener = OnlineUsers[e.UserId].OnlineUser;

                if (!ListeningParty.Listeners.Contains(listener))
                    ListeningParty.Listeners.Add(listener);
            }

            // Add to listener ids list.
            if (!ListeningParty.ListenerIds.Contains(e.UserId))
                ListeningParty.ListenerIds.Add(e.UserId);

            Logger.Important($"{e.UserId} has joined the listening party", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnListeningPartyFellowLeft(object sender, ListeningPartyFellowLeftEventArgs e)
        {
            if (ListeningParty == null)
                return;

            // Remove from listener object list
            if (OnlineUsers.ContainsKey(e.UserId))
                ListeningParty.Listeners.Remove(OnlineUsers[e.UserId].OnlineUser);

            // Remove from the list
            ListeningParty.ListenerIds.Remove(e.UserId);

            Logger.Important($"{e.UserId} has left the listening party", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnListeningPartyChangeHost(object sender, ListeningPartyChangeHostEventArgs e)
        {
            if (ListeningParty == null)
                return;

            ListeningParty.Host = OnlineUsers[e.UserId].OnlineUser;
            ListeningParty.HostId = e.UserId;

            Logger.Important($"{e.UserId} has become host of the listening party.", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnListeningPartyUserMissingSong(object sender, ListeningPartyUserMissingSongEventArgs e)
        {
            if (ListeningParty == null)
                return;

            if (OnlineUsers.ContainsKey(e.UserId))
            {
                var user = OnlineUsers[e.UserId];

                if (!ListeningParty.ListenersWithoutSong.Contains(user.OnlineUser))
                    ListeningParty.ListenersWithoutSong.Add(user.OnlineUser);
            }

            if (!ListeningParty.ListenerIdsWithoutSong.Contains(e.UserId))
                ListeningParty.ListenerIdsWithoutSong.Add(e.UserId);

            Logger.Important($"{e.UserId} does not have the active listening party song", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnListeningPartyUserHasSong(object sender, ListeningPartyUserHasSongEventArgs e)
        {
            if (ListeningParty == null)
                return;

            if (OnlineUsers.ContainsKey(e.UserId))
            {
                var user = OnlineUsers[e.UserId];
                ListeningParty.ListenersWithoutSong.Remove(user.OnlineUser);
            }

            ListeningParty.ListenerIdsWithoutSong.Remove(e.UserId);
            Logger.Important($"{e.UserId} has the active listening party song", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserFriendsListReceieved(object sender, UserFriendsListEventArgs e)
        {
            lock (FriendsList)
                FriendsList = e.Friends;

            Logger.Important($"Received friends list containing {FriendsList.Count} users.", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSongRequestReceived(object sender, SongRequestEventArgs e)
        {
            SongRequests.Add(e.Request);

            Logger.Important($"Received song request: {e.Request.TwitchUsername} ({e.Request.UserId}) | " +
                             $"{e.Request.Artist} - {e.Request.Title}", LogType.Runtime);

            if (!ConfigManager.DisplaySongRequestNotifications.Value)
                return;

            var game = (QuaverGame) GameBase.Game;

            if (game.OnlineHub.IsOpen)
            {
                if (game.OnlineHub.Sections[OnlineHubSectionType.SongRequests] != game.OnlineHub.SelectedSection)
                    game.OnlineHub.MarkSectionAsUnread(OnlineHubSectionType.SongRequests);

                return;
            }

            NotificationManager.Show(NotificationLevel.Info, $"You have received a new song request. Click here to view it!",
                (o, args) =>
                {
                    game.OnlineHub.SelectSection(OnlineHubSectionType.SongRequests);

                    if (game.OnlineHub.IsOpen)
                        return;

                    DialogManager.Show(new OnlineHubDialog());
                });

            game.OnlineHub.MarkSectionAsUnread(OnlineHubSectionType.SongRequests);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnTwitchConnectionReceived(object sender, TwitchConnectionEventArgs e)
        {
            TwitchUsername = e.Connected ? e.TwitchUsername : null;
            Logger.Important($"Received Twitch Connection Status: Connected: {e.Connected} | Username: {e.TwitchUsername}", LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameMapsetShared(object sender, GameMapsetSharedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.IsMapsetShared = e.IsShared;

            Logger.Important($"Received multiplayer game mapset shared status: {CurrentGame.IsMapsetShared}",
                LogType.Runtime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameCountdownStopped(object sender, StopCountdownEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.CountdownStartTime = -1;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnGameCountdownStarted(object sender, StartCountdownEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.CountdownStartTime = e.TimeStarted;
        }

        private static void OnTournamentModeChanged(object sender, TournamentModeEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.TournamentMode = e.TournamentMode;
            Logger.Debug($"Tournament Mode Updated: {CurrentGame.TournamentMode}", LogType.Network, false);
        }

        private static void OnNeedsDifficultyRatings(object sender, GameNeedDifficultyRatingsEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.NeedsDifficultyRatings = e.Needs;
            
            if (CurrentGame.NeedsDifficultyRatings)
                SendGameDifficultyRatings(e.Md5, e.AlternativeMd5);
            
            Logger.Debug($"Game Needs Difficulty Ratings: {CurrentGame.NeedsDifficultyRatings}", LogType.Runtime);
        }
        
        private static void OnAutoHostChanged(object sender, AutoHostChangedEventArgs e)
        {
            if (CurrentGame == null)
                return;

            CurrentGame.IsAutoHost = e.Enabled;
            Logger.Debug($"AutoHost has been changed to: {CurrentGame.IsAutoHost}", LogType.Network);
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

        public static void SendGameDifficultyRatings(string md5, string alternativeMd5)
        {
            if (CurrentGame == null)
                return;

            var map = MapManager.FindMapFromMd5(md5) ?? MapManager.FindMapFromMd5(alternativeMd5);

            if (map == null)
                return;

            if (map.DifficultyProcessorVersion != DifficultyProcessorKeys.Version)
                return;
            
            Client?.SendGameDifficultyRatings(map.Md5Checksum, map.GetAlternativeMd5(), map.GetDifficultyRatings());
            CurrentGame.NeedsDifficultyRatings = false;
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
        /// <param name="game"></param>
        /// <returns></returns>
        public static ModIdentifier GetUserActivatedMods(int userId, MultiplayerGame game = null)
        {
            game = game ?? CurrentGame;

            if (game == null)
                return 0;

            var currMods = (ModIdentifier) long.Parse(game.Modifiers ?? "0");

            if (currMods < 0)
                currMods = 0;

            var playerMods = game.PlayerMods.Find(x => x.UserId == userId);

            if (playerMods != null)
            {
                var pm =  (ModIdentifier) long.Parse(playerMods.Modifiers ?? "0");

                if (pm < 0)
                    pm = 0;

                currMods |= pm;
            }

            return currMods;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static ModIdentifier GetSelfActivatedMods() => GetUserActivatedMods(Self.OnlineUser.Id);

        /// <summary>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static MultiplayerTeam GetTeam(int userId, MultiplayerGame game = null)
        {
            game = game ?? CurrentGame;

            if (game == null || game .Ruleset != MultiplayerGameRuleset.Team)
                return MultiplayerTeam.Red;

            if (game .RedTeamPlayers.Contains(userId))
                return MultiplayerTeam.Red;

            if (game .BlueTeamPlayers.Contains(userId))
                return MultiplayerTeam.Blue;

            return MultiplayerTeam.Red;
        }

        /// <summary>
        /// </summary>
        /// <param name="action"></param>
        public static void UpdateListeningPartyState(ListeningPartyAction action)
        {
            if (ListeningParty == null || ListeningParty.Host != Self.OnlineUser)
                return;

            var map = MapManager.Selected.Value;
            var unix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            Client?.UpdateListeningPartyState(action, map.Md5Checksum, map.MapId, unix, AudioEngine.Track.Time,
                AudioEngine.Track.IsPaused, map.Artist, map.Title);
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        public static void AddFriend(User user)
        {
            if (FriendsList == null)
                return;

            lock (FriendsList)
            {
                if (!FriendsList.Contains(user.OnlineUser.Id))
                    FriendsList.Add(user.OnlineUser.Id);

                Client?.AddFriend(user.OnlineUser.Id);

                FriendsListUserChanged?.Invoke(typeof(OnlineManager),
                    new FriendsListUserChangedEventArgs(FriendsListAction.Add, user.OnlineUser.Id));

                Logger.Important($"{user.OnlineUser.Id} has been added to our friends list.",LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Success, $"{user.OnlineUser.Username} has been added to your friends list!");
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        public static void RemoveFriend(User user)
        {
            if (FriendsList == null)
                return;

            lock (FriendsList)
            {
                if (FriendsList.Contains(user.OnlineUser.Id))
                    FriendsList.Remove(user.OnlineUser.Id);

                Client?.RemoveFriend(user.OnlineUser.Id);

                FriendsListUserChanged?.Invoke(typeof(OnlineManager),
                    new FriendsListUserChangedEventArgs(FriendsListAction.Remove, user.OnlineUser.Id));

                Logger.Important($"{user.OnlineUser.Id} has been removed from our friends list.", LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Success, $"{user.OnlineUser.Username} has been removed from your friends list!");
            }
        }

        /// <summary>
        ///     Returns a list of empty scores to represent each multiplayer user
        /// </summary>
        /// <returns></returns>
        private static List<Score> GetScoresFromMultiplayerUsers()
        {
            var users = OnlineUsers.ToList();

            var playingUsers = users.FindAll(x =>
                CurrentGame.PlayerIds.Contains(x.Value.OnlineUser.Id) &&
                !CurrentGame.PlayersWithoutMap.Contains(x.Value.OnlineUser.Id) &&
                CurrentGame.RefereeUserId != x.Value.OnlineUser.Id &&
                x.Value != Self);

            var scores = new List<Score>();

            playingUsers.ForEach(x =>
            {
                scores.Add(new Score
                {
                    PlayerId = x.Key,
                    SteamId = x.Value.OnlineUser.SteamId,
                    Name = x.Value.OnlineUser.Username,
                    Mods = (long) GetUserActivatedMods(x.Value.OnlineUser.Id),
                    IsMultiplayer = true,
                    IsOnline = true
                });
            });

            ScoresHelper.SetRatingProcessors(scores);
            return scores;
        }
    }
}