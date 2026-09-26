using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.BlockedUsers;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Overlays.Chatting;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens;
using Quaver.Shared.Screens.V2.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.V2Hub.Users;

public sealed class UserRightClickOptions : V2Dropdown<UserMenuAction>
{
    private User User { get; }

    public UserRightClickOptions(User user, float width, SkinV2DropdownConfig config, Container overlayHost) : base(width, GetOptions(user), FontManager.GetWobbleFont(Fonts.InterBold), config, overlayHost)
    {
        User = user;
        OptionSelected += OnOptionSelected;
    }

    private static IReadOnlyList<DropdownEntry<UserMenuAction>> GetOptions(User user)
    {
        var options = new List<DropdownEntry<UserMenuAction>>
        {
            new DropdownOption<UserMenuAction>(UserMenuAction.ViewProfile, LocalizationManager.Get("Screen_Multiplayer_ViewProfile"), Color.White)
        };

        if (!user.HasUserInfo)
            return options;

        if (user.OnlineUser.SteamId > 0)
            options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.SteamProfile, LocalizationManager.Get("Screen_Multiplayer_SteamProfile"), ColorHelper.HexToColor("#0787E3")));

        if (!string.IsNullOrEmpty(user.OnlineUser.ClanId))
            options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.ViewClan, LocalizationManager.Get("Screen_Hub_ViewClan"), Color.Beige));

        if (OnlineManager.Self?.OnlineUser?.Id == user.OnlineUser.Id)
        {
            if (OnlineManager.CurrentGame?.Ruleset == MultiplayerGameRuleset.Team) options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.SwitchTeams, LocalizationManager.Get("Screen_Multi_SwitchTeams"), ColorHelper.HexToColor("#F2994A")));
            return options;
        }

        var isFriend = OnlineManager.FriendsList?.Contains(user.OnlineUser.Id) == true;
        options.Add(new DropdownOption<UserMenuAction>(isFriend ? UserMenuAction.RemoveFriend : UserMenuAction.AddFriend, LocalizationManager.Get(isFriend ? "Screen_Hub_RemoveFriend" : "Screen_Hub_AddFriend"), isFriend ? ColorHelper.HexToColor("#FF6868") : ColorHelper.HexToColor("#27B06E")));

        if (!user.OnlineUser.UserGroups.HasFlag(UserGroups.Bot) && !user.OnlineUser.UserGroups.HasFlag(UserGroups.Developer))
        {
            var blocked = BlockedUsers.IsUserBlocked(user.OnlineUser.Id);
            options.Add(new DropdownOption<UserMenuAction>(blocked ? UserMenuAction.UnblockUser : UserMenuAction.BlockUser, LocalizationManager.Get(blocked ? "Screen_Hub_UnblockUser" : "Screen_Hub_BlockUser"), ColorHelper.HexToColor("#FF6868")));
        }

        var game = OnlineManager.CurrentGame;
        if (game != null && game.Type == MultiplayerGameType.Friendly && game.Players.Count < game.MaxPlayers && !game.PlayerIds.Contains(user.OnlineUser.Id))
            options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.InviteToGame, LocalizationManager.Get("Screen_Hub_InviteToGame"), ColorHelper.HexToColor("#9B51E0")));

        options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.Chat, LocalizationManager.Get("Screen_Main_Menu_Chat"), ColorHelper.HexToColor("#b48bff")));

        if (OnlineManager.SpectatorClients != null)
        {
            var spectating = OnlineManager.SpectatorClients.ContainsKey(user.OnlineUser.Id);
            options.Add(new DropdownOption<UserMenuAction>(spectating ? UserMenuAction.StopSpectating : UserMenuAction.Spectate, LocalizationManager.Get(spectating ? "Screen_Hub_StopSpectating" : "Screen_Hub_Spectate"), ColorHelper.HexToColor("#0FBAE5")));
        }

        if (game != null && game.HostId == OnlineManager.Self?.OnlineUser?.Id && game.PlayerIds.Contains(user.OnlineUser.Id))
        {
            options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.Kick, LocalizationManager.Get("Screen_Multiplayer_KickPlayer"), ColorHelper.HexToColor("#FF6868")));
            options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.GiveHost, LocalizationManager.Get("Screen_Multiplayer_GiveHost"), ColorHelper.HexToColor("#27B06E")));
            if (game.Ruleset == MultiplayerGameRuleset.Team)
                options.Add(new DropdownOption<UserMenuAction>(UserMenuAction.SwitchTeams, LocalizationManager.Get("Screen_Multi_SwitchTeams"), ColorHelper.HexToColor("#F2994A")));
        }

        return options;
    }

    private void OnOptionSelected(object sender, DropdownOptionEventArgs<UserMenuAction> e)
    {
        var user = User;
        var id = user.OnlineUser.Id;

        if (e.Option.Value != UserMenuAction.ViewProfile &&
            e.Option.Value != UserMenuAction.ViewClan &&
            e.Option.Value != UserMenuAction.SteamProfile &&
            !OnlineManager.OnlineUsers.TryGetValue(id, out user))
            return;

        switch (e.Option.Value)
        {
            case UserMenuAction.ViewProfile:
                BrowserHelper.OpenURL($"https://quavergame.com/profile/{id}");
                break;
            case UserMenuAction.ViewClan:
                BrowserHelper.OpenURL($"https://two.quavergame.com/clans/{user.OnlineUser.ClanId}");
                break;
            case UserMenuAction.SteamProfile:
                BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{user.OnlineUser.SteamId}");
                break;
            case UserMenuAction.AddFriend:
                OnlineManager.AddFriend(user);
                break;
            case UserMenuAction.RemoveFriend:
                OnlineManager.RemoveFriend(user);
                break;
            case UserMenuAction.BlockUser:
                BlockedUsers.Block(id, user.OnlineUser.Username);
                break;
            case UserMenuAction.UnblockUser:
                BlockedUsers.Unblock(id, user.OnlineUser.Username);
                break;
            case UserMenuAction.InviteToGame:
                OnlineManager.Client?.InviteToGame(id);
                NotificationManager.Show(NotificationLevel.Success, LocalizationManager.Get("Screen_Hub_InviteSent", user.OnlineUser.Username ?? id.ToString()));
                break;
            case UserMenuAction.Chat:
                OpenChat(user);
                break;
            case UserMenuAction.Spectate:
                Spectate(user);
                break;
            case UserMenuAction.StopSpectating:
                OnlineManager.Client?.StopSpectating();
                break;
            case UserMenuAction.Kick:
                OnlineManager.Client?.KickMultiplayerGamePlayer(id);
                break;
            case UserMenuAction.SwitchTeams:
                var team = OnlineManager.GetTeam(id) == MultiplayerTeam.Red
                    ? MultiplayerTeam.Blue
                    : MultiplayerTeam.Red;
                if (user == OnlineManager.Self)
                    OnlineManager.Client?.ChangeGameTeam(team);
                else
                    OnlineManager.Client?.ChangeOtherPlayerTeam(id, team);
                break;
            case UserMenuAction.GiveHost:
                OnlineManager.Client?.TransferMultiplayerGameHost(id);
                break;
        }
    }

    private static void OpenChat(User user)
    {
        var channel = OnlineChat.JoinedChatChannels.Find(x => x.Name == user.OnlineUser.Username);
        if (channel != null)
        {
            OnlineChat.Instance.ActiveChannel.Value = channel;
            return;
        }

        var privateChat = new ChatChannel
        {
            Name = user.OnlineUser.Username,
            AllowedUserGroups = UserGroups.Normal,
            Description = LocalizationManager.Get("Screen_Hub_PrivateChat")
        };
        OnlineChat.Instance.ChannelList.ChannelContainer.Add(privateChat);
        OnlineChat.Instance.MessageContainer.AddChannel(privateChat);
        OnlineChat.Instance.ActiveChannel.Value = privateChat;
    }

    private static void Spectate(User user)
    {
        var game = GameBase.Game as QuaverGame;
        switch (game?.CurrentScreen?.Type)
        {
            case QuaverScreenType.Menu:
            case QuaverScreenType.Results:
            case QuaverScreenType.Select:
            case QuaverScreenType.Importing:
            case QuaverScreenType.Download:
                OnlineManager.Client?.StopSpectating();
                OnlineManager.Client?.SpectatePlayer(user.OnlineUser.Id);
                break;
            default:
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Hub_CannotSpectateNow"));
                break;
        }
    }
    
    public override void Destroy()
    {
        OptionSelected -= OnOptionSelected;
        base.Destroy();
    }
}

public enum UserMenuAction
{
    ViewProfile,
    ViewClan,
    SteamProfile,
    AddFriend,
    RemoveFriend,
    BlockUser,
    UnblockUser,
    InviteToGame,
    Chat,
    Spectate,
    StopSpectating,
    Kick,
    SwitchTeams,
    GiveHost
}
