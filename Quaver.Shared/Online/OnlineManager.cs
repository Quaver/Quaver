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
using Quaver.Server.Common.Packets.Server;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Online.Username;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Select;
using Steamworks;
using UniversalThreadManagement;
using Wobble;
using Wobble.Bindables;
using Wobble.Discord;
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
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            if (Status.Value != ConnectionStatus.Disconnected)
                return;

            Logger.Important($"Attempting to log into the Quaver server...", LogType.Network);
            NotificationManager.Show(NotificationLevel.Error, "You can only log into the server on the official Steam build.");

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
        }

        /// <summary>
        ///     Called when the connection status of the user has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e) => Status.Value = e.Status;

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
                    NotificationManager.Show(NotificationLevel.Error, "Failed to authenticate user to the server.");
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

            // Request to join chats if we are already in them upon logging in (for reconnections)
            foreach (var chan in ChatManager.JoinedChatChannels)
                Client?.JoinChatChannel(chan.Name);

            lock (OnlineUsers)
            {
                OnlineUsers.Clear();
                OnlineUsers[e.Self.OnlineUser.Id] = e.Self;
            }

            // Make sure the config username is changed.
            ConfigManager.Username.Value = Self.OnlineUser.Username;

            DiscordHelper.Presence = new DiscordRpc.RichPresence
            {
                LargeImageKey = "quaver",
                LargeImageText = GetRichPresenceLargeKeyText(GameMode.Keys4),
                EndTimestamp = 0
            };

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

            DiscordHelper.Presence = new DiscordRpc.RichPresence
            {
                LargeImageKey = "quaver",
                LargeImageText = GetRichPresenceLargeKeyText(e.Response.GameMode),
                EndTimestamp = 0
            };

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
            foreach (var user in e.Users)
            {
                OnlineUsers[user.Id] = new User(user);
                ChatManager.Dialog.OnlineUserList?.UpdateUserInfo(OnlineUsers[user.Id]);
            }
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
                onlineUser.CurrentStatus = user.Value;

                ChatManager.Dialog.OnlineUserList?.UpdateUserInfo(onlineUser);
            }
        }
    }
}
