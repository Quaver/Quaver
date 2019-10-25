using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form.Dropdowns.RightClick;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Graphics;

namespace Quaver.Shared.Graphics.Overlays.Hub.OnlineUsers.Scrolling
{
    public class DrawableOnlineUserRightClickOptions : RightClickOptions
    {
        private const string ViewProfile = "View Profile";

        private const string SteamProfile = "Steam Profile";

        private const string AddFriend = "Add Friend";

        private const string RemoveFriend = "Remove Friend";

        private const string JoinListeningParty = "Join Listening Party";

        private const string InviteToGame = "Invite To Game";

        private const string Chat = "Chat";

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
                    case JoinListeningParty:
                        NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet!");
                        break;
                    case InviteToGame:
                        NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet!");
                        break;
                    case Chat:
                        NotificationManager.Show(NotificationLevel.Warning, "Not implemented yet!");
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
            if (OnlineManager.Self.OnlineUser.Id == user.OnlineUser.Id)
                return options;

            // Friends List
            if (OnlineManager.FriendsList.Contains(user.OnlineUser.Id))
                options.Add(RemoveFriend, ColorHelper.HexToColor($"#FF6868"));
            else
                options.Add(AddFriend, ColorHelper.HexToColor("#27B06E"));

            if (OnlineManager.CurrentGame != null && OnlineManager.CurrentGame.Type == MultiplayerGameType.Friendly)
                options.Add(InviteToGame, ColorHelper.HexToColor("#9B51E0"));

            if (user.CurrentStatus.Status == ClientStatus.Listening)
                options.Add(JoinListeningParty, ColorHelper.HexToColor("#F2994A"));

            return options;
        }
    }
}