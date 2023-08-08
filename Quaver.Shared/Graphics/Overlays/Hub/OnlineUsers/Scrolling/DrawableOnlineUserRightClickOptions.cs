using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens;
using Wobble;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling
{
    public class DrawableOnlineUserRightClickOptions : RightClickOptions
    {
        private const string ViewProfile = "View Profile";

        private const string SteamProfile = "Steam Profile";

        private const string AddFriend = "Add Friend";

        private const string RemoveFriend = "Remove Friend";

        // private const string JoinListeningParty = "Join Listening Party";

        private const string InviteToGame = "Invite To Game";

        private const string Chat = "Chat";

        private const string Spectate = "Spectate";

        private const string StopSpectating = "Stop Spectating";

        private const string Kick = "Kick Player";

        private const string SwitchTeams = "Switch Team";

        private const string GiveHost = "Give Host";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DrawableOnlineUserRightClickOptions(User user) : base(GetOptions(user), new ScalableVector2(200, 40), 22)
        {
            ItemSelected += (sender, args) =>
            {
                switch (args.Text)
                {
                    case ViewProfile:
                        BrowserHelper.OpenURL($"https://quavergame.com/profile/{user.OnlineUser.Id}");
                        break;
                    case SteamProfile:
                        BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{user.OnlineUser?.SteamId}");
                        break;
                    case AddFriend:
                        OnlineManager.AddFriend(user);
                        break;
                    case RemoveFriend:
                        OnlineManager.RemoveFriend(user);
                        break;
                    // case JoinListeningParty:
                    //     HandleJoinListeningParty(user);
                    //     break;
                    case InviteToGame:
                        HandleInviteToGame(user);
                        break;
                    case Chat:
                        HandleOpenChat(user);
                        break;
                    case Spectate:
                        HandleSpectating(user);
                        break;
                    case StopSpectating:
                        OnlineManager.Client?.StopSpectating();
                        break;
                    case Kick:
                        OnlineManager.Client?.KickMultiplayerGamePlayer(user.OnlineUser.Id);
                        break;
                    case SwitchTeams:
                        var team = OnlineManager.GetTeam(user.OnlineUser.Id) == MultiplayerTeam.Red
                            ? MultiplayerTeam.Blue
                            : MultiplayerTeam.Red;

                        if (user == OnlineManager.Self)
                            OnlineManager.Client?.ChangeGameTeam(team);
                        else
                            OnlineManager.Client.ChangeOtherPlayerTeam(user.OnlineUser.Id, team);
                        break;
                    case GiveHost:
                        OnlineManager.Client?.TransferMultiplayerGameHost(user.OnlineUser.Id);
                        break;
                }
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, Color> GetOptions(User user)
        {
            var options = new Dictionary<string, Color>()
            {
                {ViewProfile, Color.White},
                {SteamProfile, ColorHelper.HexToColor("#0787E3")},
            };

            // Don't add actions meant for other users
            if (OnlineManager.Self?.OnlineUser?.Id == user.OnlineUser.Id)
            {
                if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                    options.Add(SwitchTeams, ColorHelper.HexToColor("#F2994A"));

                return options;
            }

            // Friends List
            if (OnlineManager.FriendsList != null && OnlineManager.FriendsList.Contains(user.OnlineUser.Id))
                options.Add(RemoveFriend, ColorHelper.HexToColor($"#FF6868"));
            else
                options.Add(AddFriend, ColorHelper.HexToColor("#27B06E"));

            // Invite To Multiplayer
            if (OnlineManager.CurrentGame != null
                && OnlineManager.CurrentGame.Type == MultiplayerGameType.Friendly
                && OnlineManager.CurrentGame.Players.Count < OnlineManager.CurrentGame.MaxPlayers
                && !OnlineManager.CurrentGame.PlayerIds.Contains(user.OnlineUser.Id))
            {
                options.Add(InviteToGame, ColorHelper.HexToColor("#9B51E0"));
            }

            // // Join Listening Party
            // if (user.CurrentStatus.Status == ClientStatus.Listening)
            //     options.Add(JoinListeningParty, ColorHelper.HexToColor("#F2994A"));

            // Chat
            options.Add(Chat, ColorHelper.HexToColor("#b48bff"));

            if (OnlineManager.SpectatorClients != null)
            {
                var color = ColorHelper.HexToColor("#0FBAE5");
                options.Add(OnlineManager.SpectatorClients.ContainsKey(user.OnlineUser.Id) ? StopSpectating : Spectate, color);
            }

            // Host of the multiplayer game so add more options
            if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.HostId == OnlineManager.Self?.OnlineUser?.Id
                && OnlineManager.CurrentGame.PlayerIds.Contains(user.OnlineUser.Id))
            {
                if (user != OnlineManager.Self)
                {
                    options.Add(Kick, ColorHelper.HexToColor($"#FF6868"));
                    options.Add(GiveHost, ColorHelper.HexToColor("#27B06E"));
                }

                if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                    options.Add(SwitchTeams, ColorHelper.HexToColor("#F2994A"));
            }

            return options;
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        private void HandleJoinListeningParty(User user)
        {
            var game = (QuaverGame) GameBase.Game;

            // Don't allow if currently in a game
            if (OnlineManager.CurrentGame != null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must leave your current game before joining " +
                                                                  "a listening party");
                return;
            }

            // Don't allow if currently in a game
            if (OnlineManager.ListeningParty != null)
            {
                NotificationManager.Show(NotificationLevel.Error, "You must leave your current listening party " +
                                                                  "before joinnig another.");
                return;
            }

            // Don't allow if on certain screens
            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Editor:
                case QuaverScreenType.Gameplay:
                case QuaverScreenType.Loading:
                case QuaverScreenType.Importing:
                case QuaverScreenType.Multiplayer:
                    NotificationManager.Show(NotificationLevel.Error, "You must exit the current screen before " +
                                                                      "joining a listening party.");
                    return;
            }

            OnlineManager.Client.JoinListeningParty(user.OnlineUser.Id);
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        private void HandleInviteToGame(User user)
        {
            OnlineManager.Client?.InviteToGame(user.OnlineUser.Id);

            NotificationManager.Show(NotificationLevel.Success, $"Successfully invited {user.OnlineUser.Username ?? "user"} " +
                                                                $"to the game!");
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        private void HandleOpenChat(User user)
        {
            var chan = OnlineChat.JoinedChatChannels.Find(x => x.Name == user.OnlineUser.Username);

            if (chan != null)
            {
                OnlineChat.Instance.ActiveChannel.Value = chan;
                return;
            }

            var privateChat = new ChatChannel()
            {
                Name = user.OnlineUser.Username,
                AllowedUserGroups = UserGroups.Normal,
                Description = "Private Chat"
            };

            OnlineChat.Instance.ChannelList.ChannelContainer.Add(privateChat);
            OnlineChat.Instance.MessageContainer.AddChannel(privateChat);
            OnlineChat.Instance.ActiveChannel.Value = privateChat;
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        private void HandleSpectating(User user)
        {
            var game = (QuaverGame) GameBase.Game;

            switch (game.CurrentScreen.Type)
            {
                case QuaverScreenType.Menu:
                case QuaverScreenType.Results:
                case QuaverScreenType.Select:
                case QuaverScreenType.Importing:
                case QuaverScreenType.Alpha:
                case QuaverScreenType.Download:
                    OnlineManager.Client?.StopSpectating();
                    OnlineManager.Client?.SpectatePlayer(user.OnlineUser.Id);
                    break;
                default:
                    NotificationManager.Show(NotificationLevel.Error, "Please finish what you're doing before spectating!");
                    break;
            }
        }
    }
}