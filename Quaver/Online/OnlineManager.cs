using System;
using System.Collections.Generic;
using System.Linq;
using Amib.Threading;
using Quaver.API.Enums;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Notifications;
using Quaver.Logging;
using Quaver.Online.Chat;
using Quaver.Scheduling;
using Quaver.Server.Client;
using Quaver.Server.Client.Events.Disconnnection;
using Quaver.Server.Client.Events.Login;
using Quaver.Server.Client.Events.Scores;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Packets.Server;
using Steamworks;
using WebSocketSharp;
using Logger = Quaver.Logging.Logger;

namespace Quaver.Online
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

                _client?.Disconnect();
                _client = value;
            }
        }

        /// <summary>
        ///     If we're currently connected to the server.
        /// </summary>
        public static bool Connected => Client?.Socket != null;

        /// <summary>
        ///     The user client for self (the current client.)
        /// </summary>
        public static User Self { get; private set; }

        /// <summary>
        ///     Dictionary containing all of the currently online users.
        /// </summary>
        public static Dictionary<int, User> OnlineUsers { get; private set; }

        /// <summary>
        ///     The thread that online actions will take place on.
        /// </summary>
        private static SmartThreadPool Thread { get; } = new SmartThreadPool();

        /// <summary>
        ///     Logs into the Quaver server.
        /// </summary>
        public static void Login()
        {
            Logger.LogImportant($"Attempting to log into the Quaver server...", LogType.Network);

            if (!SteamManager.AuthSessionTicketValidated)
            {
                Logger.LogError($"Could not log in because the steam auth session ticket was not validated.", LogType.Network);
                throw new Exception("Failed to login");
            }

            // Create the new online client and subscribe to all of its online events.
            Client = new OnlineClient();
            SubscribeToEvents();

            // Initiate the connection to the game server.
            Thread.QueueWorkItem(() => Client.Connect(SteamUser.GetSteamID().m_SteamID, SteamFriends.GetPersonaName(), SteamManager.PTicket, SteamManager.PcbTicket));
        }

        /// <summary>
        ///     Subscribes to all events
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private static void SubscribeToEvents()
        {
            if (Client == null)
                throw new InvalidOperationException("Cannot subscribe to events if there is no OnlineClient.");

            Client.OnLoginFailed += OnLoginFailed;
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
        }

        /// <summary>
        ///     When login to the server fails, this event handler will be called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLoginFailed(object sender, FailureToLoginEventArgs e)
        {
            switch (e.ResponseCode)
            {
                case -100:
                    NotificationManager.Show(NotificationLevel.Error, "No response from the server.");
                    break;
                case 401:
                    NotificationManager.Show(NotificationLevel.Error, "You are banned.");
                    break;
                case 400:
                    NotificationManager.Show(NotificationLevel.Error, "An issue has occurred during the login process.");
                    break;
            }

            Logger.LogError(e.Error, LogType.Network);
        }

        /// <summary>
        ///     Called when the client receives a response after selecting a username.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnChooseAUsernameResponse(object sender, ChooseAUsernameResponseEventArgs e)
        {
            Logger.LogImportant(e.Message, LogType.Network);

            switch (e.Status)
            {
                // Success
                case 200:
                    NotificationManager.Show(NotificationLevel.Success, "Account has successfully been created!");
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
            Client = null;

            Logger.LogImportant($"Disconnected from the server for reason: {e.CloseEventArgs.Reason} with code: {e.CloseEventArgs.Code}", LogType.Network);

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

            NotificationManager.Show(NotificationLevel.Info, "Disconnected from the server.");
        }

        /// <summary>
        ///     When the client successfully logs into the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            Self = e.Self;
            ChatManager.MuteTimeLeft = Self.MuteTimeEnd - (long) TimeHelper.GetUnixTimestampMilliseconds();

            OnlineUsers.Add(e.Self.Id, e.Self);
            NotificationManager.Show(NotificationLevel.Success, $"Successfully logged in as: {Self.Username}");

            // Make sure the config username is changed.
            ConfigManager.Username.Value = Self.Username;

            Console.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
        }

        /// <summary>
        ///     Called when a user connects to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserConnected(object sender, UserConnectedEventArgs e)
        {
            if (OnlineUsers.ContainsKey(e.User.Id))
                return;

            OnlineUsers.Add(e.User.Id, e.User);

            Console.WriteLine($"User: {e.User.Username} [{e.User.SteamId}] (#{e.User.Id}) has connected to the server.");
            Console.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
        }

        /// <summary>
        ///     Called when a user disconnects from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
        {
            if (OnlineUsers.ContainsKey(e.UserId))
                OnlineUsers.Remove(e.UserId);

            Console.WriteLine($"User: #{e.UserId} has disconnected from the server.");
            Console.WriteLine($"There are currently: {OnlineUsers.Count} users online.");
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
            var mapsets = MapManager.Mapsets.Where(x => x.Maps.Any(y => y.MapId == e.Id && y.Md5Checksum == e.Md5)).ToList();

            if (mapsets.Count == 0)
                return;

            var map = mapsets.First().Maps.Find(x => x.MapId == e.Id && x.Md5Checksum == e.Md5);

            switch (e.Code)
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

            Logger.LogInfo($"Retrieved Scores/Status For Map: {map.Md5Checksum} ({map.MapId}) - {map.RankedStatus}", LogType.Network);
        }
    }
}